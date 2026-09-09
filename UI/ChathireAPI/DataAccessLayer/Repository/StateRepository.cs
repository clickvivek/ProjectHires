using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface IStateRepository : IRepository<State, int>
    {
        Task<List<State>> GetState(string? State);
    }
    public class StateRepository : BaseRepository<State, int>, IStateRepository
    {
        public StateRepository(EFContexts context) : base(context) { }

        public async Task<List<State>> GetState(string? State)
        {
            if (State == null)
            {
                return await _context.States.Include(s=>s.CountryCodeNavigation).ToListAsync();
            }
            else
                return await _context.States.Include(s => s.CountryCodeNavigation).Where(s => (s.StateCode != null && s.StateCode.Contains(State))
                || s.StateName != null && s.StateName.Contains(State)).ToListAsync();
        }

    }
    
}
