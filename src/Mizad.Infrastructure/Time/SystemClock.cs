using Mizad.Application.Abstractions;

namespace Mizad.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.Now;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
