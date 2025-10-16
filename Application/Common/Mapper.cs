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
            ID = e.ID,
            Grade = e.Grade,
            Date = e.Date,
            Note = e.Note,
            StudentIndex = e.Student?.IndexNumber ?? "",
            StudentFullName= e.Student?.FirstName + " " + e.Student?.LastName ?? "",
            SubjectName = e.Subject?.Name ?? "",
            ExaminerFullName = e.Examiner?.FirstName + " " + e.Examiner?.LastName ?? "",
            SupervisorFullName = e.Supervisor?.FirstName + " " + e.Supervisor?.LastName ?? "",
        };
    }
}
