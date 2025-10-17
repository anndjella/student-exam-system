using Domain.Entity;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Persistance
{
    public class StudentRepositoryTests : IClassFixture<SqliteDbFixture>
    {
        private readonly AppDbContext _db;

        public StudentRepositoryTests(SqliteDbFixture fx)
        {
            _db = fx.Db;
        }

        [Fact]
        public async Task Insert_Then_GetById_ReturnsSameData()
        {
            await SqliteDbFixture.ClearAsync(_db);

            Student s = new Student
            {
                JMBG = "0101990123456",
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/15"
            };

            _db.Students.Add(s);
            await _db.SaveChangesAsync();

            var fromDb = await _db.Students.AsNoTracking().SingleAsync(x => x.ID == s.ID);

            Assert.Equal(s.FirstName, fromDb.FirstName);
            Assert.Equal(s.LastName, fromDb.LastName);
            Assert.Equal(s.IndexNumber, fromDb.IndexNumber);

        }
        [Fact]
        public async Task Insert_Duplicate_IndexNumber_Throws_DbUpdateException()
        {
            await SqliteDbFixture.ClearAsync(_db);

            _db.Students.Add(new Student
            {
                JMBG = "1111990123456",
                FirstName = "Pera",
                LastName = "Perić",
                DateOfBirth = new DateOnly(1990, 11, 11),
                IndexNumber = "2024/5"
            });
            await _db.SaveChangesAsync();

            _db.Students.Add(new Student
            {
                JMBG = "2202992123456",
                FirstName = "Mika",
                LastName = "Mikić",
                DateOfBirth = new DateOnly(1992, 2, 22),
                IndexNumber = "2024/5"
            });

            await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
        }
        [Fact]
        public async Task Deleting_Student_cascades_to_Exams()
        {
            await SqliteDbFixture.ClearAsync(_db);

            Student student = new Student
            {
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                JMBG = "0101990123456",
                IndexNumber = "2024/57"
            };
            _db.Students.Add(student);

            Teacher examiner = new Teacher
            {
                FirstName = "A",
                LastName = "B",
                DateOfBirth = new DateOnly(1970, 1, 1),
                JMBG = "0101970123456",
                Title = Title.AssistantProfessor
            };
            Teacher supervisor = new Teacher
            {
                FirstName = "X",
                LastName = "Y",
                DateOfBirth = new DateOnly(1975, 1, 1),
                JMBG = "0101975123456",
                Title = Title.FullProfessor
            };
            _db.Teachers.AddRange(examiner, supervisor);

            var subject = new Subject
            {
                Name = "Math",
                ESPB = 8
            };
            _db.Subjects.Add(subject);

            await _db.SaveChangesAsync();

            Exam exam = new Exam
            {
                StudentID = student.ID,
                SubjectID = subject.ID,
                ExaminerID = examiner.ID,
                SupervisorID = supervisor.ID,
                Grade = 8,
                Date = new DateOnly(2024, 1, 10),
                Note = "ok"
            };
            _db.Exams.Add(exam);
            await _db.SaveChangesAsync();

            Assert.Equal(1, await _db.Exams.CountAsync(e => e.StudentID == student.ID));

            _db.Students.Remove(student);
            await _db.SaveChangesAsync();

            Assert.Null(await _db.Students.FindAsync(student.ID));
            Assert.Equal(0, await _db.Exams.CountAsync(e => e.StudentID == student.ID));

            Assert.NotNull(await _db.Subjects.FindAsync(subject.ID));
            Assert.NotNull(await _db.Teachers.FindAsync(examiner.ID));
            Assert.NotNull(await _db.Teachers.FindAsync(supervisor.ID));

        }
    }
}
