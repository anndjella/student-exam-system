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
        public int StudentID { get; private set; }

        public byte Grade { get; private set; }
        public DateTime Date { get; private set; }

        public Student Student { get; private set; }

        public int ExaminerID { get; private set; }
        public Teacher Examiner { get; private set; }

        public int? SupervisorID { get; private set; }
        public Teacher? Supervisor { get; private set; }
        public int SubjectID { get; private set; }
        public Subject Subject { get; private set; }

    }
}
