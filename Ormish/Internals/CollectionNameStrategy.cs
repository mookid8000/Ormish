using System;

namespace Ormish.Internals
{
    class CollectionNameStrategy
    {
        public string GetName(Type type)
        {
            return type.Name;
        }
    }
}