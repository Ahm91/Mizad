namespace Mizad.Domain.Common;

/// <summary>
/// Thrown when a domain invariant is violated in a way the caller should have prevented
/// (a programming or data-integrity error). Expected business failures use <see cref="Result"/>.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException()
        : this(Error.Unexpected("Domain.Unexpected", "حدث خطأ غير متوقع في منطق البرنامج."))
    {
    }

    public DomainException(string message)
        : this(Error.Unexpected("Domain.Invariant", message))
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        Error = Error.Unexpected("Domain.Invariant", message);
    }

    public DomainException(Error error)
        : base((error ?? throw new ArgumentNullException(nameof(error))).Message)
    {
        Error = error;
    }

    public Error Error { get; }
}
