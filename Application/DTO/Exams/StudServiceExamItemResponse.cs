using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public sealed class StudServiceExamItemResponse
    {
        public int ID { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentIndexNum { get; set; } = "";
        public string SubjectCode { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public byte SubjectECTS { get; set; }
        public byte? Grade { get; set; }
        public string? Note { get; set; } = "";
        public string TermName { get; set; } = "";
        public DateOnly Date { get; set; }
        public string TeacherName { get; set; } = "";
        public string TeacherEmployeeNum { get; set; } = "";
        public DateTime? SignedAt { get; set; }
    }
}
