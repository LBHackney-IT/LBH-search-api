using System.Linq;
using SearchApi.V1.Infrastructure;
using NUnit.Framework;
using SearchApi.Tests.V1.Helper;

namespace SearchApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class DatabaseContextTest : DatabaseTests
    {
        [Test]
        public void CanGetADatabaseEntity()
        {
            var databaseEntity = DatabaseEntityHelper.CreateDatabaseEntity();

            DatabaseContext.Add(databaseEntity);
            DatabaseContext.SaveChanges();

            var result = DatabaseContext.DatabaseEntities.ToList().FirstOrDefault();

            Assert.AreEqual(result, databaseEntity);
        }
    }
}
