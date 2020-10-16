using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace challenge.Models
{
    public class Compensation
    {
        [Key]
        public string EmployeeId {get;set; }
        public decimal Salary { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public Employee Employee { get; set; }
    }
}
