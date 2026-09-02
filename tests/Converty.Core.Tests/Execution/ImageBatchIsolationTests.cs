using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Execution;

public sealed class ImageBatchIsolationTests
{
    [Fact]
    public async Task RunAsyncContinuesAcrossMalformedImageMembersAndReportsFailureAfterBatch()
    {
        string root = CreateTempDirectory();
        try
        {
            string first = Path.Combine(root, "first.png");
            string failing = Path.Combine(root, "broken.jpg");
            string middle = Path.Combine(root, "middle.webp");
            string truncated = Path.Combine(root, "truncated.bmp");
            string last = Path.Combine(root, "last.jpeg");
            string middleExisting = Path.Combine(root, "middle.png");
            string lastExisting = Path.Combine(root, "last.png");

            File.WriteAllBytes(first, [1]);
            File.WriteAllBytes(failing, [2]);
            File.WriteAllBytes(middle, [3]);
            File.WriteAllBytes(truncated, [4]);
            File.WriteAllBytes(last, [5]);
            File.WriteAllBytes(middleExisting, [42]);
            File.WriteAllBytes(lastExisting, [43]);

            // For first.png -> image.png, the selected source is itself the occupied base
            // output path. OutputPathResolver must therefore preserve the source and publish
            // the successful conversion as first (1).png without any special-case setup.
            var worker = new SequencedWorkerClient([0, 7, 0, 7, 0]);
            var runner = new ConversionBatchRunner(
                ProductPresetRegistry.Default,
                new OutputPathResolver(),
                worker,
                TimeSpan.FromMinutes(5));

            ConversionFailedException error = await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(
                    PresetId.Parse("image.png"),
                    [first, failing, middle, truncated, last],
                    TestContext.Current.CancellationToken));

            Assert.Equal(failing, error.InputPath);
            Assert.Equal(5, worker.Inputs.Count);
            Assert.Equal(Path.Combine(root, "first (1).png"), Assert.Single(Directory.GetFiles(root, "first (1).png")));
            Assert.Equal(Path.Combine(root, "middle (1).png"), Assert.Single(Directory.GetFiles(root, "middle (1).png")));
            Assert.Equal(Path.Combine(root, "last (1).png"), Assert.Single(Directory.GetFiles(root, "last (1).png")));
            Assert.False(File.Exists(Path.Combine(root, "broken (1).png")));
            Assert.False(File.Exists(Path.Combine(root, "truncated (1).png")));

            Assert.Equal([1], File.ReadAllBytes(first));
            Assert.Equal([2], File.ReadAllBytes(failing));
            Assert.Equal([3], File.ReadAllBytes(middle));
            Assert.Equal([4], File.ReadAllBytes(truncated));
            Assert.Equal([5], File.ReadAllBytes(last));
            Assert.Equal([42], File.ReadAllBytes(middleExisting));
            Assert.Equal([43], File.ReadAllBytes(lastExisting));

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
        string path = Path.Combine(Path.GetTempPath(), "converty-image-batch-isolation-test-" + Guid.NewGuid().ToString("N"));
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
                exitCode == 0 ? string.Empty : "synthetic per-file image failure"));
        }
    }
}