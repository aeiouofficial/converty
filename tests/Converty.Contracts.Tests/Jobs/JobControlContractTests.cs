using Converty.Contracts.Jobs;

namespace Converty.Contracts.Tests.Jobs;

public sealed class JobControlContractTests
{
    [Fact]
    public void RequestAcceptsSupportedValues()
    {
        Guid jobId = Guid.NewGuid();
        var result = new JobControlRequest(SchemaVersions.Current, JobControlOperation.Status, jobId);
        Assert.Equal(SchemaVersions.Current, result.SchemaVersion);
        Assert.Equal(JobControlOperation.Status, result.Operation);
        Assert.Equal(jobId, result.JobId);
    }

    [Fact]
    public void RequestRejectsUnsupportedSchema() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new JobControlRequest(999, JobControlOperation.Status, Guid.NewGuid()));

    [Fact]
    public void RequestRejectsUndefinedOperation() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new JobControlRequest(SchemaVersions.Current, (JobControlOperation)999, Guid.NewGuid()));

    [Fact]
    public void RequestRejectsEmptyJobId() =>
        Assert.Throws<ArgumentException>(() => new JobControlRequest(SchemaVersions.Current, JobControlOperation.Status, Guid.Empty));

    [Fact]
    public void SuccessfulResponseRequiresMatchingStatusAndNoReason()
    {
        Guid jobId = Guid.NewGuid();
        var status = Status(jobId, ConversionJobState.Queued);
        var result = new JobControlResponse(SchemaVersions.Current, JobControlOperation.Status, jobId, true, status, null);
        Assert.True(result.Succeeded);
        Assert.Same(status, result.Status);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void SuccessWithoutStatusIsRejected() =>
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current, JobControlOperation.Status, Guid.NewGuid(), true, null, null));

    [Fact]
    public void SuccessWithReasonIsRejected()
    {
        Guid jobId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current, JobControlOperation.Status, jobId, true, Status(jobId, ConversionJobState.Queued), JobControlFailureReason.JobNotFound));
    }

    [Fact]
    public void FailureWithoutReasonIsRejected() =>
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current, JobControlOperation.Status, Guid.NewGuid(), false, null, null));

    [Fact]
    public void JobNotFoundFailureForbidsStatus()
    {
        Guid jobId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current, JobControlOperation.Status, jobId, false, Status(jobId, ConversionJobState.Queued), JobControlFailureReason.JobNotFound));
    }

    [Theory]
    [InlineData(JobControlFailureReason.NotCancellable)]
    [InlineData(JobControlFailureReason.PersistenceFailure)]
    public void StatefulFailureRequiresStatus(JobControlFailureReason reason) =>
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current, JobControlOperation.Cancel, Guid.NewGuid(), false, null, reason));

    [Fact]
    public void ResponseRejectsMismatchedStatusJobId()
    {
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Status,
            Guid.NewGuid(),
            true,
            Status(Guid.NewGuid(), ConversionJobState.Queued),
            null));
    }

    [Fact]
    public void ResponseRejectsUndefinedFailureReason()
    {
        Guid jobId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Cancel,
            jobId,
            false,
            Status(jobId, ConversionJobState.Queued),
            (JobControlFailureReason)999));
    }

    private static JobStatusSnapshot Status(Guid jobId, ConversionJobState state) =>
        new(SchemaVersions.Current, jobId, Guid.NewGuid(), state, null, null);
}
