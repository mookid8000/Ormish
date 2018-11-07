using MongoDB.Driver;
using Testy;

namespace Ormish.Tests
{
    public abstract class MongoFixtureBase : FixtureBase
    {
        protected static readonly IMongoDatabase MongoDatabase;
        protected static readonly MongoUrl MongoUrl;

        static MongoFixtureBase()
        {
            MongoUrl = new MongoUrl("mongodb://localhost/ormish");
            MongoDatabase = new MongoClient(MongoUrl).GetDatabase(MongoUrl.DatabaseName);
        }

        protected sealed override void SetUp()
        {
            MongoDatabase.Client.DropDatabase(MongoUrl.DatabaseName);

            AdditionalSetUp();
        }

        protected virtual void AdditionalSetUp() { }
    }
}