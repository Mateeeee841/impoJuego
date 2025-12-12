using FluentAssertions;
using ImpoJuego.Data;

namespace ImpoJuego.Tests;

public class WordCategoriesTests
{
    [Fact]
    public void GetCategoryNames_ShouldReturnAllCategories()
    {
        var categories = WordCategories.GetCategoryNames();

        categories.Should().NotBeEmpty();
        categories.Should().Contain("Países");
        categories.Should().Contain("Famosos");
    }

    [Fact]
    public void GetWords_WithValidCategory_ShouldReturnWords()
    {
        var words = WordCategories.GetWords("Países");

        words.Should().NotBeEmpty();
        words.Should().Contain("Argentina");
    }

    [Fact]
    public void GetWords_WithInvalidCategory_ShouldReturnEmptyList()
    {
        var words = WordCategories.GetWords("CategoriaInexistente");

        words.Should().BeEmpty();
    }

    [Fact]
    public void GetRandomCategory_ShouldReturnValidCategory()
    {
        var random = new Random(42);
        var categories = WordCategories.GetCategoryNames();

        var randomCategory = WordCategories.GetRandomCategory(random);

        categories.Should().Contain(randomCategory);
    }

    [Fact]
    public void GetRandomWord_WithValidCategory_ShouldReturnWord()
    {
        var random = new Random(42);

        var word = WordCategories.GetRandomWord("Países", random);

        word.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetRandomWord_WithInvalidCategory_ShouldReturnEmptyString()
    {
        var random = new Random(42);

        var word = WordCategories.GetRandomWord("CategoriaInexistente", random);

        word.Should().BeEmpty();
    }

    [Fact]
    public void AddCategory_ShouldAddNewCategory()
    {
        var newWords = new List<string> { "Test1", "Test2", "Test3" };

        WordCategories.AddCategory("TestCategory", newWords);

        var words = WordCategories.GetWords("TestCategory");
        words.Should().BeEquivalentTo(newWords);

        // Limpiar
        WordCategories.AddCategory("TestCategory", new List<string>());
    }

    [Fact]
    public void AddWordsToCategory_WithExistingCategory_ShouldAddWords()
    {
        // Primero crear una categoría de prueba
        WordCategories.AddCategory("TestCategory2", new List<string> { "Existing" });

        WordCategories.AddWordsToCategory("TestCategory2", "NewWord1", "NewWord2");

        var words = WordCategories.GetWords("TestCategory2");
        words.Should().Contain("Existing");
        words.Should().Contain("NewWord1");
        words.Should().Contain("NewWord2");

        // Limpiar
        WordCategories.AddCategory("TestCategory2", new List<string>());
    }

    [Fact]
    public void AddWordsToCategory_WithNonExistingCategory_ShouldNotThrow()
    {
        // No debería lanzar excepción
        var action = () => WordCategories.AddWordsToCategory("NonExistentCategory", "Word1");

        action.Should().NotThrow();
    }
}
