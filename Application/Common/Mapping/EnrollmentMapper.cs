using Application.DTO.Enrollments;
using Domain.Entity;
using static Application.Common.Mapping.MappingHelpers;

namespace Application.Common.Mapping
{
    public static class EnrollmentMapper
    {
        public static EnrollmentResponse ToResponse(Enrollment enrollment) => new()
        {
            SubjectID = enrollment.SubjectID,
            SubjectCode = enrollment.Subject != null ? enrollment.Subject.Code : string.Empty,
            SubjectName = enrollment.Subject != null ? enrollment.Subject.Name : string.Empty,
            SubjectECTS = enrollment.Subject?.ECTS ?? 0,
            StudentID = enrollment.StudentID,
            StudentName = FullName(enrollment.Student),
            StudentIndex = enrollment.Student != null ? enrollment.Student.IndexNumber : string.Empty,
            IsPassed = enrollment.IsPassed,
            CreatedAt = enrollment.CreatedAt,
            PassedAt = enrollment.PassedAt
        };
    }
}
