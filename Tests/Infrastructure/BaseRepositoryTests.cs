using Domain.Entity;
using FluentAssertions;
using Infrastructure.Repositories;
using Tests.TestDoubles;

namespace Tests.Infrastructure;

public sealed class BaseRepositoryTests
{
    [Fact]
    public async Task BaseRepositoryMethods_WorkThroughConcreteRepository()
    {
        var (connection, db) = await SqliteAppDbContextFactory.CreateOpenDbAsync();
        await using var _ = connection;
        await using var __ = db;
        var repository = new TermRepository(db);
        var term = new Term
        {
            Name = "January",
            RegistrationStartDate = new DateOnly(2026, 1, 1),
            RegistrationEndDate = new DateOnly(2026, 1, 5),
            StartDate = new DateOnly(2026, 1, 10),
            EndDate = new DateOnly(2026, 1, 20)
        };

        repository.Add(term);
        await db.SaveChangesAsync();

        term.ID.Should().BeGreaterThan(0);
        var existsAfterAdd = await repository.ExistsById(term.ID, CancellationToken.None);
        existsAfterAdd.Should().BeTrue();

        var loaded = await repository.GetByIdAsync(term.ID, CancellationToken.None);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("January");

        repository.Remove(loaded);
        await db.SaveChangesAsync();

        var existsAfterRemove = await repository.ExistsById(term.ID, CancellationToken.None);
        existsAfterRemove.Should().BeFalse();
    }
}
