using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Execution;

public sealed class FfmpegProcessLauncherTests
{
    [Fact]
    public void CreateStartInfoUsesTrustedExecutableAndStructuredArguments()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.mp4.h264"));
        const string ffmpeg = @"C:\Program Files\Converty\tools\ffmpeg\ffmpeg.exe";
        const string input = @"C:\Media\odd & name; -x.mov";
        const string output = @"C:\Media\odd & name; -x.mp4";

        var launcher = new FfmpegProcessLauncher();
        System.Diagnostics.ProcessStartInfo startInfo = launcher.CreateStartInfo(ffmpeg, preset, input, output);

        Assert.Equal(ffmpeg, startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.CreateNoWindow);
        Assert.True(startInfo.RedirectStandardError);
        Assert.True(startInfo.RedirectStandardOutput);
        Assert.Empty(startInfo.Arguments);
        Assert.Contains(input, startInfo.ArgumentList);
        Assert.Contains(output, startInfo.ArgumentList);
        Assert.Equal(Path.GetDirectoryName(ffmpeg), startInfo.WorkingDirectory);
    }

    [Fact]
    public void CreateStartInfoDoesNotTreatFilenameMetacharactersAsCommands()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.flac"));
        const string input = @"C:\Media\a & whoami | calc.exe ; ' quoted [].wav";
        const string output = @"C:\Media\a & whoami | calc.exe ; ' quoted [].flac";

        var launcher = new FfmpegProcessLauncher();
        System.Diagnostics.ProcessStartInfo startInfo = launcher.CreateStartInfo(
            @"C:\Converty\tools\ffmpeg\ffmpeg.exe",
            preset,
            input,
            output);

        Assert.Contains(input, startInfo.ArgumentList);
        Assert.Contains(output, startInfo.ArgumentList);
        Assert.DoesNotContain("cmd.exe", startInfo.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", startInfo.FileName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TrustedResolverUsesOnlyApplicationLocalFfmpeg()
    {
        string root = Path.Combine(Path.GetTempPath(), "converty-ffmpeg-test-" + Guid.NewGuid().ToString("N"));
        string expected = Path.Combine(root, "tools", "ffmpeg", "ffmpeg.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(expected)!);
        File.WriteAllBytes(expected, []);
        try
        {
            Assert.Equal(Path.GetFullPath(expected), TrustedFfmpegPath.Resolve(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TrustedResolverFailsWhenBundledFfmpegIsMissing()
    {
        string root = Path.Combine(Path.GetTempPath(), "converty-ffmpeg-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            Assert.Throws<FileNotFoundException>(() => TrustedFfmpegPath.Resolve(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1801)]
    public async Task ExecuteRejectsInvalidTimeoutSeconds(int seconds)
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.mp3"));
        var launcher = new FfmpegProcessLauncher();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => launcher.ExecuteAsync(
            @"C:\Converty\tools\ffmpeg\ffmpeg.exe",
            preset,
            @"C:\Media\in.wav",
            @"C:\Media\out.mp3",
            TimeSpan.FromSeconds(seconds)));
    }
}
