using System;
using System.Collections;
using System.Data;

namespace DisconnectedLayer
{
    public class Cars
    {
        public Cars()
        {
            var carsInventory = new DataSet
            {
                ExtendedProperties = { { "Time Stamp", DateTime.Now }, { "DataSetId", Guid.NewGuid() }, { "Company", "My Shop" } }
            };
            FillDataSet(carsInventory);
            PrintDataSet(carsInventory);
        }

        private static void FillDataSet(DataSet dataSet)
        {
            var carIdColumn = new DataColumn("Id", typeof(int))
            {
                Caption = "Car Id",
                ReadOnly = true,
                AllowDBNull = false,
                Unique = true,
                AutoIncrement = true,
                AutoIncrementSeed = 0,
                AutoIncrementStep = 1
            };

            var carName = new DataColumn("Name", typeof(string));
            var carColor = new DataColumn("Color", typeof(string));
            var carMake = new DataColumn("Make", typeof(string));
            var carPetName = new DataColumn("PetName", typeof(string))
            {
                Caption = "Friendly name"
            };

            var inventoryTable = new DataTable("Inventory");
            inventoryTable.Columns.AddRange(new[] { carIdColumn, carName, carColor, carMake, carPetName });
            var carRow = inventoryTable.NewRow();
            carRow["Name"] = "BMV";
            carRow["Color"] = "Orange";
            carRow["Make"] = "Germany";
            carRow["PetName"] = "Hamlet";
            inventoryTable.Rows.Add(carRow);

            carRow = inventoryTable.NewRow();
            carRow[1] = "Seat";
            carRow[2] = "Green";
            carRow[3] = "Spain";
            carRow[4] = "Wolf";
            inventoryTable.Rows.Add(carRow);

            inventoryTable.PrimaryKey = new[] { inventoryTable.Columns[0] };
            dataSet.Tables.Add(inventoryTable);
        }

        private static void PrintDataSet(DataSet dataSet)
        {
            Console.WriteLine("Objects of DataSet\n");
            Console.WriteLine("Data Set Name: {0}", dataSet.DataSetName);
            foreach (DictionaryEntry extendedProperty in dataSet.ExtendedProperties)
            {
                Console.WriteLine("Key = {0}, Value = {1}", extendedProperty.Key, extendedProperty.Value);
            }
            Console.WriteLine();

            foreach (DataTable table in dataSet.Tables)
            {
                Console.WriteLine("=> Table: {0}", table);

                for (var currentColumn = 0; currentColumn < table.Columns.Count; currentColumn++)
                {
                    Console.Write(table.Columns[currentColumn].ColumnName + "\t");
                }
                Console.WriteLine("\n---------------------------------------");

                PrintTable(table);

                // Not usefull method
                //for (var currentRow = 0; currentRow < table.Rows.Count; currentRow++)
                //{
                //    for (var currentColumn = 0; currentColumn < table.Columns.Count; currentColumn++)
                //    {
                //        Console.Write(table.Rows[currentRow][currentColumn] + "\t");
                //    }
                //    Console.WriteLine();
                //}
            }
        }

        private static void PrintTable(DataTable dataTable)
        {
            var dataTableReader = dataTable.CreateDataReader();
            while (dataTableReader.Read())
            {
                for (var i = 0; i < dataTableReader.FieldCount; i++)
                {
                    Console.Write("{0}\t", dataTableReader.GetValue(i).ToString().Trim());
                }
                Console.WriteLine();
            }
            dataTableReader.Close();
        }
    }
}
