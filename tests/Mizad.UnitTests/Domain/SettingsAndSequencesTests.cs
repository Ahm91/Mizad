using Mizad.Domain.Administration;

namespace Mizad.UnitTests.Domain;

public sealed class SettingsAndSequencesTests
{
    [Fact]
    public void SettingKeys_AreUnique_AndHaveDefaults()
    {
        SettingKeys.All.Select(s => s.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count().ShouldBe(SettingKeys.All.Count);
        SettingKeys.All.ShouldAllBe(s => s.DefaultText != null);
    }

    [Fact]
    public void SettingValues_RoundTripWithInvariantCulture()
    {
        SettingValue.Parse(SettingValue.Format(12.5m), 0m).ShouldBe(12.5m);
        SettingValue.Parse(SettingValue.Format(true), false).ShouldBeTrue();
        SettingValue.Parse(SettingValue.Format(new TimeOnly(2, 30)), TimeOnly.MinValue).ShouldBe(new TimeOnly(2, 30));
        SettingValue.Parse(SettingValue.Format(BelowCostPolicy.Warn), BelowCostPolicy.Block).ShouldBe(BelowCostPolicy.Warn);
        SettingValue.Parse(SettingValue.Format(DayOfWeek.Saturday), DayOfWeek.Monday).ShouldBe(DayOfWeek.Saturday);
        SettingValue.Format(12.5m).ShouldBe("12.5");
    }

    [Fact]
    public void InvalidOrMissingSettingValue_FallsBackToDefault()
    {
        SettingValue.Parse("not-a-number", 7).ShouldBe(7);
        SettingValue.Parse<int>(null, 30).ShouldBe(30);
        SettingValue.Parse("Unknown", BelowCostPolicy.Block).ShouldBe(BelowCostPolicy.Block);
    }

    [Fact]
    public void DocumentNumbers_ArePrefixedAndPadded()
    {
        DocumentSequence.Format("SB", 1, 6).ShouldBe("SB-000001");
        DocumentSequence.Format("ADJ", 1234567, 6).ShouldBe("ADJ-1234567");
    }

    [Fact]
    public void EveryDocumentType_HasOneSequence()
    {
        DocumentSequenceDefaults.All.Select(s => s.DocumentType).Distinct().Count().ShouldBe(DocumentSequenceDefaults.All.Count);
        DocumentSequenceDefaults.All.Select(s => s.Prefix).Distinct().Count().ShouldBe(DocumentSequenceDefaults.All.Count);
    }
}
