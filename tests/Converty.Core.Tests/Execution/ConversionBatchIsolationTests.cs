using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Execution;

public sealed class ConversionBatchIsolationTests
{
    [Fact]
    public async Task RunAsyncContinuesAfterMiddleConversionFailureBeforeReportingBatchFailure()
    {
        string root = CreateTempDirectory();
        try
        {
            string first = Path.Combine(root, "first.wav");
            string failing = Path.Combine(root, "broken.wav");
            string last = Path.Combine(root, "last.wav");
            string firstExisting = Path.Combine(root, "first.mp3");
            string lastExisting = Path.Combine(root, "last.mp3");

            File.WriteAllBytes(first, [1]);
            File.WriteAllBytes(failing, [2]);
            File.WriteAllBytes(last, [3]);
            File.WriteAllBytes(firstExisting, [41]);
            File.WriteAllBytes(lastExisting, [42]);

            var worker = new SequencedWorkerClient([0, 7, 0]);
            var runner = new ConversionBatchRunner(
                ProductPresetRegistry.Default,
                new OutputPathResolver(),
                worker,
                TimeSpan.FromMinutes(5));

            ConversionFailedException error = await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(
                    PresetId.Parse("audio.mp3"),
                    [first, failing, last],
                    TestContext.Current.CancellationToken));

            Assert.Equal(failing, error.InputPath);
            Assert.Equal(3, worker.Inputs.Count);
            Assert.Equal(Path.Combine(root, "first (1).mp3"), Assert.Single(Directory.GetFiles(root, "first (1).mp3")));
            Assert.Equal(Path.Combine(root, "last (1).mp3"), Assert.Single(Directory.GetFiles(root, "last (1).mp3")));
            Assert.False(File.Exists(Path.Combine(root, "broken.mp3")));

            Assert.Equal([1], File.ReadAllBytes(first));
            Assert.Equal([2], File.ReadAllBytes(failing));
            Assert.Equal([3], File.ReadAllBytes(last));
            Assert.Equal([41], File.ReadAllBytes(firstExisting));
            Assert.Equal([42], File.ReadAllBytes(lastExisting));

            Assert.All(worker.Inputs, stagedPath => Assert.False(File.Exists(stagedPath)));
            Assert.All(worker.Outputs, stagedPath => Assert.False(File.Exists(stagedPath)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "converty-batch-isolation-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class SequencedWorkerClient(IReadOnlyList<int> exitCodes) : IConversionWorkerClient
    {
        private int _index;

        public List<string> Inputs { get; } = [];
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
            Outputs.Add(stagedOutputPath);

            int exitCode = exitCodes[_index++];
            File.WriteAllBytes(stagedOutputPath, exitCode == 0 ? [7] : [99]);
            return Task.FromResult(new ConversionWorkerResult(
                exitCode,
                exitCode == 0 ? string.Empty : "synthetic per-file failure"));
        }
    }
}
