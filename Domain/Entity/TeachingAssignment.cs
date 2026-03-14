using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class TeachingAssignment
    {
        public bool CanGrade { get; set; } = false;
        public int SubjectID {  get; set; }
        public Subject Subject { get; set; } = null!;
        public int TeacherID {  get; set; }
        public Teacher Teacher { get; set; } = null!;
    }
}
