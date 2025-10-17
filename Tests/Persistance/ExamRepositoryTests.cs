using Domain.Entity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Persistance
{
    public class ExamRepositoryTests : IClassFixture<SqliteDbFixture>
    {
        private readonly AppDbContext _db;
        private readonly Student _student;
        private readonly Teacher _teacher;
        private readonly Subject _subject;
        public ExamRepositoryTests(SqliteDbFixture fx)
        {
            _db = fx.Db;
            _student = new Student { FirstName = "A", LastName = "B", JMBG = "0101990123456", DateOfBirth = new DateOnly(1990, 1, 1), IndexNumber = "2020/1" };
            _teacher = new Teacher { FirstName = "T", LastName = "E", JMBG = "0202990123456", DateOfBirth = new DateOnly(1980, 2, 2), Title = Title.FullProfessor };
            _subject = new Subject { Name = "Math", ESPB = 6 };

            _db.Students.Add(_student);
            _db.Teachers.Add(_teacher);
            _db.Subjects.Add(_subject);
            _db.SaveChanges();
        }
        [Fact]
        public async Task Insert_OK()
        {
            Exam exam = new Exam
            {
                StudentID = _student.ID,
                SubjectID = _subject.ID,
                ExaminerID = _teacher.ID,
                SupervisorID = null,
                Grade = 7,
                Date = new DateOnly(2025, 10, 3),
                Note = null
            };
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync();
            var fromDb = await _db.Exams.AsNoTracking().SingleAsync(x => x.ID == exam.ID);

            Assert.Equal(fromDb.Grade, exam.Grade);
            Assert.Equal(fromDb.StudentID, exam.StudentID);
            Assert.Equal(fromDb.SupervisorID, exam.SupervisorID);
        }
        [Fact]
        public async Task Insert_AlreadyPassed_Throws()
        {
            Exam exam = new Exam
            {
                StudentID = _student.ID,
                SubjectID = _subject.ID,               
                Grade = 7,
            };
            _db.Exams.Add(exam);
            await Assert.ThrowsAsync<DbUpdateException>(() =>  _db.SaveChangesAsync());
        }
        [Fact]
        public async Task Update_Grade_WrongRange_Throws()
        {
            Exam exam = new Exam
            {
                StudentID = _student.ID,
                SubjectID = _subject.ID,
                ExaminerID = _teacher.ID,
                SupervisorID = null,
                Grade = 7,
                Date = new DateOnly(2025, 10, 3),
                Note = null
            };
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync();
            exam.Grade = 4;
            await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
        }
    }
}
