using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class Term :IEntity
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly RegistrationStartDate { get; set; }
        public DateOnly RegistrationEndDate { get; set; }
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
