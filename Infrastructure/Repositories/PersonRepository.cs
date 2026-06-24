using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entity;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        public PersonRepository(AppDbContext db) : base(db) { }

        public Task<bool> ExistsByJmbgAsync(string jmbg, CancellationToken ct = default)
         =>  Set.IgnoreQueryFilters().AnyAsync(e=>e.JMBG== jmbg,ct);
    }
}
