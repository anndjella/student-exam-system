using Application.DTO.Exams;
using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Student
{
    public sealed class StudentExamsResponse
    {
        public List<StudentExamItemResponse> Passed { get; set; } = new();
        public List<StudentExamItemResponse> NotPassed { get; set; } = new();
    }
}
