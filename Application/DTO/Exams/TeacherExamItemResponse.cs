using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public class TeacherExamItemResponse
    {
        public int ID { get; set; }

        public int StudentId { get; set; }
        public string StudentIndexNum { get; set; } = "";
        public string StudentFullName { get; set; } = "";

        public DateOnly? ExamDate { get; set; } 
        public byte? Grade { get; set; }
        public string? Note { get; set; }= "";

        public DateTime? SignedAt { get; set; }

        public string? EnteredByTeacherName { get; set; }
        public string? EnteredByEmployeeNumber { get; set; }
    }
}
