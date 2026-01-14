using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Enrollments
{
    public class EnrollmentResponse
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public byte SubjectECTS { get; set; }
    }
}
