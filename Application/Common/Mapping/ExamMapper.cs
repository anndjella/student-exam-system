using Application.DTO.Exams;
using Domain.Entity;
using static Application.Common.Mapping.MappingHelpers;

namespace Application.Common.Mapping
{
    public static class ExamMapper
    {
        public static TeacherExamItemResponse ToTeacherResponse(Exam exam) => new()
        {
            ID = exam.ID,
            StudentId = exam.StudentID,
            StudentFullName = FullName(exam.Registration?.Student),
            StudentIndexNum = exam.Registration?.Student?.IndexNumber ?? string.Empty,
            EnteredByTeacherName = FullName(exam.Teacher),
            EnteredByEmployeeNumber = exam.Teacher != null ? exam.Teacher.EmployeeNumber : string.Empty,
            Grade = exam.Grade,
            ExamDate = exam.Date,
            Note = exam.Note,
            SignedAt = exam.SignedAt
        };

        public static StudentExamItemResponse ToStudentResponse(Exam exam) => new()
        {
            ID = exam.ID,
            SubjectCode = exam.Registration?.Subject?.Code ?? string.Empty,
            SubjectName = exam.Registration?.Subject?.Name ?? string.Empty,
            SubjectECTS = exam.Registration?.Subject?.ECTS ?? 0,
            Date = exam.Date,
            Grade = exam.Grade,
            Note = exam.Note,
            TeacherName = FullName(exam.Teacher),
            TermName = exam.Registration?.Term?.Name ?? string.Empty
        };

        public static StudServiceExamItemResponse ToStudServiceResponse(Exam exam) => new()
        {
            ID = exam.ID,
            StudentName = FullName(exam.Registration?.Student),
            StudentIndexNum = exam.Registration?.Student?.IndexNumber ?? string.Empty,
            Date = exam.Date,
            Grade = exam.Grade,
            Note = exam.Note,
            SignedAt = exam.SignedAt,
            TeacherEmployeeNum = exam.Teacher?.EmployeeNumber ?? string.Empty,
            TeacherName = FullName(exam.Teacher),
        };
    }
}
