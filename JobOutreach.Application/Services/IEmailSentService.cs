using JobOutreach.Application.Common;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public interface IEmailSentService
{
    Task<IEnumerable<EmailSent>> GetAllEmailSentAsync();
    Task<PagedResult<EmailSent>> GetEmailSentPagedAsync(int pageNumber, int pageSize);
    Task DeleteEmailSentAsync(int id);
}
