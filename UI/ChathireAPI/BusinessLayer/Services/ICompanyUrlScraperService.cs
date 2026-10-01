using BusinessEntityAndDTO.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface ICompanyUrlScraperService
    {
        Task<CompanyUrlScrapedDto> ScrapeCompanyUrlAsync(string rawUrl);
        Task<List<CompanyUrlScrapedDto>> ScrapeAndSaveCompanyUrlsAsync(List<string> urls, bool autoSaveToDb, long? adminUserId);
    }
}
