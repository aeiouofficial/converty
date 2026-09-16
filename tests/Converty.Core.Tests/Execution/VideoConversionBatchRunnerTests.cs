using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Capabilities;
using Converty.Core.Execution;
using Converty.Core.Output;
using Converty.Core.Planning;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Execution;

public sealed class VideoConversionBatchRunnerTests
{
    [Fact]
    public async Task RunAsyncVideoCopyProbesPlansExecutesPostValidatesAndPublishes()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "clip.mp4");
            byte[] sourceBytes = [1, 2, 3, 4];
            File.WriteAllBytes(input, sourceBytes);

            List<string> events = [];
            var probe = new SequencedProbeClient(
                events,
                MediaProbeResultV1.Success(CreateMp4Facts()),
                MediaProbeResultV1.Success(CreateMp4Facts()));
            var worker = new ModeRecordingWorkerClient(events, corruptCopy: false);
            ConversionBatchRunner runner = CreateVideoRunner(worker, probe);

            ConversionBatchResult result = await runner.RunAsync(
                PresetId.Parse("video.mp4.h264"),
                [input],
                TestContext.Current.CancellationToken);

            Assert.Equal(["probe", "worker:Copy", "probe"], events);
            Assert.Equal(ConversionMode.Copy, Assert.Single(worker.Modes));
            Assert.Equal(2, probe.Paths.Count);
            Assert.NotEqual(input, probe.Paths[0]);
            Assert.EndsWith(".partial.mp4", probe.Paths[1], StringComparison.OrdinalIgnoreCase);

            string published = Assert.Single(result.Files).OutputPath;
            Assert.Equal(Path.Combine(root, "clip (1).mp4"), published);
            Assert.Equal(sourceBytes, File.ReadAllBytes(published));
            Assert.Equal(sourceBytes, File.ReadAllBytes(input));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncVideoCopyHashMismatchNeverPublishes()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "clip.mp4");
            File.WriteAllBytes(input, [1, 2, 3, 4]);

            var probe = new SequencedProbeClient(
                [],
                MediaProbeResultV1.Success(CreateMp4Facts()),
                MediaProbeResultV1.Success(CreateMp4Facts()));
            var worker = new ModeRecordingWorkerClient([], corruptCopy: true);
            ConversionBatchRunner runner = CreateVideoRunner(worker, probe);

            await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(
                    PresetId.Parse("video.mp4.h264"),
                    [input],
                    TestContext.Current.CancellationToken));

            Assert.Equal(ConversionMode.Copy, Assert.Single(worker.Modes));
            Assert.False(File.Exists(Path.Combine(root, "clip (1).mp4")));
            Assert.Equal([1, 2, 3, 4], File.ReadAllBytes(input));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncVideoWrongPostProbeContainerNeverPublishes()
    {
        string root = CreateTempDirectory();
        try
        {
            string input = Path.Combine(root, "clip.mp4");
            File.WriteAllBytes(input, [1, 2, 3, 4]);

            var probe = new SequencedProbeClient(
                [],
                MediaProbeResultV1.Success(CreateMp4Facts()),
                MediaProbeResultV1.Success(CreateMp4Facts(container: MediaContainerId.WebM)));
            var worker = new ModeRecordingWorkerClient([], corruptCopy: false);
            ConversionBatchRunner runner = CreateVideoRunner(worker, probe);

            await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(
                    PresetId.Parse("video.mp4.h264"),
                    [input],
                    TestContext.Current.CancellationToken));

            Assert.False(File.Exists(Path.Combine(root, "clip (1).mp4")));
            Assert.Equal([1, 2, 3, 4], File.ReadAllBytes(input));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsyncVideoProbeFailureIsMemberLocalAndLaterValidMemberStillPublishes()
    {
        string root = CreateTempDirectory();
        try
        {
            string broken = Path.Combine(root, "broken.mp4");
            string valid = Path.Combine(root, "valid.mp4");
            File.WriteAllBytes(broken, [9, 9]);
            File.WriteAllBytes(valid, [1, 2, 3, 4]);

            var probe = new SequencedProbeClient(
                [],
                MediaProbeResultV1.Failure(MediaProbeFailureReason.UnsupportedInput),
                MediaProbeResultV1.Success(CreateMp4Facts()),
                MediaProbeResultV1.Success(CreateMp4Facts()));
            var worker = new ModeRecordingWorkerClient([], corruptCopy: false);
            ConversionBatchRunner runner = CreateVideoRunner(worker, probe);

            ConversionFailedException error = await Assert.ThrowsAsync<ConversionFailedException>(() =>
                runner.RunAsync(
                    PresetId.Parse("video.mp4.h264"),
                    [broken, valid],
                    TestContext.Current.CancellationToken));

            Assert.Equal(broken, error.InputPath);
            Assert.Single(worker.Modes);
            Assert.False(File.Exists(Path.Combine(root, "broken (1).mp4")));
            Assert.Equal([1, 2, 3, 4], File.ReadAllBytes(Path.Combine(root, "valid (1).mp4")));
            Assert.Equal([9, 9], File.ReadAllBytes(broken));
            Assert.Equal([1, 2, 3, 4], File.ReadAllBytes(valid));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ConversionBatchRunner CreateVideoRunner(
        IConversionWorkerClient worker,
        IMediaProbeClient probe) =>
        new(
            ProductPresetRegistry.Default,
            new OutputPathResolver(),
            worker,
            probe,
            CreateMp4Planner(),
            TimeSpan.FromMinutes(5));

    private static ConversionPlanner CreateMp4Planner()
    {
        ProviderId provider = VideoProductCapabilityCatalog.EngineProviderId;
        FormatId mp4 = FormatId.Parse("video.mp4");
        return new ConversionPlanner(new CapabilityGraph(
        [
            new CapabilityDescriptor(provider, mp4, mp4, ConversionMode.Copy, 100),
            new CapabilityDescriptor(provider, mp4, mp4, ConversionMode.Remux, 90),
            new CapabilityDescriptor(provider, mp4, mp4, ConversionMode.Transcode, 80),
        ]));
    }

    private static MediaProbeFactsV1 CreateMp4Facts(MediaContainerId container = MediaContainerId.Mp4) =>
        new(
            container,
            [
                new MediaStreamFactsV1(
                    index: 0,
                    kind: MediaStreamKind.Video,
                    codec: MediaCodecId.H264,
                    profile: MediaProfileId.H264High,
                    isDefault: true,
                    isAttachedPicture: false,
                    pixelFormat: MediaPixelFormatId.Yuv420p,
                    bitDepth: 8,
                    width: 1920,
                    height: 1080,
                    colorTransfer: MediaColorTransferId.Bt709,
                    hdrState: MediaHdrState.Sdr,
                    sampleRate: null,
                    channelCount: null,
                    channelLayout: MediaAudioChannelLayoutId.Unknown,
                    hasPolicyRelevantMetadata: false),
                new MediaStreamFactsV1(
                    index: 1,
                    kind: MediaStreamKind.Audio,
                    codec: MediaCodecId.Aac,
                    profile: MediaProfileId.Unknown,
                    isDefault: true,
                    isAttachedPicture: false,
                    pixelFormat: MediaPixelFormatId.Unknown,
                    bitDepth: null,
                    width: null,
                    height: null,
                    colorTransfer: MediaColorTransferId.Unknown,
                    hdrState: MediaHdrState.Unknown,
                    sampleRate: 48000,
                    channelCount: 2,
                    channelLayout: MediaAudioChannelLayoutId.Stereo,
                    hasPolicyRelevantMetadata: false),
            ],
            MediaProbeCompleteness.Complete,
            hasChapters: false,
            hasGlobalMetadata: false,
            hasPolicyRelevantStreamMetadata: false);

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "converty-video-pipeline-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class SequencedProbeClient : IMediaProbeClient
    {
        private readonly Queue<MediaProbeResultV1> _results;
        private readonly List<string> _events;

        public SequencedProbeClient(List<string> events, params MediaProbeResultV1[] results)
        {
            _events = events;
            _results = new Queue<MediaProbeResultV1>(results);
        }

        public List<string> Paths { get; } = [];

        public Task<MediaProbeResultV1> ProbeAsync(
            string stagedInputPath,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Paths.Add(stagedInputPath);
            _events.Add("probe");
            return Task.FromResult(_results.Dequeue());
        }
    }

    private sealed class ModeRecordingWorkerClient(List<string> events, bool corruptCopy) : IConversionWorkerClient
    {
        public List<ConversionMode> Modes { get; } = [];

        public Task<ConversionWorkerResult> ExecuteAsync(
            PresetId presetId,
            string stagedInputPath,
            string stagedOutputPath,
            TimeSpan timeout,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Video execution must use the explicit conversion mode overload.");

        public Task<ConversionWorkerResult> ExecuteAsync(
            PresetId presetId,
            ConversionMode mode,
            string stagedInputPath,
            string stagedOutputPath,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Modes.Add(mode);
            events.Add($"worker:{mode}");

            byte[] bytes = corruptCopy && mode == ConversionMode.Copy
                ? [0xEE]
                : File.ReadAllBytes(stagedInputPath);
            File.WriteAllBytes(stagedOutputPath, bytes);
            return Task.FromResult(new ConversionWorkerResult(0, string.Empty));
        }
    }
}