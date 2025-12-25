using Application.DTO.Exams;
using Application.DTO.Students;
using Application.DTO.Subjects;
using Application.DTO.Teachers;
using Domain.Entity;
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
            Id = s.ID,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Age = s.Age,
            Gpa = s.GPA,
            IndexNumber = s.IndexNumber
        };
        public static TeacherResponse TeacherToResponse(Teacher s) => new()
        {
            Id = s.ID,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Title = s.Title
        };
        public static SubjectResponse SubjectToResponse(Subject s) => new()
        {
            Id = s.ID,
            Name = s.Name,
            ESPB = s.ESPB
        };
        public static ExamResponse ExamToResponse(Exam e) => new()
        {
            StudentID=e.StudentID,
            SubjectID=e.SubjectID,
            Grade = e.Grade,
            Date = e.Date,
            Note = e.Note,
            StudentIndex = e.Student?.IndexNumber ?? string.Empty,

            StudentFullName = e.Student != null ? $"{e.Student.FirstName} {e.Student.LastName}".Trim() : string.Empty,
            SubjectName = e.Subject?.Name ?? string.Empty,
            ExaminerFullName = e.Examiner != null ? $"{e.Examiner.FirstName} {e.Examiner.LastName}".Trim() : string.Empty,
            SupervisorFullName = e.Supervisor != null ? $"{e.Supervisor.FirstName} {e.Supervisor.LastName}".Trim() : string.Empty,
        };

        public static Student CreateToStudent(CreateStudentRequest req, int id) => new()
        {
            ID = id,
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            DateOfBirth = req.DateOfBirth,
            IndexNumber = req.IndexNumber
        };
        public static Teacher CreateToTeacher(CreateTeacherRequest req, int id) => new()
        {
            ID = id,
            JMBG = req.JMBG,
            FirstName = req.FirstName,
            LastName = req.LastName,
            DateOfBirth = req.DateOfBirth,
            Title = req.Title
        };
        public static StudentResponse CreateToStudentResponse(CreateStudentRequest req, int id,int age,double gpa) => new()
        {
            Id=id,
            IndexNumber=req.IndexNumber,
            FirstName=req.FirstName,
            LastName = req.LastName,
            Age=age,
            Gpa=gpa
        };
    }
}
