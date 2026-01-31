using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entity;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class PersonRepository : IPersonRepository
    {
        private readonly AppDbContext _db;
        public PersonRepository(AppDbContext db) => _db = db;
        public Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default)
         =>  _db.People.AnyAsync(e=>e.JMBG== jmbg,ct);

        public Task<Person?> GetByIdAsync(int personId, CancellationToken ct = default)
        => _db.People.FirstOrDefaultAsync(e => e.ID == personId, ct);
    }
}
