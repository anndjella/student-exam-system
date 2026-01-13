using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.TeachingAssignment
{
    public sealed class CreateTeachingAssignmentRequest
    {
        public int TeacherID { get;set; }
        public int SubjectID { get;set; }
        public bool CanGrade { get; set; } = false;
    }
}
