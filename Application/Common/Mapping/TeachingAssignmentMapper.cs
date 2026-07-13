using Application.DTO.TeachingAssignment;
using Domain.Entity;
using static Application.Common.Mapping.MappingHelpers;

namespace Application.Common.Mapping
{
    public static class TeachingAssignmentMapper
    {
        public static TeachingAssignmentResponse ToResponse(TeachingAssignment assignment) => new()
        {
            SubjectID = assignment.SubjectID,
            SubjectName = assignment.Subject != null ? assignment.Subject.Name : string.Empty,
            TeacherID = assignment.TeacherID,
            TeacherEmployeeNum = assignment.Teacher != null ? assignment.Teacher.EmployeeNumber : string.Empty,
            TeacherName = FullName(assignment.Teacher),
            CanGrade = assignment.CanGrade
        };
    }
}
