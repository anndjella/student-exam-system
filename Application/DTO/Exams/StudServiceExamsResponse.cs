using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public sealed class StudServiceExamsResponse
    {
        public int UnsignedCount { get; set; }
        public int Total { get; set; }
        public List<StudServiceExamItemResponse> Exams { get; set; } = new();
    }
}
