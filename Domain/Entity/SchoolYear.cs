using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class SchoolYear :IEntity
    {
        public int ID {  get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set;}
        public ICollection<Enrollment> Enrollments { get; set; } =new List<Enrollment>();
    }
}
