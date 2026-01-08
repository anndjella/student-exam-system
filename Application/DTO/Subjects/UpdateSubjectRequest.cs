using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Subjects
{
    public class UpdateSubjectRequest
    {
        public string? Name { get; set; }
        public byte? ECTS { get; set; }
    }
}
