using Api.Application.Abstractions;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {

        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);
        public Task UpdateAsync(T entity) { _dbSet.Update(entity); return Task.CompletedTask; }
        public Task UpdateRangeAsync(IEnumerable<T> entities) { _dbSet.UpdateRange(entities); return Task.CompletedTask; }
        public Task DeleteAsync(T entity) { _dbSet.Remove(entity); return Task.CompletedTask; }
        public Task DeleteRangeAsync(IEnumerable<T> entities) { _dbSet.RemoveRange(entities); return Task.CompletedTask; }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        public IQueryable<T> Query() => _dbSet.AsQueryable();

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
