using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
//using DataAccessLayer.CustomModels;
//using DataAccessLayer.EFContext;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;


namespace DataAccessLayer.Repository
{
    public class BaseRepository
    {
        protected readonly EFContexts _context;
        public BaseRepository(EFContexts context)
        {
            _context = context;
        }
        public async Task<E> Get<E, P>(P id) where E : BaseModel<P> where P : struct
        {
            var user = await _context.FindAsync(typeof(E), id);

            if (user != null)
                return (E)user;
            return null;
        }
        public async Task<List<E>> GetAll<E>() where E : class
        {
            var values = await _context.Set<E>().ToListAsync();

            if (values != null)
                return values;
            return null;
        }

        //public async Task<List<String>> GetAllValue(string table , string colunm) 
        //{
        //    var values = await _context.Set(Type.GetType(table)).ToListAsync();

        //    if (values != null)
        //        return values;
        //    return null;
        //}
        public async Task Put<E, P>(P id, E user, bool saveChanges = true) where E : BaseModel<P> where P : struct
        {
            if (id.Equals(user.Id))
            {
                throw new ArgumentException("Invalid or Missing Id", "Id");
            }

            _context.Entry(user).State = EntityState.Modified;
            if (saveChanges)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await Exists<E, P>(id))
                    {
                        throw new KeyNotFoundException("Key Not Found : " + id);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        public async Task<E> Post<E, P>(E entity, bool saveChanges = true) where E : BaseModel<P> where P : struct
        {
            _context.Add(entity);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            return entity;
        }
        public async Task Delete<E, P>(P id, bool saveChanges = true) where E : BaseModel<P> where P : struct
        {
            var entity = await Get<E, P>(id);
            if (entity == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }

            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> Exists<E, P>(P id) where E : BaseModel<P> where P : struct
        {
            var user = await Get<E, P>(id);
            if (user == null)
                return false;
            else
                return true;
        }
    }

    public class BaseRepository<T, I> : BaseRepository where T : BaseModel<I> where I : struct
    {
        //protected readonly WheelDogLeagueContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(EFContexts context) : base(context)
        {
            //_context = context;
            _dbSet = _context.Set<T>();
        }

        public R GetRepository<R>() where R : IRepository, new()
        {
            return new R();
        }
        public async Task<T> Get(I id)
        {
            var user = await _dbSet.FindAsync(id);

            if (user != null)
                return (T)user;
            return null;
        }

        //public async Task<T> GetByProfileId(I id)
        //{
        //    var user = await _dbSet.Where(a => a.);

        //    if (user != null)
        //        return (T)user;
        //    return null;
        //}

        public async Task<List<T>> GetAll()
        {
            var values = await _dbSet.ToListAsync();

            if (values != null)
                return (List<T>)values;
            return null;
        }

        //public async Task<E> Get<E, P>(P id) where E : BaseModel<P> where P : struct
        //{
        //    var user = await _context.FindAsync(typeof(E), id);

        //    if (user != null)
        //        return (E)user;
        //    return null;
        //}

        public async Task Put(I id, T enity, bool saveChanges = true)
        {
            if (id.Equals(enity.Id))
            {
                throw new ArgumentException("Invalid or Missing Id", "Id");
            }

            _context.Entry(enity).State = EntityState.Modified;

            if (saveChanges)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await Exists(id))
                    {
                        throw new KeyNotFoundException("Key Not Found : " + id);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        //public async Task Put<E, P>(P id, E user, bool saveChanges = true)
        //    where E : BaseModel<P> where P : struct
        //{
        //    if (id.Equals(user.Id))
        //    {
        //        throw new ArgumentException("Invalid or Missing Id", "Id");
        //    }

        //    _context.Entry(user).State = EntityState.Modified;
        //    if (saveChanges)
        //    {
        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!await Exists<E, P>(id))
        //            {
        //                throw new KeyNotFoundException("Key Not Found : " + id);
        //            }
        //            else
        //            {
        //                throw;
             //        }
        //    }
        //}

        public async Task<T> Post(T entity, bool saveChanges = true)
        {
            _context.Add(entity);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }

            return entity;
        }

        //public async Task<E> Post<E, P>(E user, bool saveChanges = true)
        //    where E : BaseModel<P> where P : struct
        //{
        //    _context.Add(user);
        //    if (saveChanges)
        //    {
        //        await _context.SaveChangesAsync();
        //    }

        //    return user;
        //}

        public async Task Delete(I id, bool saveChanges = true)
        {
            var user = await Get(id);
            if (user == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }

            _context.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<T>> AddRange(List<T> lists, UserContext userContext)
        {
            if (lists == null)
            {
                throw new KeyNotFoundException("lists Not Found");
            }
            _context.AddRange(lists);
            await _context.SaveChangesAsync();
            return lists;
        }

        public async Task RemoveRange(List<T> lists, UserContext userContext)
        {
            //var lst;

            //if (lists.GetType().FullName.Contains(typeof(CandidateProfileEmploymentType).FullName))
            //{
            //    //System.Linq.Expressions.Expression<Func<CandidateProfileEmploymentType, bool>> predicate = a => a.CandidateProfileId.Equals(id);
            //    //lists = (List<T>)_context.CandidateProfileEmploymentTypes.Where(predicate);

            //    var lists1 = _context.CandidateProfileEmploymentTypes.Where(a => a.CandidateProfileId.Equals(id)).ToList();

            //}
            //else if (lists.GetType().FullName.Contains(typeof(CandidateProfileSkill).FullName))
            //{
            //    //System.Linq.Expressions.Expression<Func<CandidateProfileSkill, bool>> predicate = a => a.CandidateProfileid.Equals(id);
            //    //lists = (List<T>)_context.CandidateProfileSkills.Where(predicate);
            //    lists = (List<T>)_context.CandidateProfileSkills.Where(a => a.CandidateProfileid.Equals(id));
            //}

            if (lists == null)
            {
                throw new KeyNotFoundException("Lists Not Found ");
            }

            _context.RemoveRange(lists);
            await _context.SaveChangesAsync();
        }

        //public async Task DeleteByProfileId(I id, bool saveChanges = true)
        //{
        //    var user = _dbSet.Where(a=>a.)
        //    if (user == null)
        //    {
        //        throw new KeyNotFoundException("Key Not Found : " + id);
        //    }

        //    _context.Remove(user);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteUser<E, P>(P id, bool saveChanges = true)
        //    where E : BaseModel<P> where P : struct
        //{
        //    var user = await Get<E, P>(id);
        //    if (user == null)
        //    {
        //        throw new KeyNotFoundException("Key Not Found : " + id);
        //    }

        //    _context.Remove(user);
        //    await _context.SaveChangesAsync();
        //}

        public async Task<bool> Exists(I id)
        {
            var user = await Get(id);
            if (user == null)
                return false;
            else
                return true;
        }

        //private async Task<bool> Exists<E, P>(P id) where E : BaseModel<P> where P : struct
        //{
        //    var user = await Get<E, P>(id);
        //    if (user == null)
        //        return false;
        //    else
        //        return true;
        //}
    }
}
