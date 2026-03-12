using ClubMonitor.Domain.Members;
using FluentAssertions;
using NUnit.Framework;

namespace Domain.UnitTests.Members;

[TestFixture]
public class EmailTests
{
    [Test]
    public void Create_Valid_NormalizesAndReturnsValue()
    {
        var input = " A.LICE@Example.Com ";
        var email = Email.Create(input);
        email.Value.Should().Be("a.lice@example.com");
    }

    [Test]
    public void Create_Invalid_ThrowsArgumentException()
    {
        var invalids = new[] { (string?)null, "", "no-at", "trailing@", new string('a', 300) };
        foreach (var s in invalids)
        {
            Assert.Throws<ArgumentException>(() => Email.Create(s!));
        }
    }

    [Test]
    public void EqualsAndHashCode_AreCaseSensitiveBehaviour()
    {
        var a = Email.Create("A@X.COM");
        var b = Email.Create("a@x.com");
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}

