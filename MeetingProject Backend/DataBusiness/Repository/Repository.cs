using DataBusiness.Interface;
using Entities.IModels;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DataBusiness.Repository
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly ProjectConnect _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(ProjectConnect dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _dbContext.SaveChanges();
        }
        
    }
}
