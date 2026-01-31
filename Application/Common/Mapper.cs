using Application.DTO.Enrollments;
using Application.DTO.Exams;
using Application.DTO.Registrations;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.DTO.Teachers;
using Application.DTO.TeachingAssignment;
using Application.DTO.Term;
using Domain.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public static class Mapper
    {
        public static StudentResponse StudentToResponse(Student s) => new()
        {
            ID = s.ID,
            FirstName = s.FirstName,
            LastName = s.LastName,
            DateOfBirth = s.DateOfBirth,
            GPA = null,
            ECTSCount = null,
            IndexNumber = s.IndexNumber
        };
        public static StudentResponse StudentToResponseWithStats(Student s, StudentStats? stats) => new()
        {
            ID = s.ID,
            FirstName = s.FirstName,
            LastName = s.LastName,
            DateOfBirth=s.DateOfBirth,
            GPA = stats?.GPA is null ? null : (double)stats.GPA.Value,
            ECTSCount = stats?.ECTSCount,
            IndexNumber = s.IndexNumber
        };
        public static TeacherResponse TeacherToResponse(Teacher s) => new()
        {
            ID = s.ID,
            FirstName = s.FirstName,
            DateOfBirth=s.DateOfBirth,
            LastName = s.LastName,
            EmployeeNumber = s.EmployeeNumber,
            Title = s.Title
        };
        internal static SubjectResponse SubjectToResponse(Subject s) => new()
        {
            ID = s.ID,
            Name = s.Name,
            ECTS = s.ECTS,
            Code = s.Code,
            Teachers = (s.TeachingAssignments ?? Enumerable.Empty<TeachingAssignment>())
                    .Where(ta => ta.Teacher != null)
                    .Select(ta => new SubjectTeacherItem
                    {
                        ID = ta.TeacherID,
                        FirstName = ta.Teacher!.FirstName,
                        LastName = ta.Teacher!.LastName,
                    })
                    .ToList()
        };
        internal static AdminSubjectResponse SubjectToAdminResponse(Subject s) => new()
        {
            ID = s.ID,
            Name = s.Name,
            ECTS = s.ECTS,
            Code = s.Code,
            IsActive=s.IsActive,
            Teachers = (s.TeachingAssignments ?? Enumerable.Empty<TeachingAssignment>())
                    .Where(ta => ta.Teacher != null)
                    .Select(ta => new AdminSubjectTeacherItem
                    {
                        ID = ta.TeacherID,
                        FirstName = ta.Teacher!.FirstName,
                        LastName = ta.Teacher!.LastName,
                        EmployeeNumber=ta.Teacher!.EmployeeNumber,
                        Title=ta.Teacher!.Title,
                        CanGrade=ta.CanGrade
                    })
                    .ToList()
        };

        public static ExamResponse ExamToResponse(Exam e) => new()
        {
            ID = e.ID,
            StudentID =  e.StudentID,

            SubjectID = e.SubjectID,
            SubjectName = e.Registration?.Subject?.Name ?? string.Empty,

            TermId = e.TermID,
            TermName = e.Registration?.Term?.Name ?? string.Empty,

            TeacherID = e.TeacherID,
            TeacherFullName = e.Teacher != null
                 ? $"{e.Teacher.FirstName} {e.Teacher.LastName}"
                 : string.Empty,

            Grade = e.Grade,
            Date = e.Date,
            Note = e.Note,
            SignedAt = e.SignedAt
        };
        internal static StudentExamItemResponse StudentExamToResponse(Exam e) => new()
        {
            ID = e.ID,
            SubjectCode = e.Registration?.Subject?.Code ?? string.Empty,
            SubjectName = e.Registration?.Subject?.Name ?? string.Empty,
            SubjectECTS = e.Registration?.Subject?.ECTS ?? 0,

            Date = e.Date,
            Grade = e?.Grade,
            Note = e?.Note,
            TeacherName = $"{e?.Teacher?.FirstName} {e?.Teacher?.LastName}",
            TermName = e.Registration?.Term?.Name ?? string.Empty
        };
        internal static StudServiceExamItemResponse StudServiceExamToResponse(Exam e) => new()
        {
            ID = e.ID,

            StudentName =$"{e.Registration?.Student?.FirstName}  {e.Registration?.Student?.LastName}" ?? string.Empty,
            StudentIndexNum=e.Registration?.Student?.IndexNumber ?? string.Empty,
            
            SubjectCode = e.Registration?.Subject?.Code ?? string.Empty,
            SubjectName = e.Registration?.Subject?.Name ?? string.Empty,
            SubjectECTS = e.Registration?.Subject?.ECTS ?? 0,

            Date = e.Date,
            Grade = e?.Grade,
            Note = e?.Note,
            SignedAt=e?.SignedAt,
            TeacherEmployeeNum=e?.Teacher?.EmployeeNumber ?? string.Empty,
            TeacherName = $"{e?.Teacher?.FirstName} {e?.Teacher?.LastName}",
            TermName = e?.Registration?.Term?.Name ?? string.Empty
        };

        public static Student CreateToStudent(CreateStudentRequest req, int id) => new()
        {
            ID = id,
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            IndexNumber = req.IndexNumber
        };

        internal static TeachingAssignmentResponse TeachingAssignmentToResponse(TeachingAssignment ta) => new()
        {
            SubjectID = ta.SubjectID,
            SubjectName = ta?.Subject != null ? ta.Subject.Name : string.Empty,
            TeacherID = ta.TeacherID,
            TeacherEmployeeNum = ta.Teacher != null ? ta.Teacher.EmployeeNumber : string.Empty,
            TeacherName = ta.Teacher != null ? $"{ta.Teacher.FirstName} {ta.Teacher.LastName}" : string.Empty,
            CanGrade = ta.CanGrade
        };
        internal static EnrollmentResponse EnrollmentToResponse(Enrollment e) => new()
        {
            SubjectID = e.SubjectID,         
            SubjectName = e.Subject != null ? e.Subject.Name : string.Empty,
            SubjectECTS = e.Subject?.ECTS ?? 0
        };
        internal static StudentRegistrationResponse StudentRegistrationToResponse(Registration e) => new()
        {
            SubjectID=e.SubjectID,
            SubjectName = e.Subject != null ? e.Subject.Name : string.Empty,
            TermID=e.TermID,
            TermName = e.Term != null ? e.Term.Name : string.Empty,
        };
        internal static TeacherRegistrationResponse TeacherRegistrationToResponse(Registration e) => new()
        {
            StudentID = e.StudentID,
            StudentName = e.Student != null ? $"{e.Student.FirstName} {e.Student.LastName}" : string.Empty,
            StudentIndexNumber = e.Student != null ? e.Student.IndexNumber : string.Empty ,
            HasExam = e?.Exam != null,
            ExamID =e?.Exam?.ID,
            ExamDate=e?.Exam?.Date,
            Grade=e?.Exam?.Grade,
            Note = e?.Exam?.Note,
            SignedAt = e?.Exam?.SignedAt
        };
        internal static StudServiceRegistrationResponse StudServiceRegistrationToResponse(Registration e) => new()
        {
            StudentID = e.StudentID,
            StudentName = e.Student != null ? $"{e.Student.FirstName} {e.Student.LastName}" : string.Empty,
            StudentIndexNumber = e.Student != null ? e.Student.IndexNumber : string.Empty,
            SubjectName = e.Subject != null ? e.Subject.Name : string.Empty,
            SubjectID=e.SubjectID,
            TermID=e.TermID,
            TermName = e.Term != null ? e.Term.Name : string.Empty,
            RegisteredAt=e.RegisteredAt 
        };
        internal static TermResponse TermToResponse(Term term) => new()
        {
            TermID = term.ID,
            TermName = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            RegistrationEndDate = term.RegistrationEndDate,
            RegistrationStartDate = term.RegistrationStartDate
        };

    }
}
