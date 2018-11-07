using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Ormish.Stuff
{
    public class OrmishSession : IDisposable
    {
        readonly CollectionNameStrategy _collectionNameStrategy;
        readonly IdentityMap _identityMap;
        readonly IMongoDatabase _database;

        public OrmishSession(IMongoDatabase database)
        {
            _collectionNameStrategy = new CollectionNameStrategy();
            _identityMap = new IdentityMap(_collectionNameStrategy);
            _database = database;
        }

        public async Task<TDocument> Load<TDocument>(string id) where TDocument : class
        {
            return _identityMap.Get<TDocument>(id)
                   ?? await InnerLoad<TDocument>(id);
        }

        async Task<TDocument> InnerLoad<TDocument>(string id) where TDocument : class
        {
            return null;
        }

        public async Task<string> Save<TDocument>(TDocument document)
        {
            return "";
        }

        public void Dispose()
        {
        }
    }
}
