using FluentAssertions;
using impojuego.Data.Entities;

namespace ImpoJuego.Tests;

public class UserTests
{
    [Fact]
    public void User_DefaultValues_ShouldBeCorrect()
    {
        var user = new User();

        user.Id.Should().Be(0);
        user.Email.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
        user.Role.Should().Be("User");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.Categories.Should().NotBeNull();
        user.Categories.Should().BeEmpty();
    }

    [Fact]
    public void User_SetProperties_ShouldWork()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "hashed",
            Role = "Admin"
        };

        user.Id.Should().Be(1);
        user.Email.Should().Be("test@test.com");
        user.PasswordHash.Should().Be("hashed");
        user.Role.Should().Be("Admin");
    }

    [Fact]
    public void User_Categories_CanBeModified()
    {
        var user = new User();
        var category = new Category { Name = "Test" };

        user.Categories.Add(category);

        user.Categories.Should().HaveCount(1);
    }
}

public class CategoryTests
{
    [Fact]
    public void Category_DefaultValues_ShouldBeCorrect()
    {
        var category = new Category();

        category.Id.Should().Be(0);
        category.Name.Should().BeEmpty();
        category.IsSystem.Should().BeFalse();
        category.IsActive.Should().BeTrue();
        category.OwnerId.Should().BeNull();
        category.Owner.Should().BeNull();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        category.Words.Should().NotBeNull();
        category.Words.Should().BeEmpty();
    }

    [Fact]
    public void Category_SetProperties_ShouldWork()
    {
        var owner = new User { Id = 1 };
        var category = new Category
        {
            Id = 1,
            Name = "Test Category",
            IsSystem = true,
            IsActive = false,
            OwnerId = 1,
            Owner = owner
        };

        category.Id.Should().Be(1);
        category.Name.Should().Be("Test Category");
        category.IsSystem.Should().BeTrue();
        category.IsActive.Should().BeFalse();
        category.OwnerId.Should().Be(1);
        category.Owner.Should().Be(owner);
    }

    [Fact]
    public void Category_Words_CanBeModified()
    {
        var category = new Category();
        var word = new Word { Text = "TestWord" };

        category.Words.Add(word);

        category.Words.Should().HaveCount(1);
    }
}

public class WordTests
{
    [Fact]
    public void Word_DefaultValues_ShouldBeCorrect()
    {
        var word = new Word();

        word.Id.Should().Be(0);
        word.Text.Should().BeEmpty();
        word.CategoryId.Should().Be(0);
    }

    [Fact]
    public void Word_SetProperties_ShouldWork()
    {
        var category = new Category { Id = 1, Name = "Test" };
        var word = new Word
        {
            Id = 1,
            Text = "TestWord",
            CategoryId = 1,
            Category = category
        };

        word.Id.Should().Be(1);
        word.Text.Should().Be("TestWord");
        word.CategoryId.Should().Be(1);
        word.Category.Should().Be(category);
    }
}
