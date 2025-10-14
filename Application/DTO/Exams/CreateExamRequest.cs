using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public class CreateExamRequest
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int ExaminerID { get; set; }
        public int? SupervisorID { get; set; }
        public byte Grade { get; set; }
        public DateOnly Date { get; set; }
        public string? Note { get; set; }
    }
}
