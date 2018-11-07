using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FastMember;
using MongoDB.Bson;
using MongoDB.Driver;
using Ormish.Internals;

namespace Ormish
{
    public class OrmishSession : IDisposable
    {
        static readonly ConcurrentDictionary<Type, TypeAccessor> TypeAccessors = new ConcurrentDictionary<Type, TypeAccessor>();
        readonly CollectionNameStrategy _collectionNameStrategy;
        readonly IdentityMap _identityMap;
        readonly IMongoDatabase _database;

        volatile IClientSessionHandle currentSession;

        public OrmishSession(IMongoDatabase database)
        {
            _collectionNameStrategy = new CollectionNameStrategy();
            _identityMap = new IdentityMap(_collectionNameStrategy);
            _database = database;
        }

        public async Task<TDocument> Load<TDocument>(string id) where TDocument : class
        {
            if (_identityMap.Get<TDocument>(id) != null)
            {
                return _identityMap.Get<TDocument>(id);
            }

            var entity = await InnerLoad<TDocument>(id);
            _identityMap.Set(id, entity);
            return entity;
        }

        public async Task<string> Save<TDocument>(TDocument document) where TDocument : class
        {
            var id = GetId(document);

            _identityMap.Set(id, document);

            return id;
        }

        public async Task SaveChanges()
        {
        }

        async Task<TDocument> InnerLoad<TDocument>(string id) where TDocument : class
        {
            var name = _collectionNameStrategy.GetName(typeof(TDocument));
            var filterDefinition = new BsonDocumentFilterDefinition<TDocument>(new BsonDocument { { "Id", id } });

            var session = await GetCurrentSession();
            
            using (var cursor = await _database.GetCollection<TDocument>(name).FindAsync(session, filterDefinition))
            {
                var entity = await cursor.FirstOrDefaultAsync();
                
                return entity;
            }
        }

        async Task<IClientSessionHandle> GetCurrentSession()
        {
            if (currentSession != null) return currentSession;

            lock (this)
            {
                if (currentSession != null)
                {
                    return currentSession;
                }

                var session = _database.Client.StartSession();

                session.StartTransaction();

                return currentSession = session;
            }
        }

        static string GetId<TDocument>(TDocument document)
        {
            var documentType = typeof(TDocument);
            var typeAccessor = TypeAccessors.GetOrAdd(documentType, _ => TypeAccessor.Create(documentType));

            object GetId(TDocument doc)
            {
                try
                {
                    return typeAccessor[doc, "Id"];
                }
                catch (Exception exception)
                {
                    throw new ArgumentException($"Could not get value from 'Id' property of {doc}", exception);
                }
            }

            var value = GetId(document);

            if (ReferenceEquals(null, value))
            {
                var newId = Guid.NewGuid().ToString();

                try
                {
                    typeAccessor[document, "Id"] = newId;
                }
                catch (Exception exception)
                {
                    throw new ArgumentException($"Could not assign ID {newId} to Id property of document of type {documentType}", exception);
                }

                return newId;
            }

            return value as string
                   ?? throw new ArgumentException($"Could not get ID {value} of type {value?.GetType()} as string from document of");
        }

        public void Dispose()
        {
        }
    }
}
