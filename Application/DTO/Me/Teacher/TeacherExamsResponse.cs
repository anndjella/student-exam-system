using Application.DTO.Exams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.Teacher
{
    public sealed class TeacherExamsResponse
    {
        public List<TeacherExamItemResponse> Mine { get; set; } = new();
        public List<TeacherExamItemResponse> Others { get; set; } = new();
    }
}
