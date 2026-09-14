using System.Reflection;
using Converty.Core.Execution;

namespace Converty.Core.Tests.Execution;

public sealed class ConversionModeArgumentTests
{
    [Theory]
    [InlineData("copy")]
    [InlineData("remux")]
    [InlineData("transcode")]
    [InlineData("transform")]
    public void ParseAcceptsOnlyCanonicalBoundedValues(string value)
    {
        Type type = GetModeArgumentType();
        MethodInfo parse = Assert.IsType<MethodInfo>(type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static));

        object? result = parse.Invoke(null, [value]);

        Assert.Equal(value, result!.ToString()!.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("REMUX")]
    [InlineData(" remux")]
    [InlineData("remux ")]
    [InlineData("copy;--input")]
    [InlineData("4")]
    [InlineData("unknown")]
    public void ParseRejectsNonCanonicalOrUnboundedValues(string value)
    {
        Type type = GetModeArgumentType();
        MethodInfo parse = Assert.IsType<MethodInfo>(type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static));

        TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, [value]));

        Assert.IsAssignableFrom<ArgumentException>(error.InnerException);
    }

    private static Type GetModeArgumentType()
    {
        Type? type = typeof(IConversionWorkerClient).Assembly.GetType(
            "Converty.Core.Execution.ConversionModeArgument",
            throwOnError: false,
            ignoreCase: false);
        Assert.NotNull(type);
        return type!;
    }
}
