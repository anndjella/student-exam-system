using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Enums;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Domain.Entity
{
    public sealed class Teacher :Person
    {
        public Title Title { get; set; }
        public string EmployeeNumber { get; set; } = null!;
        public ICollection<Exam> SignedExams { get; private set; } = new List<Exam>();
        public ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
    }
}
