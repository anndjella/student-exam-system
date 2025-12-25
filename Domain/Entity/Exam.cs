using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Exam
    {
        public int StudentID { get; set; }

        public byte Grade { get;  set; }
        public DateOnly Date { get; set; }
        public string? Note { get; set; }

        public Student Student { get; set; } = null!;

        public int ExaminerID { get;  set; }
        public Teacher Examiner { get;  set; } = null!;

        public int? SupervisorID { get;  set; }
        public Teacher? Supervisor { get;  set; }
        public int SubjectID { get;  set; }
        public Subject Subject { get;  set; } = null!;

    }
}
