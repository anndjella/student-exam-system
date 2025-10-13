using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Students
{
    public sealed class UpdateStudentRequest : IPersonUpdate
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? IndexNumber { get; set; }
    }
}
