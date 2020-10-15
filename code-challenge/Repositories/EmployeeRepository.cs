using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using challenge.Data;

namespace challenge.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<IEmployeeRepository> _logger;

        public EmployeeRepository(ILogger<IEmployeeRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Employee Add(Employee employee)
        {
            employee.EmployeeId = Guid.NewGuid().ToString();
            _employeeContext.Employees.Add(employee);
            return employee;
        }

        /// <summary>
        /// I updated this to correctly return the entire tree of DirectReports as this seemed the intended result.
        /// My preference here would have been to either build some SQL to move the heavy lifting to the DB or 
        /// structure the data differntly to allow for a linq query to mine the direct reports.
        /// But sometimes we have to play the hand we're dealt.
        /// </summary>

        public Employee GetById(string id)
        {
            var employee =  _employeeContext.Employees.Include(i=>i.DirectReports).SingleOrDefault(e => e.EmployeeId == id);
            
            
            if (employee!=null && employee.DirectReports!=null)
            {
                foreach(var directReport in employee.DirectReports)
                {
                    directReport.DirectReports = GetById(directReport.EmployeeId)?.DirectReports;
                }
            }
            return employee;
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }

        public Employee Remove(Employee employee)
        {
            return _employeeContext.Remove(employee).Entity;
        }
    }
}
