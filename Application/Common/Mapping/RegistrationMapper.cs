using Application.DTO.Registrations;
using Domain.Entity;
using static Application.Common.Mapping.MappingHelpers;

namespace Application.Common.Mapping
{
    public static class RegistrationMapper
    {
        public static StudentRegistrationResponse ToStudentResponse(Registration registration) => new()
        {
            SubjectID = registration.SubjectID,
            SubjectName = registration.Subject != null ? registration.Subject.Name : string.Empty,
            TermID = registration.TermID,
            TermName = registration.Term != null ? registration.Term.Name : string.Empty,
            Grade = registration.Exam != null ? registration.Exam.Grade : null,
            TeacherFullName = registration.Exam?.Teacher != null
                ? FullName(registration.Exam.Teacher)
                : null
        };

        public static TeacherRegistrationResponse ToTeacherResponse(Registration registration) => new()
        {
            StudentID = registration.StudentID,
            StudentName = FullName(registration.Student),
            StudentIndexNumber = registration.Student != null ? registration.Student.IndexNumber : string.Empty,
            HasExam = registration.Exam != null,
            ExamID = registration.Exam?.ID,
            ExamDate = registration.Exam?.Date,
            Grade = registration.Exam?.Grade,
            Note = registration.Exam?.Note,
            SignedAt = registration.Exam?.SignedAt
        };

        public static StudServiceRegistrationResponse ToStudServiceResponse(Registration registration) => new()
        {
            StudentID = registration.StudentID,
            StudentName = FullName(registration.Student),
            StudentIndexNumber = registration.Student != null ? registration.Student.IndexNumber : string.Empty,
            SubjectID = registration.SubjectID,
            TermID = registration.TermID,
            RegisteredAt = registration.RegisteredAt,
            CancelledAt = registration.CancelledAt
        };
    }
}
