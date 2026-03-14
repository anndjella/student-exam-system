using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Subjects
{
    public sealed class StudServiceSubjectResponse
    {
        public int ID { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int ECTS { get; set; }
        public bool IsActive{ get; set; }
        public List<StudServiceSubjectTeacherItem> Teachers { get; set; } = new();
    }

    public sealed class StudServiceSubjectTeacherItem
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Title Title { get; set; }
        public string EmployeeNumber { get; set; } = "";
        public bool CanGrade { get; set; }
    }
}
