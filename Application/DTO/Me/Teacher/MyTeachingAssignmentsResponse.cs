using Application.DTO.TeachingAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Teacher
{
    public sealed class MyTeachingAssignmentsResponse
    {
        public List<TeachingAssignmentResponse> GradableSubjects { get; set; } = new();
        public List<TeachingAssignmentResponse> NonGradableSubjects { get; set; } = new();
    }
}
