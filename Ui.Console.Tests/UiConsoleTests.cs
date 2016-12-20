using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Ui.Console.Tests
{
    [TestFixture]
    internal sealed class UiConsoleTests
    {
        private Process _consoleProcess;
        private int _processId;
       
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var assemblyCodeBaseLocation = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var dirName = Path.GetDirectoryName(assemblyCodeBaseLocation);
            if (dirName == null) return;
            if (dirName.StartsWith("file:\\"))
            {
                dirName = dirName.Substring(6);
            }
            Environment.CurrentDirectory = dirName;
        }

        [TearDown]
        public void TearDown()
        {
            StopUiConsoleProcess();
        }
        
        [Test]
        public void PrintMenuWithConsecutiveIndexNumbers(
            [Values("MainMenu", "Customers", "Cars")] string menuListName)
        {
            StartUiConsoleProcess(menuListName);
            _consoleProcess.StandardInput.Close();
            var menuListFromClass = GetMenuListFromClass(menuListName);
            
            for (var index = 0; index < menuListFromClass.GetLength(0); index++)
            {
                var readLineFromProcess = _consoleProcess.StandardOutput.ReadLine();
                var menuIndex = index + 1;
                var menuTitleFromClass = (string)menuListFromClass[index, 0];
                var readLineFormClass = string.Format("{0}. {1}", menuIndex, menuTitleFromClass);
                if (readLineFromProcess != readLineFormClass)
                {
                    Assert.Fail("Menu doesn't look as expected. \n " +
                                "Expected: {0} \n Printed: {1} ", readLineFormClass, readLineFromProcess);
                }
            }
            Assert.IsTrue(true);
        }

        [Test, Sequential]
        public void GoToNewMenuSection(
            [Values("Show Customers", "Show Cars")] string expectedFirstMenuItem,
            [Values("1", "2")] string menuNumber)
        {
            const string menuListName = "MainMenu";
            StartUiConsoleProcess(menuListName);
            using (var streamReader = _consoleProcess.StandardInput)
            {
                streamReader.WriteLine(menuNumber);
            }
            const string expectedLine = ">";
            try
            {
                CompareReadLineFromProcessWith(expectedLine);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
            var firstPrintedMenuItem = _consoleProcess.StandardOutput.ReadLine();
            Assert.AreEqual(expectedFirstMenuItem, firstPrintedMenuItem);
        }

        [Test]
        public void ExitFromConsoleThenPressed3()
        {
            const string menuListName = "MainMenu";
            StartUiConsoleProcess(menuListName);
            using (var streamWriter = _consoleProcess.StandardInput)
            {
                streamWriter.WriteLine("3");
            }
           
            if (_consoleProcess.WaitForExit(1000))
            {
                var exitCode = _consoleProcess.ExitCode;
                Assert.AreEqual(0, exitCode);
            }
            else
            {
                Assert.Fail("Cann't close application");
            }
        }

        [Test]
        public void BlankInputNotAllows(
            [Values("MainMenu", "Customers", "Cars")] string menuListName)
        {
            StartUiConsoleProcess(menuListName);
            _consoleProcess.StandardInput.Close();

            const string expectedLine = "Can not be empty";
            Assert.DoesNotThrow(() => CompareReadLineFromProcessWith(expectedLine));
        }

        [Test]
        public void InputLettersAndSymbolsNotAllows(
            [Values("a", "A", ".", "+2_a!B")] string inputText)
        {
            const string menuListName = "MainMenu";
            StartUiConsoleProcess(menuListName);
            using (var streamWriter = _consoleProcess.StandardInput)
            {
                streamWriter.WriteLine(inputText);
            }

            const string expectedLine = "Input value must be a number";
            Assert.DoesNotThrow(() => CompareReadLineFromProcessWith(expectedLine));
        }

        [Test, Sequential]
        public void PrintNameOfSelectedMenuAsCaption(
            [Values("Customers", "Cars")] string menuListName,
            [Values("Show Customers", "Show Cars")] string expectedPrintedMenuName)
        {
            StartUiConsoleProcess(menuListName);
            using (var streamWriter = _consoleProcess.StandardInput)
            {
                streamWriter.WriteLine("1");
            }
            
            const string expectedLine = ">";
            Assert.DoesNotThrow(() => CompareReadLineFromProcessWith(expectedLine));

            var printedMenuName = _consoleProcess.StandardOutput.ReadLine();
            Assert.AreEqual(expectedPrintedMenuName, printedMenuName);
        }

        [Test, Sequential]
        public void FirstPrintedColumnIsId(
            [Values("Customers", "Cars")] string menuListName,
            [Values("Show Customers", "Show Cars")] string expectedPrinedMenuName)
        {
            StartUiConsoleProcess(menuListName);
            using (var streamWriter = _consoleProcess.StandardInput)
            {
                streamWriter.WriteLine("1");
            }
            string readLineFromProcess;
            do
            {
                readLineFromProcess = _consoleProcess.StandardOutput.ReadLine();
            } while (readLineFromProcess != expectedPrinedMenuName);

            var firstPrintedDataRow = _consoleProcess.StandardOutput.ReadLine();

            if (firstPrintedDataRow == null) return;
            var firstWhiteSpaceIndex = firstPrintedDataRow.IndexOf(" ", StringComparison.Ordinal);
            var idColumn = firstPrintedDataRow.Remove(firstWhiteSpaceIndex);
            int i;
            Assert.IsTrue(int.TryParse(idColumn, out i), "First column doesn't contain numbers. It isn't ID column");
        }

        [Test, Sequential]
        public void CountOfPrintedDataColumn(
            [Values("Customers", "Cars")] string menuListName,
            [Values("Show Customers", "Show Cars")] string expectedPrinedMenuName,
            [Values(2, 4)] int expectedWhiteSpaceCount)
        {
            StartUiConsoleProcess(menuListName);
            using (var streamWriter = _consoleProcess.StandardInput)
            {
                streamWriter.WriteLine("1");
            }
            string readLineFromProcess;
            do
            {
                readLineFromProcess = _consoleProcess.StandardOutput.ReadLine();
            } while (readLineFromProcess != expectedPrinedMenuName);

             var firstPrintedDataRow = _consoleProcess.StandardOutput.ReadLine();
            if (firstPrintedDataRow == null) return;
            var whiteSpaceCount = firstPrintedDataRow.Count(char.IsWhiteSpace);
            Assert.IsTrue(whiteSpaceCount >= expectedWhiteSpaceCount, "Count of the columns is less than expected");
        }
        
        [Test]
        public void CheckMenuTypes(
            [Values("MainMenu", "Customers", "Cars")] string menuListName)
        {
            var menuListFromClass = GetMenuListFromClass(menuListName);
            for (var index = 0; index < menuListFromClass.GetLength(0); index++)
            {
                var menuItemName = menuListFromClass[index, 0] as string;
                Assert.IsNotNull(
                    menuItemName, 
                    "Menu item name is not the type of string");
                var menuItemLink = menuListFromClass[index, 1] as MenuLinkToMethod;
                Assert.IsNotNull(
                    menuItemLink, 
                    "Menu item link is not the type of delegate MenuLinkToMethod");
                var menuItemLinkNextList = menuListFromClass[index, 2];
                if (menuItemLinkNextList == null) continue;
                menuItemLinkNextList = menuItemLinkNextList as object[,];
                Assert.IsNotNull(
                    menuItemLinkNextList, 
                    "Menu item Link To Next Menu List is not the type of object[,]");
            }
        }

        private void CompareReadLineFromProcessWith(string expectedLine)
        {
            string prevTextFromProcess, readLineFromProcess = string.Empty;
            do
            {
                prevTextFromProcess = readLineFromProcess;
                readLineFromProcess = _consoleProcess.StandardOutput.ReadLine();
            } while (
                readLineFromProcess != null
                && readLineFromProcess != prevTextFromProcess
                && !readLineFromProcess.Contains(expectedLine));
            
            if (readLineFromProcess == null)
            {
                throw new Exception("Process was closed. Possibly exception was throwed in process");
            }
            if (readLineFromProcess == prevTextFromProcess)
            {
                throw new Exception("Loop is present, when reading from output. \n" +
                            "Looks like the \"" + expectedLine + "\" string is not found. \n" +
                            "Somthing wrong with it.");
            }
        }

        private static object[,] GetMenuListFromClass(string menuListName)
        {
            var consoleClass = new Program();
            switch (menuListName)
            {
                case "MainMenu":
                    return consoleClass.GetMainMenuList();
                case "Customers":
                    return consoleClass.GetCustomersMenuList();
                case "Cars":
                    return consoleClass.GetCarsMenuList();
                default:
                    Assert.Fail("menuListFromClass is not specified");
                    break;
            }
            return null;
        }
        
        private void StartUiConsoleProcess(string consoleArgs)
        {
            _consoleProcess = new Process
            {
                StartInfo =
                {
                    FileName = "Ui.Console.exe",
                    Arguments = consoleArgs,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            };
            _consoleProcess.Start();
            _processId = _consoleProcess.Id;
        }

        private void StopUiConsoleProcess()
        {
            try
            {
                
                var consoleProcessById = Process.GetProcessById(_processId);
                if (consoleProcessById.HasExited) return;
                consoleProcessById.Kill();
                consoleProcessById.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
