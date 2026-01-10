using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Student
{
    public sealed class MyEnrolledSubjectItem
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public int ECTS { get; set; }

        public int SchoolYearID { get; set; }
        public string SchoolYearName { get; set; } = "";

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    }
}
