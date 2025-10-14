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
    public class StudentRepositoryTests : IClassFixture<SqliteDbFixture>,IAsyncLifetime
    {
        private readonly AppDbContext _db;

        public StudentRepositoryTests(SqliteDbFixture fx)
        {
            _db = fx.Db;
        }
        public async Task InitializeAsync()
        {
            _db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
            await SqliteDbFixture.ClearAsync(_db);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task Insert_Then_GetById_ReturnsSameData()
        {
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

            Assert.Equal("Ana", fromDb.FirstName);
            Assert.Equal("Anić", fromDb.LastName);
            Assert.Equal("2024/15", fromDb.IndexNumber);
        }
        [Fact]
        public async Task Insert_Duplicate_IndexNumber_Throws_DbUpdateException()
        {
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
        public async Task Insert_Null_Jmbg_Throws_DbUpdateException()
        { 
            Student s = new Student
            {
                JMBG = null!,
                FirstName = "Ana",
                LastName = "Anić",
                DateOfBirth = new DateOnly(1990, 1, 1),
                IndexNumber = "2024/7"
            };

            _db.Students.Add(s);
            await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
        }

    }
}
