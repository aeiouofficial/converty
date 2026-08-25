using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;
using Converty.Host.Jobs;

namespace Converty.Host.Tests.Jobs;

public sealed class HostJobQueueTests
{
    [Fact]
    public void AcceptedRequestCreatesQueuedStatus()
    {
        var queue = new HostJobQueue(capacity: 2);
        ConversionRequest request = CreateRequest(Guid.NewGuid());

        JobAdmissionResult result = queue.TryEnqueue(request);

        Assert.True(result.Accepted);
        Assert.NotEqual(Guid.Empty, result.JobId);
        Assert.Equal(1, queue.Count);
        Assert.True(queue.TryGet(result.JobId, out JobStatusSnapshot? status));
        Assert.NotNull(status);
        Assert.Equal(request.RequestId, status.RequestId);
        Assert.Equal(ConversionJobState.Queued, status.State);
    }

    [Fact]
    public void DuplicateRequestIsRejectedWithoutMutatingQueue()
    {
        var queue = new HostJobQueue(capacity: 2);
        ConversionRequest request = CreateRequest(Guid.NewGuid());
        JobAdmissionResult first = queue.TryEnqueue(request);

        JobAdmissionResult duplicate = queue.TryEnqueue(request);

        Assert.True(first.Accepted);
        Assert.False(duplicate.Accepted);
        Assert.Equal(JobAdmissionRejection.DuplicateRequest, duplicate.Rejection);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void CapacityLimitRejectsAdditionalWorkWithoutMutation()
    {
        var queue = new HostJobQueue(capacity: 1);
        Assert.True(queue.TryEnqueue(CreateRequest(Guid.NewGuid())).Accepted);

        JobAdmissionResult result = queue.TryEnqueue(CreateRequest(Guid.NewGuid()));

        Assert.False(result.Accepted);
        Assert.Equal(JobAdmissionRejection.QueueFull, result.Rejection);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void QueuedJobCanBeCancelled()
    {
        var queue = new HostJobQueue(capacity: 1);
        JobAdmissionResult admission = queue.TryEnqueue(CreateRequest(Guid.NewGuid()));

        Assert.True(queue.TryCancel(admission.JobId, out JobStatusSnapshot? cancelled));
        Assert.NotNull(cancelled);
        Assert.Equal(ConversionJobState.Cancelled, cancelled.State);
        Assert.True(queue.TryGet(admission.JobId, out JobStatusSnapshot? stored));
        Assert.Equal(ConversionJobState.Cancelled, stored!.State);
    }

    [Fact]
    public void UnknownCancelDoesNotMutateQueue()
    {
        var queue = new HostJobQueue(capacity: 1);

        Assert.False(queue.TryCancel(Guid.NewGuid(), out _));
        Assert.Equal(0, queue.Count);
    }

    private static ConversionRequest CreateRequest(Guid requestId) =>
        new(
            SchemaVersions.Current,
            requestId,
            ConversionAction.ConvertUsingDefault,
            [@"C:\input\sample.wav"],
            targetFormat: null,
            presetId: null);
}
