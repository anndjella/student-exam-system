using Application.DTO.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Teacher
{
    public sealed class TeacherSubjectsResponse
    {
        public List<SubjectResponse> GradableSubjects { get; set; } = new();
        public List<SubjectResponse> NonGradableSubjects { get; set; } = new();
    }
}
