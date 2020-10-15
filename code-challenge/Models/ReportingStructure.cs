
using System.Linq;
/// <summary>
/// A container for an employee's reporting structure
/// </summary>
namespace challenge.Models
{
    public class ReportingStructure
    {

        public Employee Employee { get; set; }
        public int NumberOfReports { 
            get
            {
                return CountReportsRecursive(this.Employee);
            }
        }

        private int CountReportsRecursive(Employee employee)
        {
            if (employee==null)
            {
                return 0;
            }

            int numberReports = employee.DirectReports.Count();
            foreach(var directReport in employee.DirectReports)
            {
                numberReports += CountReportsRecursive(directReport);
            }

            return numberReports;
        }

    }
}
