using Domain.Entity;
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
    public class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _db;
        public SubjectRepository(AppDbContext db)
        {
            _db = db;
        }
        public void Add(Subject subject)
        =>_db.Subjects.Add(subject);

        public Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default)
        =>_db.Subjects.AsNoTracking().FirstOrDefaultAsync(x=>x.ID == id, ct);   

        public Task<Subject?> GetByNameAsync(string name, CancellationToken ct = default)
        => _db.Subjects.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), ct);
        public Task<List<int>> GetExistingIdsAsync(List<int> ids, CancellationToken ct)
        => _db.Subjects.Where(s => ids.Contains(s.ID)).Select(s => s.ID).ToListAsync(ct);


        public void Update(Subject subject)
        =>_db.Subjects.Update(subject);
    }
}
