using Domain.Entity;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Persistance;
namespace Tests.Persistance
{
    public class ExamRepositoryTests : IClassFixture<SqliteDbFixture>
    {
        private readonly AppDbContext _db;
        public ExamRepositoryTests(SqliteDbFixture fx)
        {
            _db = fx.Db;
        }    
        [Fact]
        public async Task Update_Grade_WrongRange_Throws()
        {
            await SqliteDbFixture.ClearAsync(_db);

            Student student = new Student { FirstName = "A", LastName = "B", JMBG = "0101990123456", DateOfBirth = new DateOnly(1990, 1, 1), IndexNumber = "2020/1" };
            Teacher teacher = new Teacher { FirstName = "T", LastName = "E", JMBG = "0202990123456", DateOfBirth = new DateOnly(1980, 2, 2), Title = Enums.FullProfessor };
            Subject subject = new Subject { Name = "Math", ECTS = 6 };
            _db.AddRange(student, teacher, subject);
            await _db.SaveChangesAsync();

            var exam = new Exam
            {
                StudentID = student.ID,
                SubjectID = subject.ID,
                ExaminerID = teacher.ID,
                SupervisorID = null,
                Grade = 7,
                Date = new DateOnly(2025, 10, 3),
                Note = null
            };
            _db.Exams.Add(exam);
            Func<Task> insertAct = () => _db.SaveChangesAsync();
            await insertAct.Should().NotThrowAsync("FK are valid");

            exam.Grade = 4;
            await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
        }
    }
}
