using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Exams
{
    public sealed class LockExamsRequest
    {
        public int SubjectID { get; set; }
        public int TermID { get; set; }
    }
}
