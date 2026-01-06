using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.DTO.Teachers
{
    public sealed class TeacherResponse
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Title Title { get; set; }
        public string EmployeeNumber { get; set; } = "";
    }
}
