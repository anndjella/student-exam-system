using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Student : Person
    {
        public string IndexNumber { get; set; } = null!;
        [NotMapped]
        public double? GPA { get; set; }
        [NotMapped]
        public int? ECTSCount { get; set; }
        public ICollection<Enrollment> Enrollments { get;  set; } = new List<Enrollment>();
        public ICollection<Registration> Registrations { get;  set; } = new List<Registration>();
    }
}
