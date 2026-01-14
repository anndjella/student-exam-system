using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public class ExamResponse
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public int TermId { get; set; }
        public string TermName { get; set; } = "";
        public int TeacherID { get; set; }
        public string TeacherFullName { get; set; } = "";
        public byte? Grade { get; set; }
        public DateOnly Date { get; set; }
        public string? Note { get; set; } = "";
        public DateTime? SignedAt {  get; set; }
    }
}
