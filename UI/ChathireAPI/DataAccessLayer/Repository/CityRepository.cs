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
        Task<List<City>> GetLocation(string? city, int? state, bool IsState, string? country = null);
    }

    public class CityRepository : BaseRepository<City, int>, ICityRepository
    {
        public CityRepository(EFContexts context) : base(context) { }

        public async Task<List<City>> GetLocation(string? searchString, int? state, bool IsState, string? country = null)
        {
            var query = _context.Cities
                    .AsNoTracking()
                    .Include(c => c.IdStateNavigation)
                    .ThenInclude(c => c.CountryCodeNavigation)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(country))
            {
                var cNorm = country.Trim().ToUpper();
                if (cNorm == "USA" || cNorm == "US")
                {
                    query = query.Where(s => s.IdStateNavigation != null && (s.IdStateNavigation.CountryCode == "US" || s.IdStateNavigation.CountryCode == "USA"));
                }
                else if (cNorm == "CANADA" || cNorm == "CA")
                {
                    query = query.Where(s => s.IdStateNavigation != null && (s.IdStateNavigation.CountryCode == "CA" || s.IdStateNavigation.CountryCode == "CAN" || s.IdStateNavigation.CountryCode == "Canada"));
                }
                else
                {
                    query = query.Where(s => s.IdStateNavigation != null && s.IdStateNavigation.CountryCode == cNorm);
                }
            }

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
                    if (term.Contains(','))
                    {
                        var parts = term.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var cityPart = parts[0];
                        var statePart = parts.Length > 1 ? parts[1] : "";
                        query = query.Where(s =>
                            (s.City1 != null && s.City1.StartsWith(cityPart)) &&
                            (string.IsNullOrEmpty(statePart) || (s.IdStateNavigation != null && (
                                (s.IdStateNavigation.StateCode != null && s.IdStateNavigation.StateCode.StartsWith(statePart)) ||
                                (s.IdStateNavigation.StateName != null && s.IdStateNavigation.StateName.StartsWith(statePart))
                            )))
                        );
                    }
                    else
                    {
                        bool isNumeric = term.Length > 0 && term.All(char.IsDigit);
                        if (isNumeric)
                        {
                            query = query.Where(s => s.Zip != null && s.Zip.StartsWith(term));
                        }
                        else
                        {
                            var cityMatches = await query.Where(s => s.City1 != null && s.City1.StartsWith(term))
                                                         .OrderBy(s => s.City1)
                                                         .Take(25)
                                                         .ToListAsync();
                            if (cityMatches.Any())
                            {
                                return cityMatches;
                            }

                            if (term.Length == 2)
                            {
                                var stateMatches = await query.Where(s => s.IdStateNavigation != null && s.IdStateNavigation.StateCode != null && s.IdStateNavigation.StateCode.StartsWith(term))
                                                              .OrderBy(s => s.City1)
                                                              .Take(25)
                                                              .ToListAsync();
                                if (stateMatches.Any())
                                {
                                    return stateMatches;
                                }
                            }

                            return await query.Where(s => (s.Zip != null && s.Zip.StartsWith(term)) ||
                                                          (s.IdStateNavigation != null && s.IdStateNavigation.StateName != null && s.IdStateNavigation.StateName.StartsWith(term)))
                                              .OrderBy(s => s.City1)
                                              .Take(25)
                                              .ToListAsync();
                        }
                    }
                }

                return await query.OrderBy(s => s.City1).Take(25).ToListAsync();
            }
        }
    }
}
