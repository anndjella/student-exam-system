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
        public int ESPB { get; set; }
    }
}
