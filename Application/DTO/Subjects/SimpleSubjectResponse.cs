using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Subjects
{
    public sealed class SimpleSubjectResponse
    {
        public int ID { get; set; }
        public string Code { get;set; } = "";
        public string Name { get; set; } = "";
    }
}
