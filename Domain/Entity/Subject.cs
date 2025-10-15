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
        public string Name { get; set; } = "";
        public int ESPB { get; set; }
        public ICollection<Exam> Exams { get; private set; } = new List<Exam>();
    }
}
