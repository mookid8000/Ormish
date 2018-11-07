using System;
using System.Collections.Concurrent;

namespace Ormish.Internals
{
    class IdentityMap
    {
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _entities = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
        readonly CollectionNameStrategy _collectionNameStrategy;

        public IdentityMap(CollectionNameStrategy collectionNameStrategy)
        {
            _collectionNameStrategy = collectionNameStrategy;
        }

        public TDocument Get<TDocument>(string id) where TDocument : class
        {
            var documentType = typeof(TDocument);
            var name = _collectionNameStrategy.GetName(documentType);
            var mapForThisType = _entities.GetOrAdd(name, _ => new ConcurrentDictionary<string, object>());

            if (!mapForThisType.TryGetValue(id, out var instance)) return null;

            try
            {
                return (TDocument)instance;
            }
            catch (Exception exception)
            {
                throw new InvalidCastException($"Could not return instance with ID {id} from collection {name} as {documentType}, because its type is {instance.GetType()}", exception);
            }
        }

        public void Set<TDocument>(string id, TDocument document) where TDocument : class
        {
            var documentType = typeof(TDocument);
            var name = _collectionNameStrategy.GetName(documentType);
            var mapForThisType = _entities.GetOrAdd(name, _ => new ConcurrentDictionary<string, object>());
            
            mapForThisType[id] = document;
        }
    }
}