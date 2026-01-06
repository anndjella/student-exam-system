using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Enums;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Enrollment
    {
        public int StudentID { get; set; }
        public Student Student { get; set; } = null!;
        public int SubjectID { get; set; }
        public Subject Subject { get; set; } = null!;
        public int SchoolYearID { get; set; }
        public SchoolYear SchoolYear { get; set; } = null!;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    }
}
