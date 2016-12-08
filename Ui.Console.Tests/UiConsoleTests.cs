using NUnit.Framework;

namespace Ui.Console.Tests
{
    [TestFixture]
    internal class UiConsoleTests
    {
        [Test]
        public void MainMenuListTypes()
        {
            var p = new Program();
            var mainMenuList = p.GetMainMenuList();
            AssertTypes(mainMenuList);
        }

        [Test]
        public void CustomersMenuListTypes()
        {
            var p = new Program();
            var customersMenuList = p.GetCustomersMenuList();
            AssertTypes(customersMenuList);
        }

        [Test]
        public void CarsMenuListTypes()
        {
            var p = new Program();
            var carsMenuList = p.GetCarsMenuList();
            AssertTypes(carsMenuList);
        }

        private static void AssertTypes(object[,] menuList)
        {
            for (var index = 0; index < menuList.GetLength(0); index++)
            {
                var menuName = menuList[index, 0] as string;
                Assert.IsTrue(menuName != null);
                var menuLink = menuList[index, 1] as LinkToMethod;
                Assert.IsTrue(menuLink != null);
                var menuNextList = menuList[index, 2];
                if (menuNextList == null) continue;
                menuNextList = menuNextList as object[,];
                Assert.IsTrue(menuNextList != null);
            }
        }
    }
}
