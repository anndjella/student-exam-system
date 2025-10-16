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
        public string IndexNumber { get; set; } = "";
        public double? GPA { get; set; }
        public ICollection<Exam> Exams { get;  set; } = new List<Exam>();
    }
}
