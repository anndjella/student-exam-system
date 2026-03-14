using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Enrollments
{
    public sealed class CreateEnrollmentRequest
    {
        public string StudentIndex { get; set; } = "";
        public string SubjectCode { get; set; } = "";
    }
}
