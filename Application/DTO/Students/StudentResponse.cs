using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Students
{
    public sealed class StudentResponse
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateOnly DateOfBirth { get; set; }
        public double? GPA { get; set; }
        public int? ECTSCount { get; set; }
        public string IndexNumber { get; set; } = "";
    }
}
