using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Teacher :Person
    {
        public Title Title { get; set; }

        public ICollection<Exam> ExamsAsExaminer { get; private set; } = new List<Exam>();
        public ICollection<Exam> ExamsAsSupervisor { get; private set; } = new List<Exam>();

    }
}
