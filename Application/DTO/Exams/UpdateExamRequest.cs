using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public class UpdateExamRequest
    {
        public byte? Grade { get; set; }
        public string? Note { get; set; }
    }
}
