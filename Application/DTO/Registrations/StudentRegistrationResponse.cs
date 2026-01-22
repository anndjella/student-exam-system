using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Registrations
{
    public sealed class StudentRegistrationResponse
    {
        public string SubjectName { get; set; } = "";
        public string TermName { get; set; } = "";
    }
}
