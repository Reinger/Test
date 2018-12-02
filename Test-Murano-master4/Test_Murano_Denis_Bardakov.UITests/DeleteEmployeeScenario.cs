using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Test_Murano_Denis_Bardakov.UITests
{
    [TestClass]
    public class DeleteEmployeeScenario : SeleniumTest
    {
        public DeleteEmployeeScenario() : base("Test_Murano_Denis_Bardakov") { }
        SqlConnection conn;

        [TestInitialize]
        public void InitializeSqlConnection()
        {
            conn = new SqlConnection()
            {
                ConnectionString = ConfigurationManager.
                ConnectionStrings["EmployeesContextTest"].ConnectionString
            };
            conn.Open();
        }

        [TestCleanup]
        public void CleanupSqlConnection()
        {
            conn.Close();
        }

        public static IEnumerable<object[]> Data
        {
            get
            {
                yield return new object[] { new string[][]
                {
                    new string[4] { "Сидоров Николай Петрович", "Менеджер", "активен", "25000" } ,
                    new string[4] { "Кларов Арсений Евгеньевич", "Бухгалтер", "не активен", "15000" } ,
                    new string[4] { "Палкина Тамара Петровна", "Программист", "активен", "30000" }
                }};
                yield return new object[] { new string[][]
                {
                    new string[4] { "Сидоров Николай Петрович", "Менеджер", "активен", "25000" }
                }};
            }
        }

        public void InitializeDatabase(string[][] employees)
        {
            var values = "";
            for (int i = 0; i < employees.Count(); i++)
            {
                var employee = employees[i];
                values += $"(N'{employee[0]}', N'{employee[1]}', N'{employee[2]}', {employee[3]})";
                if (i < employees.Count() - 1)
                    values += ", ";

            }
            using (SqlCommand cmd = new SqlCommand { Connection = conn })
            {
                cmd.CommandText = $@"
                TRUNCATE TABLE [dbo].[list_employees];
                INSERT INTO [dbo].[list_employees] (FullName, Position, Status, Salary)
                VALUES {values}
                ";
                cmd.ExecuteNonQuery();
            }
        }

        [TestMethod]
        [TestCategory("System.Interface")]
        [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
        public void CancelDeleteLastEmployee(string[][] employees)
        {
            InitializeDatabase(employees);
            ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());
            // Search last employee's name.
            var lastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsNotNull(lastRow);
            // Click delete on last employee.
            ChromeDriver.FindElements(By.CssSelector(".btn-danger")).Last().Click();
            // Click return employee list.
            ChromeDriver.FindElements(By.CssSelector(".no-color a")).Last().Click();
            // Check page.
            Assert.IsTrue(ChromeDriver.Title == "Список сотрудников - Моя компания");
            var currentlastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.AreEqual(currentlastRow, lastRow);
            ChromeDriver.Dispose();
        }

        [DataTestMethod]
        [TestCategory("System.Interface")]
        [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
        public void DeleteLastEmployeeInterface(string[][] employees)
        {
            InitializeDatabase(employees);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());
            // Search last employee's name.
            var lastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsNotNull(lastRow);
            // Click delete on last employee.
            ChromeDriver.FindElements(By.CssSelector(".btn-danger")).Last().Click();
            // Check page.
            Assert.IsTrue(ChromeDriver.Title == "Удалить - Моя компания");
            var name = ChromeDriver.FindElement(By.ClassName("dl-horizontal")).
                FindElement(By.TagName("dd")).Text;
            Assert.IsTrue(lastRow == name);
            // Click delete.
            ChromeDriver.FindElement(By.CssSelector(".btn-default")).Click();
            // Check page.
            Assert.IsTrue(ChromeDriver.Title == "Список сотрудников - Моя компания");
            var currentlastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.AreNotEqual(currentlastRow, lastRow);
            ChromeDriver.Dispose();
        }

        [DataTestMethod]
        [TestCategory("System.Data")]
        [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
        public void DeleteLastEmployeeSQL(string[][] employees)
        {
            InitializeDatabase(employees);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());
            // Click delete on last employee.
            ChromeDriver.FindElements(By.CssSelector(".btn-danger")).Last().Click();
            var idString = ChromeDriver.Url.Split('/').ToList().Last();
            int id;
            Assert.IsTrue(int.TryParse(idString, out id));
            // Check database.
            using (SqlCommand cmd = new SqlCommand { Connection = conn })
            {
                cmd.CommandText = $"SELECT COUNT(Id) FROM [dbo].[list_employees] WHERE Id={id}";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.AreEqual(count, 1);
            }
            // Click delete.
            ChromeDriver.FindElement(By.CssSelector(".btn-default")).Click();
            // Check database.
            using (SqlCommand cmd = new SqlCommand { Connection = conn })
            {
                cmd.CommandText = $"SELECT COUNT(Id) FROM [dbo].[list_employees] WHERE Id={id}";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.AreEqual(count, 0);
            }
            ChromeDriver.Dispose();
        }
    }
}
