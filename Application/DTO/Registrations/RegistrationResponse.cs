using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Registrations
{
    public sealed class RegistrationResponse
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public int TermID { get; set; }
        public string TermName { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
