using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Students
{
    public sealed class StudentResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public double? Gpa { get; set; }
        public string IndexNumber { get; set; } = "";
    }
}
