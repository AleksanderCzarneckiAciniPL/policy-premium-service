using System.ComponentModel.DataAnnotations;

namespace PolicyPremium.Api.Validation;

/// <summary>
/// Validates that a string equals one of the names of <see cref="EnumType"/>, ignoring case.
/// Numeric values are not accepted — only the named members. The permitted names are exposed via
/// <see cref="Names"/> so they can also be projected onto the OpenAPI schema (see Program.cs).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class EnumNameAttribute : ValidationAttribute
{
    public EnumNameAttribute(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException($"{enumType} is not an enum.", nameof(enumType));
        }

        EnumType = enumType;
        Names = Enum.GetNames(enumType);
    }

    public Type EnumType { get; }

    public IReadOnlyList<string> Names { get; }

    public override bool IsValid(object? value) =>
        // Null is left to [Required]; any supplied value must match a member name, case-insensitively.
        value is string text && Names.Any(name => string.Equals(name, text, StringComparison.OrdinalIgnoreCase));
}
