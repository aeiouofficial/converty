using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Execution;

public sealed class ConversionBatchRunnerTests
{
    [Fact]
    public async Task RunAsyncConvertsMultipleFilesAndUsesNumberedCollisionPolicy()
    {
        string root = CreateTempDirectory();
        try
        {
            string first = Path.Combine(root, "a.wav");
            string second = Path.Combine(root, "b.wav");
            File.WriteAllBytes(first, [1]);
            File.WriteAllBytes(second, [2]);
            File.WriteAllBytes(Path.Combine(root, "a.mp3"), [9]);

            var launcher = new RecordingLauncher(exitCode: 0, writeOutput: true);
            var runner = new ConversionBatchRunner(
                ProductPresetRegistry.Default,
                new OutputPathResolver(),
                launcher,
                @"C:\Converty\tools\ffmpeg\ffmpeg.exe",
                TimeSpan.FromMinutes(5));

            ConversionBatchResult result = await runner.RunAsync(PresetId.Parse("audio.mp3"), [first, second]);

            Assert.Equal(2, result.Files.Count);
            Assert.Equal(Path.Combine(root, "a (1).mp3"), result.Files[0].OutputPath);
            Assert.Equal(Path.Combine(root, "b.mp3"), result.Files[1].OutputPath);
            Assert.Equal([first, second], launcher.Inputs);
            Assert.True(File.Exists(first));
            Assert.True(File.Exists(second));
            Assert.Equal([9], File.ReadAllBytes(Path.Combine(root, "a.mp3")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncRejectsUnsupportedInputBeforeLaunchingFfmpeg()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "notes.txt");
            File.WriteAllText(input, "data");
            var launcher = new RecordingLauncher(exitCode: 0, writeOutput: true);
            var runner = CreateRunner(launcher);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                runner.RunAsync(PresetId.Parse("audio.mp3"), [input]));

            Assert.Empty(launcher.Inputs);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncDeletesPartialOutputWhenFfmpegFails()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "voice.wav");
            string output = Path.Combine(root, "voice.mp3");
            File.WriteAllBytes(input, [1]);
            var launcher = new RecordingLauncher(exitCode: 1, writeOutput: true);
            var runner = CreateRunner(launcher);

            ConversionFailedException error = await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(PresetId.Parse("audio.mp3"), [input]));

            Assert.Equal(input, error.InputPath);
            Assert.Equal(output, error.OutputPath);
            Assert.False(File.Exists(output));
            Assert.True(File.Exists(input));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncFailsIfFfmpegReportsSuccessWithoutOutput()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "voice.wav");
            File.WriteAllBytes(input, [1]);
            var launcher = new RecordingLauncher(exitCode: 0, writeOutput: false);
            var runner = CreateRunner(launcher);

            await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(PresetId.Parse("audio.mp3"), [input]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ConversionBatchRunner CreateRunner(IFfmpegProcessLauncher launcher) =>
        new(
            ProductPresetRegistry.Default,
            new OutputPathResolver(),
            launcher,
            @"C:\Converty\tools\ffmpeg\ffmpeg.exe",
            TimeSpan.FromMinutes(5));

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "converty-batch-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class RecordingLauncher(int exitCode, bool writeOutput) : IFfmpegProcessLauncher
    {
        public List<string> Inputs { get; } = [];

        public Task<FfmpegExecutionResult> ExecuteAsync(
            string ffmpegPath,
            ProductPresetDefinition preset,
            string inputPath,
            string outputPath,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Inputs.Add(inputPath);
            if (writeOutput)
            {
                File.WriteAllBytes(outputPath, [7]);
            }

            return Task.FromResult(new FfmpegExecutionResult(exitCode, exitCode == 0 ? string.Empty : "test failure"));
        }
    }
}
