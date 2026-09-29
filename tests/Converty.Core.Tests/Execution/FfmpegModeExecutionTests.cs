using System.Diagnostics;
using System.Reflection;
using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Presets;
using Converty.Provider.FFmpeg;

namespace Converty.Provider.FFmpeg.Tests;

public sealed class FfmpegModeExecutionTests
{
    [Fact]
    public void ExplicitRemuxModeProducesOnlyRemuxTokens()
    {
        ProcessStartInfo startInfo = CreateStartInfo(ConversionMode.Remux);

        Assert.Contains("copy", startInfo.ArgumentList);
        Assert.DoesNotContain("libx264", startInfo.ArgumentList);
        Assert.DoesNotContain("libvpx-vp9", startInfo.ArgumentList);
    }

    [Fact]
    public void ExplicitTranscodeModeProducesFixedTranscodeTokens()
    {
        ProcessStartInfo startInfo = CreateStartInfo(ConversionMode.Transcode);

        Assert.Contains("libx264", startInfo.ArgumentList);
        Assert.Contains("yuv420p", startInfo.ArgumentList);
        Assert.DoesNotContain("-hwaccel", startInfo.ArgumentList);
    }

    [Fact]
    public void UnsupportedPresetModeTupleFailsBeforeAnyProcessCanStart()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.mp4.h264"));
        MethodInfo method = GetModeAwareCreateStartInfo();

        TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => method.Invoke(
            null,
            [
                @"C:\Converty\tools\ffmpeg\ffmpeg.exe",
                preset,
                ConversionMode.Transform,
                @"C:\Media\input.mov",
                @"C:\Media\output.mp4",
            ]));

        Assert.IsType<InvalidOperationException>(error.InnerException);
    }

    private static ProcessStartInfo CreateStartInfo(ConversionMode mode)
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.mp4.h264"));
        MethodInfo method = GetModeAwareCreateStartInfo();
        object? result = method.Invoke(
            null,
            [
                @"C:\Converty\tools\ffmpeg\ffmpeg.exe",
                preset,
                mode,
                @"C:\Media\input.mov",
                @"C:\Media\output.mp4",
            ]);
        return Assert.IsType<ProcessStartInfo>(result);
    }

    private static MethodInfo GetModeAwareCreateStartInfo()
    {
        MethodInfo? method = typeof(FfmpegProcessLauncher).GetMethod(
            "CreateStartInfo",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(string),
                typeof(ProductPresetDefinition),
                typeof(ConversionMode),
                typeof(string),
                typeof(string),
            ],
            modifiers: null);
        Assert.NotNull(method);
        return method!;
    }
}
