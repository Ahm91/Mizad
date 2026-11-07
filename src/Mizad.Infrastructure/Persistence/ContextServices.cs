using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Mizad.Application.Abstractions;
using Mizad.Application.Abstractions.Configuration;
using Mizad.Application.Abstractions.Database;
using Mizad.Domain.Common;

namespace Mizad.Infrastructure.Persistence;

/// <summary>Shared options for every <see cref="MizadDbContext"/> (retry strategy, timeout, interceptors).</summary>
internal static class MizadDbContextOptions
{
    public const int DefaultCommandTimeoutSeconds = 30;

    public static DbContextOptions<MizadDbContext> Build(string connectionString, params IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<MizadDbContext>();
        builder.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            sql.CommandTimeout(DefaultCommandTimeoutSeconds);
        });

        if (interceptors.Length > 0)
        {
            builder.AddInterceptors(interceptors);
        }

        return builder.Options;
    }
}

/// <summary>
/// Creates a context with the CURRENT connection settings each time, so a connection saved by the setup wizard
/// is used immediately without restarting (D-004, D-044).
/// </summary>
internal sealed class MizadDbContextFactory(IConnectionStringProvider connectionStringProvider, AuditStampInterceptor auditInterceptor)
    : IDbContextFactory<MizadDbContext>
{
    public MizadDbContext CreateDbContext() =>
        new(MizadDbContextOptions.Build(connectionStringProvider.GetConnectionString(), auditInterceptor));
}

/// <summary>Used by <c>dotnet ef</c> only. Generating migrations does not connect to a database.</summary>
internal sealed class MizadDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MizadDbContext>
{
    public MizadDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MIZAD_DESIGN_SQL")
            ?? "Server=.;Database=Mizad_Design;Integrated Security=True;TrustServerCertificate=True";
        return new MizadDbContext(MizadDbContextOptions.Build(connectionString));
    }
}

/// <summary>Reads the machine connection settings (connection.json) and builds the connection string.</summary>
internal sealed class ConnectionStringProvider(IConnectionSettingsStore store, IConnectionStringFactory factory) : IConnectionStringProvider
{
    private readonly Lock _gate = new();
    private string? _connectionString;
    private bool _loaded;

    public bool IsConfigured
    {
        get
        {
            EnsureLoaded();
            return _connectionString is not null;
        }
    }

    public string GetConnectionString()
    {
        EnsureLoaded();
        return _connectionString
            ?? throw new InvalidOperationException("Database connection is not configured (connection.json is missing).");
    }

    public void Reload()
    {
        lock (_gate)
        {
            _loaded = false;
            _connectionString = null;
        }

        EnsureLoaded();
    }

    private void EnsureLoaded()
    {
        lock (_gate)
        {
            if (_loaded)
            {
                return;
            }

            var settings = store.Load();
            _connectionString = settings is null ? null : factory.Create(settings);
            _loaded = true;
        }
    }
}

/// <summary>Fixed connection string — tests and tools.</summary>
internal sealed class StaticConnectionStringProvider(string connectionString) : IConnectionStringProvider
{
    public bool IsConfigured => true;

    public string GetConnectionString() => connectionString;

    public void Reload()
    {
    }
}

/// <summary>Stamps creation/modification time and user on save (task 2.10).</summary>
internal sealed class AuditStampInterceptor(IClock clock, ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.UtcNow;
        var userId = currentUser.UserId;
        foreach (var entry in context.ChangeTracker.Entries<IHasCreationStamp>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.MarkCreated(now, userId);
            }
            else if (entry.State == EntityState.Modified && entry.Entity is IAuditable auditable)
            {
                auditable.MarkUpdated(now, userId);
            }
        }
    }
}

/// <summary>Placeholder until the login session exists (Phase 3, task 3.6).</summary>
internal sealed class AnonymousCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public int? UserId => null;

    public string? UserName => null;

    public string? FullName => null;

    public bool HasPermission(string permissionCode) => false;
}

/// <summary>Translates SQL Server failures into Arabic, user-presentable errors (task 2.18).</summary>
public static class SqlErrorTranslator
{
    private static readonly Dictionary<string, string> UniqueIndexMessages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["UX_ProductBarcodes_Barcode"] = "هذا الباركود مستخدم لصنف آخر.",
        ["UX_Products_Code"] = "كود الصنف مستخدم مسبقاً.",
        ["UX_Customers_Code"] = "كود العميل مستخدم مسبقاً.",
        ["UX_Suppliers_Code"] = "كود المورد مستخدم مسبقاً.",
        ["UX_Users_UserName"] = "اسم المستخدم مستخدم مسبقاً.",
        ["UX_Roles_Name"] = "يوجد دور بهذا الاسم.",
        ["UX_Categories_Name"] = "يوجد تصنيف بهذا الاسم.",
        ["UX_Units_Name"] = "توجد وحدة بهذا الاسم.",
        ["UX_Warehouses_Name"] = "يوجد مخزن بهذا الاسم.",
        ["UX_Warehouses_Code"] = "كود المخزن مستخدم مسبقاً.",
        ["UX_CashAccounts_Name"] = "يوجد حساب نقدي بهذا الاسم.",
        ["UX_ExpenseCategories_Name"] = "يوجد تصنيف مصروفات بهذا الاسم.",
        ["UX_PurchaseInvoices_SupplierInvoiceNumber"] = "فاتورة المورد بهذا الرقم مسجلة مسبقاً.",
        ["UX_ProductUnits_ProductId_UnitId"] = "هذه الوحدة مضافة للصنف مسبقاً.",
    };

    /// <summary>SQL error numbers that mean "the server/network is unavailable" (D-046).</summary>
    private static readonly HashSet<int> ConnectivityErrors =
        [-2, 2, 53, 40, 64, 121, 233, 1231, 4060, 10053, 10054, 10060, 10061, 10065, 11001, 18456];

    public static bool IsConnectivityError(Exception exception) =>
        FindSqlException(exception) is { } sql && sql.Errors.Cast<SqlError>().Any(e => ConnectivityErrors.Contains(e.Number));

    public static bool TryTranslate(Exception exception, out Error error)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is DbUpdateConcurrencyException)
        {
            error = Error.Conflict("Database.Concurrency", "عُدّل هذا السجل من مستخدم آخر. أعد تحميل البيانات ثم حاول مجدداً.");
            return true;
        }

        if (IsConnectivityError(exception))
        {
            error = Error.Unexpected("Database.Unavailable", "تعذر الاتصال بقاعدة البيانات. تحقق من الشبكة وتشغيل الخادم ثم أعد المحاولة.");
            return true;
        }

        var sql = FindSqlException(exception);
        switch (sql?.Number)
        {
            case 2601 or 2627:
                var index = UniqueIndexMessages.Keys.FirstOrDefault(k => sql.Message.Contains(k, StringComparison.OrdinalIgnoreCase));
                error = Error.Conflict("Database.Duplicate", index is null ? "القيمة المدخلة مكررة." : UniqueIndexMessages[index]);
                return true;
            case 547:
                error = sql.Message.Contains("CHECK", StringComparison.OrdinalIgnoreCase)
                    ? Error.Validation("Database.CheckViolation", "البيانات لا تحقق قواعد السلامة المطلوبة.")
                    : Error.Conflict("Database.Reference", "لا يمكن تنفيذ العملية لارتباط السجل ببيانات أخرى.");
                return true;
            case 1205:
                error = Error.Conflict("Database.Deadlock", "تعارضت العملية مع عملية أخرى. أعد المحاولة.");
                return true;
            default:
                error = Error.None;
                return false;
        }
    }

    private static SqlException? FindSqlException(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is SqlException sql)
            {
                return sql;
            }

            exception = exception.InnerException;
        }

        return null;
    }
}
