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
    public interface ICityRepository : IRepository<City, int>
    {
        Task<List<City>> GetLocation(string? city, int? state, bool IsState);
    }

    public class CityRepository : BaseRepository<City, int>, ICityRepository
    {
        public CityRepository(EFContexts context) : base(context) { }

        public async Task<List<City>> GetLocation(string? searchString, int? state, bool IsState)
        {
            var query = _context.Cities
                    .AsNoTracking()
                    .Include(c => c.IdStateNavigation)
                    .ThenInclude(c => c.CountryCodeNavigation)
                    .AsQueryable();

            if (state != null)
            {
                query = query.Where(s => s.IdState == state);
            }

            if (IsState)
            {
                query = query.Where(s => s.IsState == true);

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var term = searchString.Trim();
                    query = query.Where(s =>
                        (s.City1 != null && s.City1.StartsWith(term)) ||
                        (s.IdStateNavigation != null && (
                            (s.IdStateNavigation.StateName != null && s.IdStateNavigation.StateName.StartsWith(term)) ||
                            (s.IdStateNavigation.StateCode != null && s.IdStateNavigation.StateCode.StartsWith(term))
                        ))
                    );
                }

                return await query.OrderBy(s => s.City1).Take(20).ToListAsync();
            }
            else
            {
                query = query.Where(s => s.IsState == null || s.IsState == false);

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    var term = searchString.Trim();
                    bool isNumeric = term.Length > 0 && term.All(char.IsDigit);
                    if (isNumeric)
                    {
                        query = query.Where(s => s.Zip != null && s.Zip.StartsWith(term));
                    }
                    else
                    {
                        query = query.Where(s => s.City1 != null && s.City1.StartsWith(term));
                    }
                }

                return await query.OrderBy(s => s.City1).Take(25).ToListAsync();
            }
        }
    }
}
