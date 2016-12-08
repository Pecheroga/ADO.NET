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
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            Assert.IsTrue(cars.Connection.State == ConnectionState.Open);
            cars.CloseConnection();
        }

        [Test]
        public void ConnectionIsClosedAfterCreatingTheInstance()
        {
            var cars = new CarsDb();
            Assert.IsTrue(cars.Connection == null);
        }

        [Test]
        public void CanCloseOpenedConnection()
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            cars.CloseConnection();
            Assert.IsTrue(cars.Connection.State == ConnectionState.Closed);
        }

        [Test]
        public void GetAllFromDbTable(
            [Values("Inventory", "Customers")] string tableDb)
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var dataTable = cars.GetAllFrom(tableDb);
            Assert.IsNotNull(dataTable);
            Assert.AreNotEqual(0, dataTable.Rows.Count);
            cars.CloseConnection();
        }

        [Test]
        public void PresentId(
            [Values("Inventory", "Customers")] string tableDb)
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var dataTable = cars.GetAllFrom(tableDb);
            var firstRow = dataTable.Rows[0];
            var firstId = (int)firstRow[0];
            Assert.IsTrue(cars.IsPresentId(tableDb, firstId));
            cars.CloseConnection();
        }

        [Test]
        public void InsertAutoInInventoryTable()
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(cars);
            Assert.IsTrue(cars.IsPresentId("Inventory", carId));
            cars.DeleteAuto(carId);
            cars.CloseConnection();
        }

        [Test]
        public void DeleteAutoFromInventoryTable()
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(cars);
            cars.DeleteAuto(carId);
            Assert.IsFalse(cars.IsPresentId("Inventory", carId));
            cars.CloseConnection();
        }

        [Test]
        public void UpdateAutoPetName()
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(cars);
            const string expectedPetName = "p2";
            cars.UpdateAutoPetName(carId, expectedPetName);
            var petName = cars.GetPetNameProcedure(carId);
            Assert.AreEqual(expectedPetName, petName);
            cars.DeleteAuto(carId);
            cars.CloseConnection();
        }

        [Test]
        public void GetPetNameProcedure()
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var carId = InsertTestAutoInTheInventoryTable(cars);
            var petName = cars.GetPetNameProcedure(carId);
            Assert.AreEqual("p", petName);
            cars.DeleteAuto(carId);
            cars.CloseConnection();
        }

        [Test]
        public void ProcessCreditRisks()
        {
            var cars = new CarsDb();
            cars.OpenConnection(ConnectionString);
            var customersTable = cars.GetAllFrom("Customers");

            //CarsDb.ProcessCredirRisk(true, custId);
            cars.CloseConnection();
        }

        private static int InsertTestAutoInTheInventoryTable(CarsDb carsDb)
        {
            var dataTable = carsDb.GetAllFrom("Inventory");
            var carId = GenerateNewUnicIdInTheTable(dataTable);
            carsDb.InsertAuto(carId, "n", "c", "m", "p");
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
