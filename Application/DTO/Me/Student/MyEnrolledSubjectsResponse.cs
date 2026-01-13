using Application.DTO.Enrollments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Student
{
    public sealed class MyEnrolledSubjectsResponse
    {
        public List<EnrollmentResponse> Passed { get; set; } = new();
        public List<EnrollmentResponse> NotPassed { get; set; } = new();
    }
}
