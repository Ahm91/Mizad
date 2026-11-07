using System.Text;

namespace Mizad.Domain.Calculations;

/// <summary>
/// Normalizes Arabic text for tolerant search (D-041). SQL Server collations do NOT ignore tashkeel or unify
/// hamza forms (verified in Phase 1), so every Arabic search compares <c>SearchName</c> values produced here.
/// </summary>
public static class ArabicNormalizer
{
    /// <summary>
    /// Removes tashkeel and tatweel, unifies أ/إ/آ/ٱ→ا, ى→ي, ة→ه, ؤ→و, ئ→ي, converts Arabic-Indic digits to
    /// Latin, lower-cases Latin letters and collapses whitespace.
    /// </summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);
        var lastWasSpace = false;

        foreach (var ch in text.Trim())
        {
            if (IsTashkeel(ch) || ch == 'ـ')
            {
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace)
                {
                    builder.Append(' ');
                    lastWasSpace = true;
                }

                continue;
            }

            lastWasSpace = false;
            builder.Append(Map(ch));
        }

        return builder.ToString();
    }

    private static bool IsTashkeel(char ch) =>
        ch is >= 'ً' and <= 'ٟ' or 'ٰ' or >= 'ۖ' and <= 'ۭ';

    private static char Map(char ch) => ch switch
    {
        'أ' or 'إ' or 'آ' or 'ٱ' => 'ا',
        'ى' => 'ي',
        'ة' => 'ه',
        'ؤ' => 'و',
        'ئ' => 'ي',
        >= '٠' and <= '٩' => (char)('0' + (ch - '٠')),
        >= '۰' and <= '۹' => (char)('0' + (ch - '۰')),
        _ => char.ToLowerInvariant(ch),
    };
}
