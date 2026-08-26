using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Execution;

public sealed class ConversionBatchRunnerTests
{
    [Fact]
    public async Task RunAsyncConvertsMultipleFilesThroughPrivateStagingAndUsesNumberedCollisionPolicy()
    {
        string root = CreateTempDirectory();
        try
        {
            string first = Path.Combine(root, "a.wav");
            string second = Path.Combine(root, "b.wav");
            File.WriteAllBytes(first, [1]);
            File.WriteAllBytes(second, [2]);
            File.WriteAllBytes(Path.Combine(root, "a.mp3"), [9]);

            var worker = new RecordingWorkerClient(exitCode: 0, writeOutput: true);
            var runner = CreateRunner(worker);

            ConversionBatchResult result = await runner.RunAsync(
                PresetId.Parse("audio.mp3"),
                [first, second],
                TestContext.Current.CancellationToken);

            Assert.Equal(2, result.Files.Count);
            Assert.Equal(Path.Combine(root, "a (1).mp3"), result.Files[0].OutputPath);
            Assert.Equal(Path.Combine(root, "b.mp3"), result.Files[1].OutputPath);
            Assert.Equal(2, worker.Inputs.Count);
            Assert.Equal(2, worker.Outputs.Count);
            Assert.NotEqual(first, worker.Inputs[0]);
            Assert.NotEqual(second, worker.Inputs[1]);
            Assert.Equal([1], worker.InputBytes[0]);
            Assert.Equal([2], worker.InputBytes[1]);
            for (int index = 0; index < worker.Inputs.Count; ++index)
            {
                Assert.Equal(Path.GetDirectoryName(worker.Inputs[index]), Path.GetDirectoryName(worker.Outputs[index]));
                Assert.NotEqual(root, Path.GetDirectoryName(worker.Inputs[index]));
                Assert.EndsWith(".partial.mp3", worker.Outputs[index], StringComparison.OrdinalIgnoreCase);
            }

            Assert.All(worker.Inputs, stagedPath => Assert.False(File.Exists(stagedPath)));
            Assert.All(worker.Outputs, stagedPath => Assert.False(File.Exists(stagedPath)));
            Assert.True(File.Exists(first));
            Assert.True(File.Exists(second));
            Assert.Equal([1], File.ReadAllBytes(first));
            Assert.Equal([2], File.ReadAllBytes(second));
            Assert.Equal([9], File.ReadAllBytes(Path.Combine(root, "a.mp3")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncRejectsUnsupportedInputBeforeLaunchingWorker()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "notes.txt");
            File.WriteAllText(input, "data");
            var worker = new RecordingWorkerClient(exitCode: 0, writeOutput: true);
            var runner = CreateRunner(worker);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                runner.RunAsync(PresetId.Parse("audio.mp3"), [input], TestContext.Current.CancellationToken));

            Assert.Empty(worker.Inputs);
            Assert.Empty(worker.Outputs);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncDeletesOwnedStagingWhenWorkerFails()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "voice.wav");
            string output = Path.Combine(root, "voice.mp3");
            File.WriteAllBytes(input, [1]);
            var worker = new RecordingWorkerClient(exitCode: 1, writeOutput: true);
            var runner = CreateRunner(worker);

            ConversionFailedException error = await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(PresetId.Parse("audio.mp3"), [input], TestContext.Current.CancellationToken));

            Assert.Equal(input, error.InputPath);
            Assert.Equal(output, error.OutputPath);
            Assert.False(File.Exists(output));
            Assert.True(File.Exists(input));
            string temporaryOutput = Assert.Single(worker.Outputs);
            Assert.NotEqual(output, temporaryOutput);
            Assert.False(File.Exists(temporaryOutput));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncPreservesCompetingDestinationAndPublishesNextNumberedCopy()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "voice.wav");
            string competingOutput = Path.Combine(root, "voice.mp3");
            string expectedOutput = Path.Combine(root, "voice (1).mp3");
            File.WriteAllBytes(input, [1]);

            var worker = new RecordingWorkerClient(
                exitCode: 0,
                writeOutput: true,
                afterWrite: (_, _) => File.WriteAllBytes(competingOutput, [42]));
            var runner = CreateRunner(worker);

            ConversionBatchResult result = await runner.RunAsync(
                PresetId.Parse("audio.mp3"), [input], TestContext.Current.CancellationToken);

            Assert.Equal(expectedOutput, Assert.Single(result.Files).OutputPath);
            Assert.Equal([42], File.ReadAllBytes(competingOutput));
            Assert.Equal([7], File.ReadAllBytes(expectedOutput));
            Assert.False(File.Exists(Assert.Single(worker.Outputs)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncFailsIfWorkerReportsSuccessWithoutOutput()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "voice.wav");
            File.WriteAllBytes(input, [1]);
            var worker = new RecordingWorkerClient(exitCode: 0, writeOutput: false);
            var runner = CreateRunner(worker);

            await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(PresetId.Parse("audio.mp3"), [input], TestContext.Current.CancellationToken));

            Assert.False(File.Exists(Assert.Single(worker.Outputs)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ConversionBatchRunner CreateRunner(IConversionWorkerClient worker) =>
        new(ProductPresetRegistry.Default, new OutputPathResolver(), worker, TimeSpan.FromMinutes(5));

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "converty-batch-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class RecordingWorkerClient(
        int exitCode,
        bool writeOutput,
        Action<string, string>? afterWrite = null) : IConversionWorkerClient
    {
        public List<string> Inputs { get; } = [];
        public List<byte[]> InputBytes { get; } = [];
        public List<string> Outputs { get; } = [];

        public Task<ConversionWorkerResult> ExecuteAsync(
            PresetId presetId,
            string stagedInputPath,
            string stagedOutputPath,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Inputs.Add(stagedInputPath);
            InputBytes.Add(File.ReadAllBytes(stagedInputPath));
            Outputs.Add(stagedOutputPath);
            if (writeOutput)
            {
                File.WriteAllBytes(stagedOutputPath, [7]);
            }

            afterWrite?.Invoke(stagedInputPath, stagedOutputPath);
            return Task.FromResult(new ConversionWorkerResult(exitCode, exitCode == 0 ? string.Empty : "test failure"));
        }
    }
}
