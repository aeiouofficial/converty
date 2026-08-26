using Converty.Bridge.Shell;

namespace Converty.Bridge.Tests.Shell;

public sealed class ShellConversionRequestParserTests
{
    [Fact]
    public void ParseAcceptsKnownPresetAndAbsolutePaths()
    {
        string first = Path.GetFullPath(Path.Combine("work", "clip one.mov"));
        string second = Path.GetFullPath(Path.Combine("work", "clip & two.mov"));

        ShellConversionRequest request = ShellConversionRequestParser.Parse(
            ["--preset", "video.mp4.h264", "--", first, second]);

        Assert.Equal("video.mp4.h264", request.PresetId.Value);
        Assert.Equal([first, second], request.InputPaths);
    }

    [Fact]
    public void ParseTreatsLeadingDashAndMetacharactersAfterSeparatorAsPathData()
    {
        string input = Path.GetFullPath(Path.Combine("work", "--preset & whoami; [x].wav"));

        ShellConversionRequest request = ShellConversionRequestParser.Parse(
            ["--preset", "audio.mp3", "--", input]);

        Assert.Single(request.InputPaths);
        Assert.Equal(input, request.InputPaths[0]);
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public void ParseRejectsMalformedShape(string[] args)
    {
        Assert.Throws<ArgumentException>(() => ShellConversionRequestParser.Parse(args));
    }

    [Fact]
    public void ParseRejectsUnknownPreset()
    {
        string input = Path.GetFullPath(Path.Combine("work", "voice.wav"));

        Assert.Throws<KeyNotFoundException>(() => ShellConversionRequestParser.Parse(
            ["--preset", "audio.not-real", "--", input]));
    }

    [Fact]
    public void ParseRejectsRelativePath()
    {
        Assert.Throws<ArgumentException>(() => ShellConversionRequestParser.Parse(
            ["--preset", "audio.mp3", "--", "relative.wav"]));
    }

    public static TheoryData<string[]> InvalidArguments { get; } = new()
    {
        Array.Empty<string>(),
        new[] { "--preset" },
        new[] { "--preset", "audio.mp3" },
        new[] { "--preset", "audio.mp3", "--" },
        new[] { "--other", "audio.mp3", "--", Path.GetFullPath("voice.wav") },
        new[] { "--preset", "audio.mp3", "unexpected", Path.GetFullPath("voice.wav") },
        new[] { "--preset", "audio.mp3", "--", string.Empty },
    };
}
