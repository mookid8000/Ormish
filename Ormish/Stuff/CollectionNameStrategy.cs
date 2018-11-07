using System;

namespace Ormish.Stuff
{
    class CollectionNameStrategy
    {
        public string GetName(Type type)
        {
            return type.Name;
        }
    }
}