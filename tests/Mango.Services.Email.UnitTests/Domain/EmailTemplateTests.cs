using FluentAssertions;
using Mango.Services.Email.Domain.Entities;

namespace Mango.Services.Email.UnitTests.Domain;

/// <summary>
/// Unit tests for EmailTemplate entity validation and rendering.
/// </summary>
public class EmailTemplateTests
{
    private EmailTemplate CreateValidTemplate()
    {
        return new EmailTemplate
        {
            Id = 1,
            Name = "OrderConfirmation",
            Subject = "Order Confirmation - Order #{OrderId}",
            Body = "<p>Hello {CustomerName},</p><p>Your order #{OrderId} has been confirmed. Total: ${Total}</p>",
            Variables = "OrderId,CustomerName,Total",
            IsActive = true,
            Description = "Email confirmation for new orders"
        };
    }

    [Fact]
    public void IsValid_WithValidTemplate_ShouldReturnTrue()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act
        var result = template.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingName_ShouldReturnFalse()
    {
        // Arrange
        var template = CreateValidTemplate();
        template.Name = string.Empty;

        // Act
        var result = template.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingSubject_ShouldReturnFalse()
    {
        // Arrange
        var template = CreateValidTemplate();
        template.Subject = string.Empty;

        // Act
        var result = template.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingBody_ShouldReturnFalse()
    {
        // Arrange
        var template = CreateValidTemplate();
        template.Body = string.Empty;

        // Act
        var result = template.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ExtractVariables_ShouldFindAllPlaceholders()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act
        var variables = template.ExtractVariables();

        // Assert
        variables.Should().Contain(new[] { "OrderId", "CustomerName", "Total" });
        variables.Should().HaveCount(3);
    }

    [Fact]
    public void ExtractVariables_WithNoPlaceholders_ShouldReturnEmpty()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Name = "Simple",
            Subject = "Hello",
            Body = "This is plain text with no variables"
        };

        // Act
        var variables = template.ExtractVariables();

        // Assert
        variables.Should().BeEmpty();
    }

    [Fact]
    public void ExtractVariables_ShouldNotIncludeDuplicates()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Name = "Template",
            Subject = "Hello {Name}",
            Body = "Dear {Name}, your order {OrderId} has arrived. {Name}, thank you!"
        };

        // Act
        var variables = template.ExtractVariables();

        // Assert
        variables.Should().HaveCount(2);
        variables.Should().Contain(new[] { "Name", "OrderId" });
    }

    [Fact]
    public void RenderTemplate_ShouldReplaceAllVariables()
    {
        // Arrange
        var template = CreateValidTemplate();
        var values = new Dictionary<string, string>
        {
            { "OrderId", "12345" },
            { "CustomerName", "John Doe" },
            { "Total", "99.99" }
        };

        // Act
        var rendered = template.RenderTemplate(values);

        // Assert
        rendered.Should().Contain("#12345");
        rendered.Should().Contain("John Doe");
        rendered.Should().Contain("$99.99");
        rendered.Should().NotContain("{");
        rendered.Should().NotContain("}");
    }

    [Fact]
    public void RenderTemplate_WithMissingVariable_ShouldReplaceWithEmpty()
    {
        // Arrange
        var template = CreateValidTemplate();
        var values = new Dictionary<string, string>
        {
            { "OrderId", "12345" },
            { "CustomerName", "John Doe" }
            // Missing "Total"
        };

        // Act
        var rendered = template.RenderTemplate(values);

        // Assert
        rendered.Should().Contain("#12345");
        rendered.Should().Contain("John Doe");
        rendered.Should().Contain("${"); // {Total} becomes ${}
    }

    [Fact]
    public void RenderTemplate_WithEmptyDictionary_ShouldNotChange()
    {
        // Arrange
        var template = CreateValidTemplate();
        var values = new Dictionary<string, string>();
        var originalBody = template.Body;

        // Act
        var rendered = template.RenderTemplate(values);

        // Assert
        rendered.Should().Be(originalBody);
    }

    [Fact]
    public void Constructor_ShouldSetDefaults()
    {
        // Arrange & Act
        var template = new EmailTemplate
        {
            Name = "Test",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        // Assert
        template.IsActive.Should().BeTrue();
        template.IsDeleted.Should().BeFalse();
        template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Template_CanMarkAsInactive()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act
        template.IsActive = false;

        // Assert
        template.IsActive.Should().BeFalse();
    }
}
