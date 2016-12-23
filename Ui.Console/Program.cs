using System;
using System.Data;
using System.IO;
using ConnectedLayer;

namespace Ui.Console
{
    public delegate void MenuLinkToMethod();
    
    public class Program
    {
        private static object[,] _activeMenuList;
        private static readonly object[,] MainMenuList;
        private static readonly object[,] CustomersMenuList;
        private static readonly object[,] CarsMenuList;
        private static readonly string ConnectionString;
        private static readonly CarsDb CarsDb;
        private static readonly CustomersDb CustomersDb;
        private static int _carId, _custId;
        private static string _name, _color, _make, _petName;
        private static DataTable _dataTable;
        private static readonly ConsoleColor MainForegroungColor;
        private static readonly ConsoleColor ActiveMenuTitleColor;
        private static readonly ConsoleColor OkForegroungColor;
        private static readonly ConsoleColor ErrorForegroungColor;
        
        static Program()
        {
            MainForegroungColor = ConsoleColor.White;
            ActiveMenuTitleColor = ConsoleColor.Yellow;
            OkForegroungColor = ConsoleColor.Green;
            ErrorForegroungColor = ConsoleColor.Red;
            System.Console.ForegroundColor = MainForegroungColor;
            ConnectionString = 
                "Data Source=(local)\\SQLEXPRESS;Initial Catalog=AutoLot;Integrated Security=True;Pooling=False";
            CarsDb = new CarsDb();
            CustomersDb = new CustomersDb();

            MainMenuList = new object[3, 3];
            CustomersMenuList = new object[3, 3];
            CarsMenuList = new object[6, 3];

            MainMenuList[0, 0] = "Customers";
            MainMenuList[0, 1] = new MenuLinkToMethod(PrintMenu);
            MainMenuList[0, 2] = CustomersMenuList;
            MainMenuList[1, 0] = "Cars";
            MainMenuList[1, 1] = new MenuLinkToMethod(PrintMenu);
            MainMenuList[1, 2] = CarsMenuList;
            MainMenuList[2, 0] = "Exit";
            MainMenuList[2, 1] = new MenuLinkToMethod(Exit);

            CustomersMenuList[0, 0] = "Show Customers";
            CustomersMenuList[0, 1] = new MenuLinkToMethod(ShowCustomers);
            CustomersMenuList[1, 0] = "Add to risk category";
            CustomersMenuList[1, 1] = new MenuLinkToMethod(AddToRisks);
            CustomersMenuList[2, 0] = "Back";
            CustomersMenuList[2, 1] = new MenuLinkToMethod(PrintMenu);
            CustomersMenuList[2, 2] = MainMenuList;

            CarsMenuList[0, 0] = "Show Cars";
            CarsMenuList[0, 1] = new MenuLinkToMethod(ShowCars);
            CarsMenuList[1, 0] = "Insert Auto";
            CarsMenuList[1, 1] = new MenuLinkToMethod(InsertAuto);
            CarsMenuList[2, 0] = "Delete Auto";
            CarsMenuList[2, 1] = new MenuLinkToMethod(DeleteAuto);
            CarsMenuList[3, 0] = "Update auto PetName";
            CarsMenuList[3, 1] = new MenuLinkToMethod(UpdateAutoPetName);
            CarsMenuList[4, 0] = "Get PetName by CarId";
            CarsMenuList[4, 1] = new MenuLinkToMethod(GetPetNameByCarId);
            CarsMenuList[5, 0] = "Back";
            CarsMenuList[5, 1] = new MenuLinkToMethod(PrintMenu);
            CarsMenuList[5, 2] = MainMenuList;
        }

        public static void Main(string[] args)
        {
            string activeMenuListName;
            try
            {
                activeMenuListName = args[0];
            }
            catch (Exception)
            {
                activeMenuListName = string.Empty;
            }
            
            switch (activeMenuListName)
            {
                case "MainMenu":
                    _activeMenuList = MainMenuList;
                    break;
                case "Customers":
                    _activeMenuList = CustomersMenuList;
                    break;
                case "Cars":
                    _activeMenuList = CarsMenuList;
                    break;
                default:
                    _activeMenuList = MainMenuList;
                    break;
            }
            PrintMenu();
        }

        private static void PrintMenu()
        {
            while (true)
            {
                for (var menuIndex = 1; menuIndex <= _activeMenuList.GetLength(0); menuIndex++)
                {
                    var menuName = _activeMenuList[menuIndex - 1, 0];
                    System.Console.WriteLine(menuIndex + ". " + menuName);
                }

                var enteredText = GetNotNullOrEmptyInputText();
                var choosenMenuNumber = ParseStringToNumber(enteredText);

                for (var menuIndex = 1; menuIndex <= _activeMenuList.GetLength(0); menuIndex++)
                {
                    if (choosenMenuNumber != menuIndex) continue;
                    var nextMenuLink = (MenuLinkToMethod) _activeMenuList[menuIndex - 1, 1];
                    var nextActiveMenuList = _activeMenuList[menuIndex - 1, 2];
                    if (nextActiveMenuList != null)
                    {
                        TryClearConsole();
                        _activeMenuList = (object[,]) nextActiveMenuList;
                    }
                    else
                    {
                        System.Console.WriteLine();
                        var activeMenuTitle = _activeMenuList[menuIndex - 1, 0].ToString();
                        PrintColored(activeMenuTitle, ActiveMenuTitleColor);
                    }
                    nextMenuLink.DynamicInvoke();
                    System.Console.WriteLine();
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void TryClearConsole()
        {
            try
            {
                System.Console.Clear();
            }
            catch (IOException)
            {
                System.Console.WriteLine("Cannot clear console. Output is redirecting");
            }
        }

        private static void ShowCustomers()
        {
            TryConnectTo(CustomersDb);
            TryLoadAllToDataTableFrom(CustomersDb, "Customers");
            PrintDataTable();
            CustomersDb.CloseConnection();
        }

        private static void ShowCars()
        {
            TryConnectTo(CarsDb);
            TryLoadAllToDataTableFrom(CarsDb, "Inventory");
            PrintDataTable();
            CarsDb.CloseConnection();
        }

        private static void TryLoadAllToDataTableFrom(BaseDb baseDb, string tableDb)
        {
            try
            {
                _dataTable = baseDb.GetAllFrom(tableDb);
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
                TryConnectTo(CarsDb);
                System.Console.WriteLine("Enter new CarId: ");
                var enteredText = GetNotNullOrEmptyInputText();
                _carId = ParseStringToNumber(enteredText);
                if (CarsDb.IsPresentId("Inventory", _carId))
                {
                    const string errorMessage = "This id is using. Try again";
                    PrintColored(errorMessage, ErrorForegroungColor);
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

                CarsDb.InsertAuto(_carId, _name, _color, _make, _petName);
                CarsDb.CloseConnection();
                const string okMessage = "You have add new car!";
                PrintColored(okMessage, OkForegroungColor);
                break;
            }
        }

        private static void DeleteAuto()
        {
            while (true)
            {
                TryConnectTo(CarsDb);
                if (!FindId("Enter CarId: ", "Inventory", out _carId)) continue;
                CarsDb.DeleteAuto(_carId);
                CarsDb.CloseConnection();
                var okMessage = string.Format("Car with id {0} deleted", _carId);
                PrintColored(okMessage, OkForegroungColor);
                break;
            }
        }

        private static void UpdateAutoPetName()
        {
            while (true)
            {
                TryConnectTo(CarsDb);
                if (!FindId("Enter CarId: ", "Inventory", out _carId)) continue;
                System.Console.Write("Enter new PetName: ");
                _petName = GetNotNullOrEmptyInputText();
                CarsDb.UpdateAutoPetName(_carId, _petName);
                CarsDb.CloseConnection();
                var okMessage = string.Format("PetName successfully have been updated to '{0}'", _petName);
                PrintColored(okMessage, OkForegroungColor);
                break;
            }
        }

        private static void GetPetNameByCarId()
        {
            while (true)
            {
                TryConnectTo(CarsDb);
                if (!FindId("Enter CarId: ", "Inventory", out _carId)) continue;
                _petName = CarsDb.GetPetNameProcedure(_carId);
                CarsDb.CloseConnection();
                var okMessage = string.Format("PetName is: '{0}'", _petName);
                PrintColored(okMessage, OkForegroungColor);
                break;
            }
        }

        private static void AddToRisks()
        {
            while (true)
            {
                TryConnectTo(CustomersDb);
                if (FindId("Enter CustId: ", "Customers", out _custId))
                {
                    CustomersDb.ProcessCredirRisk(true, _custId);
                }
                CustomersDb.CloseConnection();
                break;
            }
        }

        private static bool FindId(string enterMessage, string db, out int id)
        {
            System.Console.WriteLine(enterMessage);
            var enteredText = GetNotNullOrEmptyInputText();
            id = ParseStringToNumber(enteredText);
            if (CarsDb.IsPresentId(db, id))
            {
                return CarsDb.IsPresentId(db, id);
            }
            const string errorMessage = "Can't find";
            PrintColored(errorMessage, ErrorForegroungColor);
            return false;
        }

        private static void TryConnectTo(BaseDb baseDb)
        {
            try
            {
                baseDb.OpenConnection(ConnectionString);
            }
            catch (Exception exception)
            {
                ReturnWhenException(exception);
            }
        }

        private static string GetNotNullOrEmptyInputText()
        {
            System.Console.Write(">");
            var enteredText = System.Console.ReadLine();
            if (!string.IsNullOrEmpty(enteredText)) return enteredText;
            const string errorMessage = "Can not be empty";
            PrintColored(errorMessage, ErrorForegroungColor);
            enteredText = GetNotNullOrEmptyInputText();
            return enteredText;
        }

        private static int ParseStringToNumber(string enteredText)
        {
            try
            {
                return int.Parse(enteredText);
            }
            catch (Exception)
            {
                const string errorMessage = "Input value must be a number";
                PrintColored(errorMessage, ErrorForegroungColor);
                enteredText = GetNotNullOrEmptyInputText();
                return ParseStringToNumber(enteredText);
            }
        }

        private static void ReturnWhenException(Exception exception)
        {
            var errorMessage = "Somthing wrong. Message: \n" + exception.Message + "\n";
            PrintColored(errorMessage, ErrorForegroungColor);
            PrintMenu();
        }

        private static void PrintColored(string message, ConsoleColor newForegrounColor)
        {
            System.Console.ForegroundColor = newForegrounColor;
            System.Console.WriteLine(message);
            System.Console.ForegroundColor = MainForegroungColor;
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

        private static void Exit()
        {
            Environment.Exit(0);
        }
    }
}
