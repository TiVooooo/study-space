﻿using Microsoft.EntityFrameworkCore;
using StudySpace.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Base
{
    public class GenericRepository<T> where T : class
    {
        protected EXE201_StudySpaceContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository()
        {
            _context ??= new EXE201_StudySpaceContext();

        }

        #region Separating asign entity and save operators

        public GenericRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public void PrepareCreate(T entity)
        {
            _context.Add(entity);
        }

        public void PrepareUpdate(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
        }

        public void PrepareRemove(T entity)
        {
            _context.Remove(entity);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion Separating asign entity and save operators

        public IEnumerable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Where(expression).AsNoTracking();
        }

        public async Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().Where(expression).AsNoTracking().ToListAsync();
        }
        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public List<T> GetAll(params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.ToList();
        }

        public async Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }
        public void Create(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }

        public async Task<int> CreateAsync(T entity)
        {
            _context.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;

            return await _context.SaveChangesAsync();
        }

        public bool Remove(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        
        public async Task<T> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public T GetById(string code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(string code)
        {
            return await _context.Set<T>().FindAsync(code);
        }
    }
}
