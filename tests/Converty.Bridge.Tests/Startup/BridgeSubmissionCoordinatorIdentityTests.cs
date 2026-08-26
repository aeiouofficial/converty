using Converty.Bridge.Ipc;
using Converty.Bridge.Startup;
using Converty.Contracts;
using Converty.Contracts.Conversion;

namespace Converty.Bridge.Tests.Startup;

public sealed class BridgeSubmissionCoordinatorIdentityTests
{
    [Fact]
    public async Task ServerIdentityFailureNeverLaunchesHostOrRetries()
    {
        var client = new IdentityRejectingClient();
        var launcher = new CountingLauncher();
        var coordinator = new BridgeSubmissionCoordinator(
            client,
            launcher,
            startupTimeout: TimeSpan.FromSeconds(1),
            retryDelay: TimeSpan.FromMilliseconds(10));

        await Assert.ThrowsAsync<BridgeServerIdentityException>(
            () => coordinator.SubmitAsync(CreateRequest(), TestContext.Current.CancellationToken));

        Assert.Equal(0, launcher.StartCount);
        Assert.Equal(1, client.CallCount);
    }

    private static ConversionRequest CreateRequest() =>
        new(
            SchemaVersions.Current,
            Guid.NewGuid(),
            ConversionAction.ConvertUsingDefault,
            [@"C:\input\sample.wav"],
            targetFormat: null,
            presetId: null);

    private sealed class IdentityRejectingClient : IBridgeRequestClient
    {
        public int CallCount { get; private set; }

        public Task<BridgeSubmissionResult> SubmitAsync(ConversionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromException<BridgeSubmissionResult>(new BridgeServerIdentityException("fake server"));
        }
    }

    private sealed class CountingLauncher : IHostProcessLauncher
    {
        public int StartCount { get; private set; }

        public void StartHost() => StartCount++;
    }
}
