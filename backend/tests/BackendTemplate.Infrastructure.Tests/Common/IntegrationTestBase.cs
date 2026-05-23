using BackendTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace BackendTemplate.Infrastructure.Tests.Common;

public abstract class IntegrationTestBase(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    protected AppDbContext DbContext { get; private set; } = null!;
    private IDbContextTransaction _transaction = null!;

    public async Task InitializeAsync()
    {
        DbContext = fixture.CreateContext();
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await DbContext.DisposeAsync();
    }
}
