using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Registrations
{
    public sealed class StudServiceRegistrationResponse
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentIndexNumber { get; set; } = "";
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public int TermID { get; set; }
        public string TermName { get; set; } = "";
        public DateTime RegisteredAt { get; set; }
    }
}
