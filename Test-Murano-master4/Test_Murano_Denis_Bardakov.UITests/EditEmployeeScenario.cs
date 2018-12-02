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
    public class EditEmployeeScenario : SeleniumTest
    {
        public EditEmployeeScenario() : base("Test_Murano_Denis_Bardakov") { }
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
        public void CancelEditLastEmployee(string[][] employees)
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
            ChromeDriver.FindElements(By.CssSelector(".btn-warning")).Last().Click();
            // Click return employee list.
            ChromeDriver.FindElements(By.CssSelector(".body-content a")).Last().Click();
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
        public void EditLastEmployeeInterface(string[][] employees)
        {
            InitializeDatabase(employees);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());
            // Search last employee's name.

            var lastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsNotNull(lastRow);

            // Click edit on last employee.
            ChromeDriver.FindElements(By.CssSelector(".btn-warning")).Last().Click();
            // Check page.
            Assert.IsTrue(ChromeDriver.Title == "Редактирование - Моя компания");

            ChromeDriver.FindElement(By.Id("FullName")).Click();
            ChromeDriver.FindElement(By.Id("FullName")).Clear();
            ChromeDriver.FindElement(By.Id("FullName")).SendKeys("Измененный");

            ChromeDriver.FindElement(By.Id("Position")).Click();
            ChromeDriver.FindElement(By.Id("Position")).Clear();
            ChromeDriver.FindElement(By.Id("Position")).SendKeys("Измененный");

            ChromeDriver.FindElement(By.Id("Salary")).Click();
            ChromeDriver.FindElement(By.Id("Salary")).Clear();
            ChromeDriver.FindElement(By.Id("Salary")).SendKeys("0");

            // Click edit.
            ChromeDriver.FindElement(By.CssSelector(".btn-default")).Click();
            // Check page.
            Assert.IsTrue(ChromeDriver.Title == "Список сотрудников - Моя компания");
            var currentlastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsFalse(currentlastRow==lastRow);
            ChromeDriver.Dispose();
        }
    }
}