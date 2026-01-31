using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me
{
    public sealed class MeResponse
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string JMBG { get; set; } = "";
        public DateOnly DateOfBirth { get; set; }

        //student
        public string? IndexNumber { get; set; }
        public decimal? GPA { get; set; }
        public int? ECTSCount { get; set; }

        // teacher
        public string? EmployeeNumber { get; set; }
        public Title? Title { get; set; }
    }
}
