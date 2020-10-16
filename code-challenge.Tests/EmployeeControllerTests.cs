using challenge.Controllers;
using challenge.Data;
using challenge.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using code_challenge.Tests.Integration.Extensions;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using code_challenge.Tests.Integration.Helpers;
using System.Text;
using System.Linq;

namespace code_challenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<TestServerStartup>()
                .UseEnvironment("Development"));

            _httpClient = _testServer.CreateClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";
            var expectedReportsJohn = 2;
            var expectedReportsPaul = 0;
            var expectedReportsRingo = 2;
            var expectedReportsPete = 0;
            var expectedReportsGeorge = 0;
            
            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);

            //Ensure Structure of direct reports is intact
            Assert.AreEqual(expectedReportsJohn, employee.DirectReports.Count());
            Assert.AreEqual(expectedReportsPaul, employee.DirectReports[0].DirectReports.Count());
            Assert.AreEqual(expectedReportsRingo, employee.DirectReports[1].DirectReports.Count());
            Assert.AreEqual(expectedReportsPete, employee.DirectReports[1].DirectReports[0].DirectReports.Count());
            Assert.AreEqual(expectedReportsGeorge, employee.DirectReports[1].DirectReports[0].DirectReports.Count());
            
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()

            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);

        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetEmployeeReportingStructure_Returns_Ok()
        {
            // Arrange
            var johnEmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var johnTotalNumberOfreports = 4;
            var expectedReportsJohn = 2;
            var expectedReportsPaul = 0;
            var expectedReportsRingo = 2;
            var expectedReportsPete = 0;
            var expectedReportsGeorge = 0;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/getReportingStructure/{johnEmployeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();

            //Verify the full structure is in tact
            Assert.AreEqual(johnEmployeeId, reportingStructure.Employee.EmployeeId);
            Assert.AreEqual(johnTotalNumberOfreports, reportingStructure.NumberOfReports);

            //Ensure Structure of direct reports is intact
            Assert.AreEqual(expectedReportsJohn, reportingStructure.Employee.DirectReports.Count());
            Assert.AreEqual(expectedReportsPaul, reportingStructure.Employee.DirectReports[0].DirectReports.Count());
            Assert.AreEqual(expectedReportsRingo, reportingStructure.Employee.DirectReports[1].DirectReports.Count());
            Assert.AreEqual(expectedReportsPete, reportingStructure.Employee.DirectReports[1].DirectReports[0].DirectReports.Count());
            Assert.AreEqual(expectedReportsGeorge, reportingStructure.Employee.DirectReports[1].DirectReports[0].DirectReports.Count());
        }

        [TestMethod]
        public void GetEmployeeReportingStructure_Returns_NotFound()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c861"; //Invalid id
            

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/getReportingStructure/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);


        }

        [TestMethod]
        public void GetCompensation_Returns_Ok()
        {
            // Arrange
            var employeeId = "b7839309-3348-463b-a7e3-5de1c168beb3"; 
            decimal salary = 100002.02m;
            DateTimeOffset effectiveDate = DateTimeOffset.Parse("10/13/2022 8:09:49 PM");

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/getCompensation/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var compensation = response.DeserializeContent<Compensation>();
            
            Assert.AreEqual(compensation.Employee.EmployeeId, employeeId);
            Assert.AreEqual(compensation.Salary, salary);
            Assert.AreEqual(effectiveDate.Ticks, compensation.EffectiveDate.Ticks);
        }

        [TestMethod]
        public void GetCompensation_Returns_NotFound()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86a"; //Invalid id
            
            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/getCompensation/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
           
        }

        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";
            decimal salary = 100001.01m;
            DateTimeOffset effectiveDate = DateTimeOffset.Parse("10/13/2021 8:09:49 PM");
            var compensation = new Compensation() { EmployeeId = employeeId, EffectiveDate = effectiveDate, Salary = salary };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/createCompensation",
                    new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;
            var newCompensation = response.DeserializeContent<Compensation>(); ;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(newCompensation.Employee.EmployeeId, employeeId);
            Assert.AreEqual(newCompensation.Salary, salary);
            Assert.AreEqual(newCompensation.EffectiveDate.Ticks, effectiveDate.Ticks);

        }

        [TestMethod]
        public void CreateCompensation_Returns_NotFound()
        {
            // Arrange
            var employeeId = "";
            decimal salary = 100001.01m;
            DateTimeOffset effectiveDate = DateTimeOffset.Parse("10/13/2021 8:09:49 PM");
            var compensation = new Compensation() { EmployeeId = employeeId, EffectiveDate = effectiveDate, Salary = salary };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/createCompensation",
                    new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;
           
            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        }

        /// <summary>
        /// Ensure we do not add a duplicate conpensation
        /// </summary>
        [TestMethod]
        public void CreateCompensation_Returns_BadRequest()
        {
            // Arrange
            var employeeId = "b7839309-3348-463b-a7e3-5de1c168beb3";
            decimal salary = 100001.01m;
            DateTimeOffset effectiveDate = DateTimeOffset.Parse("10/13/2021 8:09:49 PM");
            var compensation = new Compensation() { EmployeeId = employeeId, EffectiveDate = effectiveDate, Salary = salary };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/createCompensation",
                    new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        }

        /// <summary>
        /// I don't normally do something like this, but you called it out so I figured I'd throw it in as
        /// "proof of persistance".
        /// </summary>
        [TestMethod]
        public void CreateCompensation_Persistance_Returns_Created_Ok()
        {
            // Arrange
            var employeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c";
            decimal salary = 100001.01m;
            DateTimeOffset effectiveDate = DateTimeOffset.Parse("10/13/2021 8:09:49 PM");
            var compensation = new Compensation() { EmployeeId = employeeId, EffectiveDate = effectiveDate, Salary = salary };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/createCompensation",
                    new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var postResponse = postRequestTask.Result;

            var getRequestTask = _httpClient.GetAsync($"api/employee/getCompensation/{employeeId}");
            var getResponse = getRequestTask.Result;

            var newCompensation = getResponse.DeserializeContent<Compensation>(); ;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.AreEqual(newCompensation.Employee.EmployeeId, employeeId);
            Assert.AreEqual(newCompensation.Salary, salary);
            Assert.AreEqual(newCompensation.EffectiveDate.Ticks, effectiveDate.Ticks);

        }
    }
}
