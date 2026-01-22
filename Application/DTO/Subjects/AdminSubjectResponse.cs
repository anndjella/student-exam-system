using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Subjects
{
    public sealed class AdminSubjectsResponse
    {
        public List<AdminSubjectResponse> Active { get; set; } = new();
        public List<AdminSubjectResponse> Inactive { get;set; } = new();
    }
    public sealed class AdminSubjectResponse
    {
        public int ID { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int ECTS { get; set; }
        public bool IsActive{ get; set; }
        public List<AdminSubjectTeacherItem> Teachers { get; set; } = new();
    }

    public sealed class AdminSubjectTeacherItem
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Title Title { get; set; }
        public string EmployeeNumber { get; set; } = "";
        public bool CanGrade { get; set; }
    }
}
