using ClubMonitor.Application.Members;
using ClubMonitor.Domain.Members;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Application.UnitTests.Members;

[TestFixture]
public class CreateMemberHandlerTests
{
    [Test]
    public async Task HandleAsync_CreatesMember_WhenEmailNotExists()
    {
        var repo = new Mock<IMemberRepository>(MockBehavior.Strict);
        repo.Setup(r => r.ExistsWithEmailAsync(It.IsAny<Email>(), default)).ReturnsAsync(false);
        repo.Setup(r => r.AddAsync(It.IsAny<Member>(), default)).Returns(Task.CompletedTask).Verifiable();
        repo.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask).Verifiable();

        var handler = new CreateMemberHandler(repo.Object);

        var result = await handler.HandleAsync(new CreateMemberCommand("Alice", "alice@example.com"));

        result.Name.Should().Be("Alice");
        result.Email.Should().Be("alice@example.com");
        repo.Verify(r => r.AddAsync(It.IsAny<Member>(), default), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Test]
    public void HandleAsync_ThrowsDuplicateEmail_WhenExists()
    {
        var repo = new Mock<IMemberRepository>(MockBehavior.Strict);
        repo.Setup(r => r.ExistsWithEmailAsync(It.IsAny<Email>(), default)).ReturnsAsync(true);

        var handler = new CreateMemberHandler(repo.Object);

        Func<Task> act = async () => await handler.HandleAsync(new CreateMemberCommand("Bob", "bob@example.com"));

        act.Should().Throw<DuplicateEmailException>();
    }
}

