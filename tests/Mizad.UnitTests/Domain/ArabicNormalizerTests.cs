using Mizad.Domain.Calculations;

namespace Mizad.UnitTests.Domain;

/// <summary>TS-807 (D-041): Arabic spellings that users treat as the same word normalize identically.</summary>
public sealed class ArabicNormalizerTests
{
    [Theory]
    [Trait("TS", "TS-807")]
    [InlineData("أحمد", "احمد")]
    [InlineData("إحمد", "احمد")]
    [InlineData("آمنة", "امنه")]
    [InlineData("مُحَمَّد", "محمد")]
    [InlineData("مكتبة", "مكتبه")]
    [InlineData("مصطفى", "مصطفي")]
    [InlineData("مسؤول", "مسوول")]
    [InlineData("شاطئ", "شاطي")]
    [InlineData("كـــتاب", "كتاب")]
    public void EquivalentSpellings_NormalizeToTheSameText(string input, string expected)
    {
        ArabicNormalizer.Normalize(input).ShouldBe(expected);
    }

    [Fact]
    public void WhitespaceIsTrimmedAndCollapsed_AndLatinIsLowerCased()
    {
        ArabicNormalizer.Normalize("  زيت   Afia\t1.5 لتر ").ShouldBe("زيت afia 1.5 لتر");
    }

    [Fact]
    public void ArabicIndicDigits_BecomeLatin()
    {
        ArabicNormalizer.Normalize("علبة ١٢ قطعة ۳").ShouldBe("علبه 12 قطعه 3");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyInput_GivesEmptyString(string? input)
    {
        ArabicNormalizer.Normalize(input).ShouldBe(string.Empty);
    }
}
