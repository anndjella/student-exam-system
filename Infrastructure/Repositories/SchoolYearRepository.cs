using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class SchoolYearRepository : ISchoolYearRepository
    {
        private readonly AppDbContext _db;
        public SchoolYearRepository(AppDbContext db) { _db = db; }
        public Task<bool> ExistsByIdAsync(int id, CancellationToken ct)
        =>_db.SchoolYears.AsNoTracking().AnyAsync(e=>e.ID== id, ct); 
            
    }
}
