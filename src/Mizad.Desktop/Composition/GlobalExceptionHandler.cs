using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace Mizad.Desktop.Composition;

/// <summary>
/// Last-chance error handling (ARCHITECTURE §4.7): every unhandled exception is logged with a short reference
/// number, and the user sees a friendly Arabic message without technical or sensitive details.
/// </summary>
internal static class GlobalExceptionHandler
{
    public static void Register(System.Windows.Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        application.DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
    }

    /// <summary>Logs the exception and returns the reference shown to the user.</summary>
    public static string Report(Exception exception, string source)
    {
        var reference = CreateReference();
        Log.Error(exception, "Unhandled exception from {Source}. Reference {ErrorReference}", source, reference);
        return reference;
    }

    public static string CreateReference() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    public static string BuildUserMessage(string reference) =>
        "حدث خطأ غير متوقع أثناء تنفيذ العملية." + Environment.NewLine +
        "إذا تكرر الخطأ، يرجى التواصل مع الدعم وذكر الرقم المرجعي: " + reference;

    public static void ShowFriendlyError(string reference) =>
        MessageBox.Show(
            BuildUserMessage(reference),
            "مِيزاد",
            MessageBoxButton.OK,
            MessageBoxImage.Error,
            MessageBoxResult.OK,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var reference = Report(e.Exception, "UI thread");
        ShowFriendlyError(reference);
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Report(e.Exception, "Unobserved task");
        e.SetObserved();
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Report(exception, e.IsTerminating ? "AppDomain (terminating)" : "AppDomain");
        }

        if (e.IsTerminating)
        {
            Log.CloseAndFlush();
        }
    }
}
