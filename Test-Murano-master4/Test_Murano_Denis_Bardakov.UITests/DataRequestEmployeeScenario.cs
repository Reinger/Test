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
    public class DataRequestEmployeeScenario : SeleniumTest
    {
        public DataRequestEmployeeScenario() : base("Test_Murano_Denis_Bardakov") { }
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
                    new string[4] { "Палкина Тамара Петровна", "Программист", "активен", "30000" },
                    new string[4] { "Палкина Тамара Петровна", "Программист", "активен", "30000" },
                    new string[4] { "Палкина Тамара Петровна", "Программист", "активен", "30000" },
                    new string[4] { "Палкина Тамара Петровна", "Программист", "активен", "30000" }
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
        public void DataRequest(string[][] employees)
        {
            InitializeDatabase(employees);
            ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            ChromeDriver.Manage().Window.Maximize();
            ChromeDriver.Navigate().GoToUrl(GetAbsoluteUrl());

            for (int i = 1; i <= 6; i++)
            {
                var currentlastRow = ChromeDriver.FindElement(By.ClassName("table-bordered"))
                    .FindElements(By.TagName("tr"))[i].FindElement(By.TagName("td")).Text;

                Assert.IsNotNull(currentlastRow);
            }

            ChromeDriver.Dispose();
        }
    }
}