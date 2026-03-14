using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Subjects
{
    public class CreateSubjectRequest
    {
        public string Name { get; set; } = "";
        public byte ECTS { get; set; }
        public string Code { get; set; } = "";
    }
}
