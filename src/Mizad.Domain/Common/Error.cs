namespace Mizad.Domain.Common;

/// <summary>Category of an expected failure; lets the UI choose how to present it.</summary>
public enum ErrorKind
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    BusinessRule = 5,
    Unexpected = 6,
}

/// <summary>
/// An expected, user-presentable failure. <see cref="Code"/> is a stable machine identifier
/// (e.g. <c>Stock.Insufficient</c>); <see cref="Message"/> is the Arabic text shown to the user.
/// </summary>
public sealed record Error(string Code, string Message, ErrorKind Kind)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorKind.None);

    public static Error Validation(string code, string message) => new(code, message, ErrorKind.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorKind.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorKind.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorKind.Forbidden);

    public static Error BusinessRule(string code, string message) => new(code, message, ErrorKind.BusinessRule);

    public static Error Unexpected(string code, string message) => new(code, message, ErrorKind.Unexpected);
}
