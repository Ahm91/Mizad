using Mizad.Domain.Common;

namespace Mizad.UnitTests.Domain;

public sealed class ResultTests
{
    private static readonly Error SampleError = Error.BusinessRule("Stock.Insufficient", "الرصيد غير كافٍ");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_CarriesTheError()
    {
        var result = Result.Failure(SampleError);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SampleError);
        result.Error.Kind.ShouldBe(ErrorKind.BusinessRule);
    }

    [Fact]
    public void Failure_WithNoneError_Throws()
    {
        Should.Throw<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void GenericFailure_ReadingValue_Throws()
    {
        var result = Result.Failure<int>(SampleError);

        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldContain("Stock.Insufficient");
    }

    [Fact]
    public void DomainException_FromError_KeepsArabicMessage()
    {
        var exception = new DomainException(SampleError);

        exception.Message.ShouldBe("الرصيد غير كافٍ");
        exception.Error.ShouldBe(SampleError);
    }
}
