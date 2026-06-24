using Domain.Entity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public abstract class BaseRepository<TEntity>
        where TEntity : class, IEntity
    {
        protected readonly AppDbContext Db;
        protected readonly DbSet<TEntity> Set;

        protected BaseRepository(AppDbContext db)
        {
            Db = db;
            Set = db.Set<TEntity>();
        }

        public void Add(TEntity entity) => Set.Add(entity);

        public void AddRange(IEnumerable<TEntity> entities) => Set.AddRange(entities);

        public Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => Set.FirstOrDefaultAsync(entity => entity.ID == id, ct);

        public Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default)
            => Set.AsNoTracking().AnyAsync(entity => entity.ID == id, ct);

        public void Update(TEntity entity) => Set.Update(entity);

        public void Remove(TEntity entity) => Set.Remove(entity);
    }
}
