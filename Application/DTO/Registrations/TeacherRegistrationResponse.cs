using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Registrations
{
    public sealed class TeacherRegistrationResponse
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentIndexNumber { get; set; } = "";
    }
}
