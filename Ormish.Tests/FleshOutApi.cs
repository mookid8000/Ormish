using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Ormish.Stuff;

namespace Ormish.Tests
{
    [TestFixture]
    public class FleshOutApi : MongoFixtureBase
    {
        [Test]
        public async Task LoadingNonExistentIdReturnsNull()
        {
            using (var session = new OrmishSession(MongoDatabase))
            {
                var document = await session.Load<SomeDocument>(Guid.NewGuid().ToString());

                Assert.That(document, Is.Null);
            }
        }

        [Test]
        public async Task CanRoundtripSimpleDocument()
        {
            using (var session = new OrmishSession(MongoDatabase))
            {
                var someDocument = new SomeDocument { Text = "i'd like to read this shit again" };
                var id = await session.Save(someDocument);

                Console.WriteLine($"Generated ID: {id}");

                var roundtrippedDocument = await session.Load<SomeDocument>(id);

                Assert.That(roundtrippedDocument, Is.Not.Null);
                Assert.That(roundtrippedDocument.Text, Is.EqualTo("i'd like to read this shit again"));
            }
        }
    }

    public class SomeDocument
    {
        public string Text { get; set; }
    }
}