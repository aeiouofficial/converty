using System.Text.Json;
using Converty.Contracts;
using Converty.Contracts.Jobs;
using Converty.Serialization;

namespace Converty.Serialization.Tests;

public sealed class JobControlJsonTests
{
    private static readonly Guid JobId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RequestId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Theory]
    [InlineData(JobControlOperation.Status, "status")]
    [InlineData(JobControlOperation.Cancel, "cancel")]
    public void RequestRoundTripsCanonicalWireText(JobControlOperation operation, string wireOperation)
    {
        var source = new JobControlRequest(SchemaVersions.Current, operation, JobId);
        string json = ContractJson.Serialize(source);
        JobControlRequest result = ContractJson.DeserializeJobControlRequest(json);

        Assert.Equal(operation, result.Operation);
        Assert.Equal(JobId, result.JobId);
        Assert.Contains($"\"operation\":\"{wireOperation}\"", json, StringComparison.Ordinal);
        Assert.Contains("\"jobId\":\"11111111-1111-1111-1111-111111111111\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void SuccessResponseRoundTripsNestedStatusAndOmitsNullOptionals()
    {
        var status = new JobStatusSnapshot(SchemaVersions.Current, JobId, RequestId, ConversionJobState.Queued, null, null);
        var source = new JobControlResponse(SchemaVersions.Current, JobControlOperation.Status, JobId, true, status, null);
        string json = ContractJson.Serialize(source);
        JobControlResponse result = ContractJson.DeserializeJobControlResponse(json);

        Assert.True(result.Succeeded);
        Assert.Equal(JobControlOperation.Status, result.Operation);
        Assert.Equal(JobId, result.JobId);
        Assert.Equal(JobId, result.Status!.JobId);
        Assert.Equal(ConversionJobState.Queued, result.Status.State);
        Assert.Null(result.Reason);
        Assert.DoesNotContain("\"progress\":null", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"message\":null", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"reason\":null", json, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(JobControlFailureReason.JobNotFound, null)]
    [InlineData(JobControlFailureReason.NotCancellable, ConversionJobState.Converting)]
    [InlineData(JobControlFailureReason.PersistenceFailure, ConversionJobState.Queued)]
    public void FailureResponsesRoundTripCanonicalReason(JobControlFailureReason reason, ConversionJobState? state)
    {
        JobStatusSnapshot? status = state is null
            ? null
            : new JobStatusSnapshot(SchemaVersions.Current, JobId, RequestId, state.Value, null, null);
        var source = new JobControlResponse(SchemaVersions.Current, JobControlOperation.Cancel, JobId, false, status, reason);

        JobControlResponse result = ContractJson.DeserializeJobControlResponse(ContractJson.Serialize(source));

        Assert.False(result.Succeeded);
        Assert.Equal(reason, result.Reason);
        Assert.Equal(state, result.Status?.State);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"schemaVersion\":999,\"operation\":\"status\",\"jobId\":\"11111111-1111-1111-1111-111111111111\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"unknown\",\"jobId\":\"11111111-1111-1111-1111-111111111111\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"Status\",\"jobId\":\"11111111-1111-1111-1111-111111111111\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"{11111111-1111-1111-1111-111111111111}\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"extra\":true}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"operation\":\"cancel\",\"jobId\":\"11111111-1111-1111-1111-111111111111\"}")]
    public void InvalidControlRequestsAreRejected(string json) =>
        Assert.Throws<JsonException>(() => ContractJson.DeserializeJobControlRequest(json));

    [Theory]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"succeeded\":true,\"jobId\":\"11111111-1111-1111-1111-111111111111\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"succeeded\":false,\"jobId\":\"11111111-1111-1111-1111-111111111111\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"succeeded\":false,\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"reason\":\"notCancellable\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"succeeded\":false,\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"reason\":\"jobNotFound\",\"status\":{\"schemaVersion\":1,\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"requestId\":\"22222222-2222-2222-2222-222222222222\",\"state\":\"queued\"}}")]
    public void ContradictoryControlResponsesAreRejected(string json) =>
        Assert.Throws<JsonException>(() => ContractJson.DeserializeJobControlResponse(json));
}
