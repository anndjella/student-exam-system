using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Tests.Persistance
{
    public sealed class SqliteDbFixture : IDisposable
    {
        public SqliteConnection Connection { get; }
        public AppDbContext Db { get; }

        public SqliteDbFixture()
        {
            Connection = new SqliteConnection("Filename=:memory:");
            Connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(Connection)
                .Options;

            Db = new AppDbContext(options);

            Db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Db.Dispose();
            Connection.Close();
            Connection.Dispose();
        }
    }

}