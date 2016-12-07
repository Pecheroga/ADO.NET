using System;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace ConnectedLayer.Tests
{
    [TestFixture]
    public class ConnectedLayerTests
    {
        private const string ConnectionString =
                "Data Source=(local)\\SQLEXPRESS;Initial Catalog=AutoLot;Integrated Security=True;Pooling=False";

        [Test]
        public void CanOpenConnectionToDb()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            Assert.IsTrue(i.Connection.State == ConnectionState.Open);
            i.CloseConnection();
        }

        [Test]
        public void ConnectionIsClosedAfterCreatingTheInstance()
        {
            var i = new Cars();
            Assert.IsTrue(i.Connection == null);
        }

        [Test]
        public void CanCloseOpenedConnection()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            i.CloseConnection();
            Assert.IsTrue(i.Connection.State == ConnectionState.Closed);
        }

        [Test]
        public void GetAllFromDbTable(
            [Values("Inventory", "Customers")] string tableDb)
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var dataTable = i.GetAllFrom(tableDb);
            Assert.IsNotNull(dataTable);
            Assert.AreNotEqual(0, dataTable.Rows.Count);
            i.CloseConnection();
        }

        [Test]
        public void PresentId(
            [Values("Inventory", "Customers")] string tableDb)
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var dataTable = i.GetAllFrom(tableDb);
            var firstRow = dataTable.Rows[0];
            var firstId = (int)firstRow[0];
            Assert.IsTrue(i.IsPresentId(tableDb, firstId));
            i.CloseConnection();
        }

        [Test]
        public void InsertAutoInInventoryTable()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(i);
            Assert.IsTrue(i.IsPresentId("Inventory", carId));
            i.DeleteAuto(carId);
            i.CloseConnection();
        }

        [Test]
        public void DeleteAutoFromInventoryTable()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(i);
            i.DeleteAuto(carId);
            Assert.IsFalse(i.IsPresentId("Inventory", carId));
            i.CloseConnection();
        }

        [Test]
        public void UpdateAutoPetName()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(i);
            const string expectedPetName = "p2";
            i.UpdateAutoPetName(carId, expectedPetName);
            var petName = i.GetPetNameProcedure(carId);
            Assert.AreEqual(expectedPetName, petName);
            i.DeleteAuto(carId);
            i.CloseConnection();
        }

        [Test]
        public void GetPetNameProcedure()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(i);
            var petName = i.GetPetNameProcedure(carId);
            Assert.AreEqual("p", petName);
            i.DeleteAuto(carId);
            i.CloseConnection();
        }

        [Test]
        public void ProcessCreditRisks()
        {
            var i = new Cars();
            i.OpenConnection(ConnectionString);
            var customersTable = i.GetAllFrom("Customers");

            //i.ProcessCredirRisk(true, custId);
            i.CloseConnection();
        }

        private static int InsertTestAutoInTheInventoryTable(Cars i)
        {
            var dataTable = i.GetAllFrom("Inventory");
            var carId = GenerateNewUnicIdInTheTable(dataTable);
            i.InsertAuto(carId, "n", "c", "m", "p");
            return carId;
        }

        private static int GenerateNewUnicIdInTheTable(DataTable dataTable)
        {
            var rand = new Random();
            var id = rand.Next(0, 1000);
            if (dataTable.Rows.Cast<DataRow>().Any(row => id == (int)row[0]))
            {
                GenerateNewUnicIdInTheTable(dataTable);
            }
            return id;
        }
    }
}
