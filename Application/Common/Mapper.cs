using Application.DTO.Enrollments;
using Application.DTO.Exams;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.DTO.Teachers;
using Application.DTO.TeachingAssignment;
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
            GPA = s.GPA,
            ECTSCount = s.ECTSCount,
            IndexNumber = s.IndexNumber
        };
        public static TeacherResponse TeacherToResponse(Teacher s) => new()
        {
            ID = s.ID,
            FirstName = s.FirstName,
            LastName = s.LastName,
            EmployeeNumber = s.EmployeeNumber,
            Title = s.Title
        };
        public static SubjectResponse SubjectToResponse(Subject s) => new()
        {
            ID = s.ID,
            Name = s.Name,
            ESPB = s.ECTS
        };
        //public static ExamResponse ExamToResponse(Exam e) => new()
        //{
        //    StudentID=e.StudentID,
        //    SubjectID=e.SubjectID,
        //    Grade = e.Grade,
        //    Date = e.Date,
        //    Note = e.Note,
        //    StudentIndex = e.Student?.IndexNumber ?? string.Empty,

        //    StudentFullName = e.Student != null ? $"{e.Student.FirstName} {e.Student.LastName}".Trim() : string.Empty,
        //    SubjectName = e.Subject?.Name ?? string.Empty,
        //    ExaminerFullName = e.Examiner != null ? $"{e.Examiner.FirstName} {e.Examiner.LastName}".Trim() : string.Empty,
        //    SupervisorFullName = e.Supervisor != null ? $"{e.Supervisor.FirstName} {e.Supervisor.LastName}".Trim() : string.Empty,
        //};

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
            SubjectName = ta.Subject != null ? ta.Subject.Name : string.Empty,
            TeacherID = ta.TeacherID,
            TeacherEmployeeNum = ta.Teacher != null ? ta.Teacher.EmployeeNumber : string.Empty,
            TeacherName = ta.Teacher != null ? $"{ta.Teacher.FirstName} {ta.Teacher.LastName}" : string.Empty,
            CanGrade = ta.CanGrade

        };
        internal static EnrollmentResponse EnrollmentToResponse(Enrollment e) => new()
        {
            SubjectID = e.SubjectID,
            SubjectName = e.Subject != null ? e.Subject.Name : string.Empty,
            ECTS = e.Subject != null ? e.Subject.ECTS : 0,
            SchoolYearID = e.SchoolYearID,  
            SchoolYearName=e.SchoolYear != null ? $"{e.SchoolYear.StartDate.Year}/{e.SchoolYear.EndDate.Year}" : string.Empty,
            Status=e.Status
        };
        //public static Teacher CreateToTeacher(CreateTeacherRequest req, int id) => new()
        //{
        //    ID = id,
        //    JMBG = req.JMBG,
        //    FirstName = req.FirstName,
        //    LastName = req.LastName,
        //    DateOfBirth = req.DateOfBirth,
        //    Title = req.Title
        //};
        //public static StudentResponse CreateToStudentResponse(CreateStudentRequest req, int id,int age,double gpa) => new()
        //{
        //    Id=id,
        //    IndexNumber=req.IndexNumber,
        //    FirstName=req.FirstName,
        //    LastName = req.LastName,
        //    Age=age,
        //    Gpa=gpa
        //};
    }
}
