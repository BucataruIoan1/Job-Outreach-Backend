using JobOutreach.Application.Common;
using JobOutreach.Application.Interfaces;
using JobOutreach.Domain.Entities;

namespace JobOutreach.Application.Services;

public class EmailSentService(IEmailSentRepository emailSentRepository) : IEmailSentService
{
    private readonly IEmailSentRepository _emailSentRepository = emailSentRepository;

    public async Task<IEnumerable<EmailSent>> GetAllEmailSentAsync()
        => await _emailSentRepository.GetAllAsync();

    public async Task<PagedResult<EmailSent>> GetEmailSentPagedAsync(int pageNumber, int pageSize)
        => await _emailSentRepository.GetPagedAsync(pageNumber, pageSize);

    public async Task DeleteEmailSentAsync(int id)
        => await _emailSentRepository.DeleteAsync(id);
}
