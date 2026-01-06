using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Exam : IEntity
    {
        public int ID { get; set; }
        public DateTime? SignedAt { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int TermID { get; set; }
        public Registration Registration { get; set; } = null!;
        public int TeacherID { get; set; }
        public Teacher Teacher { get; set; } = null!;
        public byte? Grade { get; set; } = null!;
        public DateOnly Date { get; set; }
        public string? Note { get; set; }
    }
}
