namespace Mango.Services.Email.Domain.Entities;

/// <summary>
/// Email template entity for managing reusable email templates.
/// Supports dynamic variable substitution using placeholder patterns like {VariableName}.
/// </summary>
public class EmailTemplate : AuditableEntity
{
    /// <summary>
    /// Template name (e.g., "OrderConfirmation", "PasswordReset").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line with optional variable placeholders.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body HTML content with optional variable placeholders.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of variable names used in this template (e.g., "UserName,OrderId,Total").
    /// </summary>
    public string? Variables { get; set; }

    /// <summary>
    /// Whether this template is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional description of the template's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Validate that the template has required fields.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Subject) &&
               !string.IsNullOrWhiteSpace(Body);
    }

    /// <summary>
    /// Extract variable names from the template content.
    /// Looks for patterns like {VariableName}.
    /// </summary>
    public List<string> ExtractVariables()
    {
        var variables = new List<string>();
        var pattern = @"\{([a-zA-Z_][a-zA-Z0-9_]*)\}";
        var regex = new System.Text.RegularExpressions.Regex(pattern);

        var matches = regex.Matches(Body);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var varName = match.Groups[1].Value;
            if (!variables.Contains(varName))
            {
                variables.Add(varName);
            }
        }

        return variables;
    }

    /// <summary>
    /// Replace template variables with actual values.
    /// </summary>
    public string RenderTemplate(Dictionary<string, string> variables)
    {
        var result = Body;

        foreach (var kvp in variables)
        {
            var placeholder = $"{{{kvp.Key}}}";
            result = result.Replace(placeholder, kvp.Value ?? string.Empty);
        }

        return result;
    }
}
