using FluentAssertions;
using FrontendAccountCreation.Core.Utilities;

namespace FrontendAccountCreation.Core.UnitTests.Utilities;

[TestClass]
public class AllowListTests
{
    [TestMethod]
    public void Create_WithItems_ShouldCreateAllowListWithThoseItems()
    {
        // Arrange
        var item1 = "apple";
        var item2 = "banana";

        // Act
        var allowList = AllowList<string>.Create(item1, item2);

        // Assert
        allowList.Should().NotBeNull();
        allowList.IsAllowed(item1).Should().BeTrue();
        allowList.IsAllowed(item2).Should().BeTrue();
    }

    [TestMethod]
    public void Create_WithNoItems_ShouldCreateEmptyAllowList()
    {
        // Arrange & Act
        var allowList = AllowList<string>.Create();

        // Assert
        allowList.Should().NotBeNull();
        allowList.IsAllowed("anything").Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowed_WhenItemIsInList_ShouldReturnTrue_ForStringType()
    {
        // Arrange
        var allowedItem = "test@example.com";
        var allowList = AllowList<string>.Create("one@example.com", allowedItem, "two@example.com");

        // Act
        var result = allowList.IsAllowed(allowedItem);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowed_WhenItemIsNotInList_ShouldReturnFalse_ForStringType()
    {
        // Arrange
        var notAllowedItem = "other@example.com";
        var allowList = AllowList<string>.Create("one@example.com", "two@example.com");

        // Act
        var result = allowList.IsAllowed(notAllowedItem);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowed_WhenItemIsInList_ShouldReturnTrue_ForIntType()
    {
        // Arrange
        var allowedItem = 42;
        var allowList = AllowList<int>.Create(10, allowedItem, 30);

        // Act
        var result = allowList.IsAllowed(allowedItem);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowed_WhenItemIsNotInList_ShouldReturnFalse_ForIntType()
    {
        // Arrange
        var notAllowedItem = 99;
        var allowList = AllowList<int>.Create(10, 20, 30);

        // Act
        var result = allowList.IsAllowed(notAllowedItem);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowed_WithNullItem_WhenNullIsNotInList_ShouldReturnFalse_ForReferenceType()
    {
        // Arrange
        var allowList = AllowList<string?>.Create("item1", "item2"); // string? to explicitly allow null in the list type

        // Act
        var result = allowList.IsAllowed(null);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowed_WithNullItem_WhenNullIsInList_ShouldReturnTrue_ForReferenceType()
    {
        // Arrange
        var allowList = AllowList<string?>.Create("item1", null, "item2"); // string? to explicitly allow null in the list type

        // Act
        var result = allowList.IsAllowed(null);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Create_WithDuplicateItems_ShouldStoreUniqueItemsAndBeAllowed()
    {
        // Arrange
        var item1 = "duplicate";
        var item2 = "unique";

        // Act
        var allowList = AllowList<string>.Create(item1, item2, item1, item1);

        // Assert
        allowList.IsAllowed(item1).Should().BeTrue();
        allowList.IsAllowed(item2).Should().BeTrue();
        // No direct way to check the count of _allowList with current public API,
        // but ImmutableHashSet handles duplicates correctly internally.
        // The primary concern is that IsAllowed works as expected.
    }

    [TestMethod]
    public void IsAllowed_CaseSensitivity_ForStrings()
    {
        // Arrange
        var item = "TestCase";
        var allowList = AllowList<string>.Create(item);

        // Act
        var isAllowedSameCase = allowList.IsAllowed("TestCase");
        var isAllowedDifferentCase = allowList.IsAllowed("testcase");

        // Assert
        isAllowedSameCase.Should().BeTrue();
        // ImmutableHashSet<string> is case-sensitive by default.
        isAllowedDifferentCase.Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowed_WithEmptyString_WhenEmptyStringIsInList_ShouldReturnTrue()
    {
        // Arrange
        var allowList = AllowList<string>.Create("", "item1");

        // Act
        var result = allowList.IsAllowed("");

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowed_WithEmptyString_WhenEmptyStringIsNotInList_ShouldReturnFalse()
    {
        // Arrange
        var allowList = AllowList<string>.Create("item1", "item2");

        // Act
        var result = allowList.IsAllowed("");

        // Assert
        result.Should().BeFalse();
    }

    private class CustomItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is CustomItem other)
            {
                return Id == other.Id && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }

    [TestMethod]
    public void IsAllowed_WithCustomObjectType_WhenItemIsInList_ShouldReturnTrue()
    {
        // Arrange
        var item1 = new CustomItem { Id = 1, Name = "One" };
        var item2 = new CustomItem { Id = 2, Name = "Two" }; // This is the one we'll check
        var item3 = new CustomItem { Id = 3, Name = "Three" };
        var allowList = AllowList<CustomItem>.Create(item1, item2, item3);

        // Act
        var result = allowList.IsAllowed(new CustomItem { Id = 2, Name = "Two" }); // New instance but equal

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowed_WithCustomObjectType_WhenItemIsNotInList_ShouldReturnFalse()
    {
        // Arrange
        var item1 = new CustomItem { Id = 1, Name = "One" };
        var item3 = new CustomItem { Id = 3, Name = "Three" };
        var allowList = AllowList<CustomItem>.Create(item1, item3);

        var notAllowedItem = new CustomItem { Id = 2, Name = "Two" };

        // Act
        var result = allowList.IsAllowed(notAllowedItem);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowed_WithCustomObjectType_WhenDifferentInstanceButNotInList_ShouldReturnFalse()
    {
        // Arrange
        var item1 = new CustomItem { Id = 1, Name = "One" };
        var allowList = AllowList<CustomItem>.Create(item1);

        var notAllowedItem = new CustomItem { Id = 1, Name = "DifferentName" }; // Different property

        // Act
        var result = allowList.IsAllowed(notAllowedItem);

        // Assert
        result.Should().BeFalse();
    }
}