using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Subject : IEntity
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public byte ECTS { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
 
    }
}
