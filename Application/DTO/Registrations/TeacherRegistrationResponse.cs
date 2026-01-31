using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Registrations
{
    public sealed class TeacherRegistrationResponse
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentIndexNumber { get; set; } = "";
        public bool HasExam { get; set; }
        public int? ExamID { get; set; }
        public DateOnly? ExamDate { get; set; }
        public int? Grade { get; set; }
        public string? Note { get; set; }
        public DateTime? SignedAt { get; set; }
    }
}
