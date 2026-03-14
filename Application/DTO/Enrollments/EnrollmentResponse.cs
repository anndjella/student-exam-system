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
        public string SubjectCode { get; set; } = "";
        public byte SubjectECTS { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentIndex { get; set; } = "";
        public bool IsPassed { get;set; }
        public DateTime? PassedAt { get;set; }
        public DateTime CreatedAt { get; set; }
    }
}
