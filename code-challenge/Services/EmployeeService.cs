using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using challenge.Repositories;

namespace challenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();
                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        public ReportingStructure GetReportingStructure(string id)
        {
            var reportingStructure = new ReportingStructure();

            if (String.IsNullOrEmpty(id))
            {
                return null;
            }

            var employee = _employeeRepository.GetById(id);
            if (employee!=null)
            {
                reportingStructure.Employee = employee;    
            } 
            else
            {
                //Employee Not found
                return null;
            }
            
            return reportingStructure;
        }
        
        public Compensation GetCompensationById(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return null;
            }

            var compensation = _employeeRepository.GetCompensation(id);
           
            return compensation;
        }

        public Compensation CreateCompensation(Compensation compensation)
        {
            if (_employeeRepository.GetCompensation(compensation.EmployeeId)!=null)
            {
                _logger.LogDebug($"Compensation already exists on employee { compensation.EmployeeId} ");
                throw new Exception($"Compensation already exists on employee {compensation.EmployeeId}");
            }
            var employee = GetById(compensation.EmployeeId);
            if (employee==null)
            {
                _logger.LogDebug($"Employee does not exist { compensation.EmployeeId} ");
                throw new Exception($"Employee does not exist { compensation.EmployeeId}");
            }
            var newCompensation = _employeeRepository.CreateCompensation(compensation);
            _employeeRepository.SaveAsync().Wait();

            newCompensation.Employee = employee;
            
            return newCompensation;
        }
    }
}
