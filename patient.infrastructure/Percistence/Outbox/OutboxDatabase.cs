using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
using patient.infrastructure.Percistence.DomainModel;

namespace patient.infrastructure.Percistence.Outbox;

internal class OutboxDatabase : IOutboxDatabase<DomainEvent>
{
    private readonly DomainDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public OutboxDatabase(DomainDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public DbSet<OutboxMessage<DomainEvent>> GetOutboxMessages()
    {
        return _dbContext.OutboxMessages;
    }
}
