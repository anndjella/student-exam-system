using Application.DTO.Teachers;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Subjects
{
    public sealed class SubjectResponse
    {
        public int ID { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int ECTS { get; set; }
        public List<SubjectTeacherItem> Teachers { get; set; } = new();
    }
    public sealed class SubjectTeacherItem
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
}
