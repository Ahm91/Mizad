using System.Runtime.CompilerServices;

namespace Mizad.Domain.Common;

/// <summary>Invariant checks that throw <see cref="DomainException"/>; for programming errors, not user input.</summary>
public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"'{name}' must not be empty.");
        }

        return value;
    }

    public static decimal AgainstNegative(decimal value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < 0m)
        {
            throw new DomainException($"'{name}' must not be negative (was {value}).");
        }

        return value;
    }

    public static decimal AgainstNonPositive(decimal value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value <= 0m)
        {
            throw new DomainException($"'{name}' must be greater than zero (was {value}).");
        }

        return value;
    }

    public static T AgainstNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        where T : class
    {
        return value ?? throw new DomainException($"'{name}' must not be null.");
    }
}
