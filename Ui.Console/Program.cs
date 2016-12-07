using System;
using System.Data;
using ConnectedLayer;

namespace Ui.Console
{
    public delegate void LinkToMethod();

    public class Program
    {
        private static object[,] _activeMenuList;
        private static readonly object[,] MainMenuList;
        private static readonly object[,] CustomersMenuList;
        private static readonly object[,] CarsMenuList;
        private static readonly string ConnectionString;
        private static readonly Cars InventoryObj;
        private static int _carId, _custId;
        private static string _name, _color, _make, _petName;
        private static DataTable _dataTable;

        static Program()
        {
            System.Console.ForegroundColor = ConsoleColor.White;
            ConnectionString = "Data Source=(local)\\SQLEXPRESS;Initial Catalog=AutoLot;Integrated Security=True;Pooling=False";
            InventoryObj = new Cars();

            MainMenuList = new object[3, 3];
            CustomersMenuList = new object[3, 3];
            CarsMenuList = new object[6, 3];

            MainMenuList[0, 0] = "Customers";
            MainMenuList[0, 1] = new LinkToMethod(PrintMenu);
            MainMenuList[0, 2] = CustomersMenuList;
            MainMenuList[1, 0] = "Cars";
            MainMenuList[1, 1] = new LinkToMethod(PrintMenu);
            MainMenuList[1, 2] = CarsMenuList;
            MainMenuList[2, 0] = "Exit";
            MainMenuList[2, 1] = new LinkToMethod(Exit);

            CustomersMenuList[0, 0] = "Show Customers";
            CustomersMenuList[0, 1] = new LinkToMethod(ShowCustomers);
            CustomersMenuList[1, 0] = "Add to risk category";
            CustomersMenuList[1, 1] = new LinkToMethod(AddToRisks);
            CustomersMenuList[2, 0] = "Back";
            CustomersMenuList[2, 1] = new LinkToMethod(PrintMenu);
            CustomersMenuList[2, 2] = MainMenuList;

            CarsMenuList[0, 0] = "Inventory";
            CarsMenuList[0, 1] = new LinkToMethod(ShowInventory);
            CarsMenuList[1, 0] = "Insert Auto";
            CarsMenuList[1, 1] = new LinkToMethod(InsertAuto);
            CarsMenuList[2, 0] = "Delete Auto";
            CarsMenuList[2, 1] = new LinkToMethod(DeleteAuto);
            CarsMenuList[3, 0] = "Update auto PetName";
            CarsMenuList[3, 1] = new LinkToMethod(UpdateAutoPetName);
            CarsMenuList[4, 0] = "Get PetName by CarId";
            CarsMenuList[4, 1] = new LinkToMethod(GetPetNameByCarId);
            CarsMenuList[5, 0] = "Back";
            CarsMenuList[5, 1] = new LinkToMethod(PrintMenu);
            CarsMenuList[5, 2] = MainMenuList;

            _activeMenuList = MainMenuList;
        }

        private static void Main()
        {
            PrintMenu();
        }

        private static void PrintMenu()
        {
            for (var menuIndex = 1; menuIndex <= _activeMenuList.GetLength(0); menuIndex++)
            {
                var menuName = _activeMenuList[menuIndex - 1, 0];
                System.Console.WriteLine(menuIndex + ". " + menuName);
            }

            var inputText = GetNotNullOrEmptyInputText();
            var choosenMenuNumber = ParseStringToNumber(inputText);

            for (var menuIndex = 1; menuIndex <= _activeMenuList.GetLength(0); menuIndex++)
            {
                if (choosenMenuNumber != menuIndex) continue;
                var method = (LinkToMethod)_activeMenuList[menuIndex - 1, 1];
                if (_activeMenuList[menuIndex - 1, 2] != null)
                {
                    System.Console.Clear();
                    _activeMenuList = (object[,])_activeMenuList[menuIndex - 1, 2];
                }
                else
                {
                    System.Console.WriteLine();
                    PrintMenuTitle(_activeMenuList[menuIndex - 1, 0].ToString());
                }
                method.DynamicInvoke();
                System.Console.WriteLine();
                PrintMenu();
            }
        }

        private static void ShowCustomers()
        {
            TryConnectToDb();
            TryLoadAllFromDbToDataTable("Customers");
            PrintDataTable();
            InventoryObj.CloseConnection();
        }


        private static void ShowInventory()
        {
            TryConnectToDb();
            TryLoadAllFromDbToDataTable("Inventory");
            PrintDataTable();
            InventoryObj.CloseConnection();
        }

        private static void TryLoadAllFromDbToDataTable(string tableDb)
        {
            try
            {
                _dataTable = InventoryObj.GetAllFrom(tableDb);
            }
            catch (Exception exception)
            {
                ReturnWhenException(exception);
            }
        }

        private static void PrintDataTable()
        {
            for (var rowIndex = 0; rowIndex < _dataTable.Rows.Count; rowIndex++)
            {
                var inventoryRow = _dataTable.Rows[rowIndex];
                for (var columnIndex = 0; columnIndex < _dataTable.Columns.Count; columnIndex++)
                {
                    var trimedCell = inventoryRow[columnIndex].ToString().Trim();
                    System.Console.Write(trimedCell + " ");
                }
                System.Console.WriteLine();
            }
        }

        private static void InsertAuto()
        {
            while (true)
            {
                TryConnectToDb();
                System.Console.WriteLine("Enter new CarId: ");
                var inputText = GetNotNullOrEmptyInputText();
                _carId = ParseStringToNumber(inputText);
                if (InventoryObj.IsPresentId("Inventory", _carId))
                {
                    PrintErrorText("This id is using. Try again");
                    continue;
                }
                System.Console.WriteLine("Enter new Name: ");
                _name = GetNotNullOrEmptyInputText();
                System.Console.WriteLine("Enter new Color: ");
                _color = GetNotNullOrEmptyInputText();
                System.Console.WriteLine("Enter new Make: ");
                _make = GetNotNullOrEmptyInputText();
                System.Console.WriteLine("Enter new PetName: ");
                _petName = GetNotNullOrEmptyInputText();

                InventoryObj.InsertAuto(_carId, _name, _color, _make, _petName);
                InventoryObj.CloseConnection();
                PrintOkMessage("You have add new car!");
                break;
            }
        }

        private static void DeleteAuto()
        {
            while (true)
            {
                TryConnectToDb();
                if (!FindId("Enter CarId: ", "Inventory", out _carId)) continue;
                InventoryObj.DeleteAuto(_carId);
                InventoryObj.CloseConnection();
                PrintOkMessage(string.Format("Car with id {0} deleted", _carId));
                break;
            }
        }

        private static void UpdateAutoPetName()
        {
            while (true)
            {
                TryConnectToDb();
                if (!FindId("Enter CarId: ", "Inventory", out _carId)) continue;
                System.Console.Write("Enter new PetName: ");
                _petName = GetNotNullOrEmptyInputText();
                InventoryObj.UpdateAutoPetName(_carId, _petName);
                InventoryObj.CloseConnection();
                PrintOkMessage(string.Format("PetName successfully have been updated to '{0}'", _petName));
                break;
            }
        }

        private static void GetPetNameByCarId()
        {
            while (true)
            {
                TryConnectToDb();
                if (!FindId("Enter CarId: ", "Inventory", out _carId)) continue;
                _petName = InventoryObj.GetPetNameProcedure(_carId);
                InventoryObj.CloseConnection();
                PrintOkMessage(string.Format("PetName is: '{0}'", _petName));
                break;
            }
        }

        private static void AddToRisks()
        {
            while (true)
            {
                TryConnectToDb();
                if (FindId("Enter CustId: ", "Customers", out _custId))
                {
                    InventoryObj.ProcessCredirRisk(true, _custId);
                }
                InventoryObj.CloseConnection();
                break;
            }
        }

        private static bool FindId(string enterMessage, string db, out int id)
        {
            System.Console.WriteLine(enterMessage);
            var inputText = GetNotNullOrEmptyInputText();
            id = ParseStringToNumber(inputText);
            if (InventoryObj.IsPresentId(db, id))
            {
                return InventoryObj.IsPresentId(db, id);
            }
            PrintErrorText("Can't find");
            return false;
        }

        private static void TryConnectToDb()
        {
            try
            {
                InventoryObj.OpenConnection(ConnectionString);
            }
            catch (Exception exception)
            {
                ReturnWhenException(exception);
            }
        }

        private static string GetNotNullOrEmptyInputText()
        {
            System.Console.Write(">");
            var inputText = System.Console.ReadLine();
            if (!string.IsNullOrEmpty(inputText)) return inputText;
            PrintErrorText("Can not be empty");
            inputText = GetNotNullOrEmptyInputText();
            return inputText;
        }

        private static int ParseStringToNumber(string inputText)
        {
            try
            {
                return int.Parse(inputText);
            }
            catch (Exception)
            {
                PrintErrorText("Input value must be a number");
                inputText = GetNotNullOrEmptyInputText();
                return ParseStringToNumber(inputText);
            }
        }

        private static void PrintMenuTitle(string menuTitle)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(menuTitle);
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        private static void PrintOkMessage(string okMessage)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine(okMessage);
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ReturnWhenException(Exception exception)
        {
            PrintErrorText("Somthing wrong. Message: \n" + exception.Message + "\n");
            PrintMenu();
        }

        private static void PrintErrorText(string errorMessage)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(errorMessage);
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Exit()
        {
            Environment.Exit(0);
        }

        public object[,] GetMainMenuList()
        {
            return MainMenuList;
        }

        public object[,] GetCustomersMenuList()
        {
            return CustomersMenuList;
        }

        public object[,] GetCarsMenuList()
        {
            return CarsMenuList;
        }
    }
}
