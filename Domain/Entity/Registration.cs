using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Enums;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Registration
    {
        public int StudentID { get; set; }
        public Student Student { get; set; } = null!;
        public int SubjectID { get; set; }
        public Subject Subject { get; set; } = null!;
        public int TermID { get; set; }
        public Term Term { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt {  get; set; }
        public DateTime? CancelledAt { get; set;}
        public Exam? Exam { get; set; }
    }
}
