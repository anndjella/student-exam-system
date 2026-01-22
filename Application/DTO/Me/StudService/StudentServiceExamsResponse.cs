using Application.DTO.Exams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Me.StudService
{
    public sealed class StudentServiceExamsResponse
    {
        public int UnsignedCount { get; set; }
        public List<StudServiceExamItemResponse> Exams { get; set; } = new();
    }
}
