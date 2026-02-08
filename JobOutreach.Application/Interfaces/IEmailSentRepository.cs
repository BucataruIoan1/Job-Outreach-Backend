using JobOutreach.Application.Common;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Interfaces;

public interface IEmailSentRepository
{
    Task AddAsync(EmailSent emailSent);
    Task DeleteAsync(int id);
    Task<IEnumerable<EmailSent>> GetAllAsync();
    Task<PagedResult<EmailSent>> GetPagedAsync(int pageNumber, int pageSize);
}
