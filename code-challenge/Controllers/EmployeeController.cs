using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using challenge.Services;
using challenge.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace challenge.Controllers
{
    [Route("api/employee")]
    public class EmployeeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        
        [HttpGet("getReportingStructure/{id}", Name = "getReportingStructure")]
        public IActionResult GetReportingStructure(String id)
        {
            _logger.LogDebug($"Recieved reporting structure get request for '{id}'");

            var reportingStructure = _employeeService.GetReportingStructure(id);

            if (reportingStructure == null)
            {
                return NotFound();
            }

            return Ok(reportingStructure);
        }
        
        [HttpGet("getCompensation/{id}", Name = "getCompensation")]
        public IActionResult GetCompensation(String id)
        {
            _logger.LogDebug($"Recieved compensation get request for '{id}'");

            var compensation = _employeeService.GetCompensationById(id);

            if (compensation == null)
            {
                return NotFound();
            }

            return Ok(compensation);
        }

        [HttpPost("createCompensation", Name = "createCompensation")]
        public IActionResult CreateCompensation([FromBody] Compensation compensation)
        {
            //Validate
            if (compensation == null || string.IsNullOrEmpty(compensation.EmployeeId))
            {
                _logger.LogDebug($"Received compensation no employee information");
                return NotFound();
            }
            
            _logger.LogDebug($"Received compensation create request for '{compensation.EmployeeId}'");

            try
            {
                _employeeService.CreateCompensation(compensation);
            } catch(Exception e)
            {
                return BadRequest(e);
            }

            return CreatedAtRoute("getCompensation", new { id = compensation.EmployeeId },compensation);
        }
    }
}
