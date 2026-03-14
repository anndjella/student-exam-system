using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.TeachingAssignment
{
    public sealed class TeachingAssignmentResponse
    {
        public int SubjectID {  get; set; }
        public string SubjectName { get; set; } = "";
        public int TeacherID {  get; set; }
        public string TeacherName { get; set; } = "";
        public string TeacherEmployeeNum { get; set; } = "";
        public bool CanGrade { get; set; }
    }
}
