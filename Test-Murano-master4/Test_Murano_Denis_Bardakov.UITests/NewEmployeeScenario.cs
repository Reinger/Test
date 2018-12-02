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
    public class NewEmployeeScenario : SeleniumTest
    {
        public NewEmployeeScenario() : base("Test_Murano_Denis_Bardakov") { }
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

        [TestMethod]
        [TestCategory("System.Interface")]
        public void CancelNewEmployee()
        {
            ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());

            var lastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsTrue(lastRow== "Нет подходящих сотрудников по указанному фильтру");

            ChromeDriver.FindElement(By.CssSelector("li:nth-child(2) a")).Click();

            ChromeDriver.FindElements(By.CssSelector(".body-content a")).Last().Click();

            Assert.IsTrue(ChromeDriver.Title == "Список сотрудников - Моя компания");
            var currentlastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsTrue(currentlastRow == "Нет подходящих сотрудников по указанному фильтру");
            ChromeDriver.Dispose();
        }

        [TestMethod]
        [TestCategory("System.Interface")]
        public void NewEmployee()
        {
            ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());

            var lastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsTrue(lastRow == "Нет подходящих сотрудников по указанному фильтру");

            ChromeDriver.FindElement(By.CssSelector("li:nth-child(2) a")).Click();

            Assert.IsTrue(ChromeDriver.Title == "Создать - Моя компания");

            ChromeDriver.FindElement(By.Id("FullName")).Click();
            ChromeDriver.FindElement(By.Id("FullName")).Clear();
            ChromeDriver.FindElement(By.Id("FullName")).SendKeys("Петров");

            ChromeDriver.FindElement(By.Id("Position")).Click();
            ChromeDriver.FindElement(By.Id("Position")).Clear();
            ChromeDriver.FindElement(By.Id("Position")).SendKeys("Программист");

            ChromeDriver.FindElement(By.Id("Status")).Click();
            new SelectElement(ChromeDriver.FindElement(By.Id("Status"))).SelectByText("активен");
            ChromeDriver.FindElement(By.Id("Salary")).Click();

            ChromeDriver.FindElement(By.Id("Salary")).Click();
            ChromeDriver.FindElement(By.Id("Salary")).Clear();
            ChromeDriver.FindElement(By.Id("Salary")).SendKeys("55000");


            ChromeDriver.FindElements(By.CssSelector(".btn-default")).Last().Click();

            Assert.IsTrue(ChromeDriver.Title == "Список сотрудников - Моя компания");
            var currentlastRow = ChromeDriver.FindElement(By.ClassName("table-bordered")).
                FindElements(By.TagName("tr")).Last().
                FindElement(By.TagName("td")).Text;
            Assert.IsTrue(currentlastRow == "Петров");
            ChromeDriver.Dispose();
        }
    }
}