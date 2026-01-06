using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public class ExamResponse
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public byte Grade { get; set; }
        public DateOnly Date { get; set; }
        public string? Note { get; set; } = "";
        public string StudentIndex { get; set; } = "";
        public string StudentFullName { get; set; } = "";
        public string SubjectName { get; set; } = "";
        //public string ExaminerFullName { get; set; } = "";
        //public string SupervisorFullName { get; set; } = "";
    }
}
