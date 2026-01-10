using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Student
{
    public sealed class MyEnrolledSubjectsResponse
    {
        public List<MyEnrolledSubjectItem> Passed { get; set; } = new();
        public List<MyEnrolledSubjectItem> NotPassed { get; set; } = new();
    }
}
