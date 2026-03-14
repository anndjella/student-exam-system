using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class StudentStats
    {
        public int StudentID { get; set; }
        public decimal? GPA { get; set; }
        public int? ECTSCount { get; set; }
    }

}
