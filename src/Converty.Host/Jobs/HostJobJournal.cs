using System.Text.Json;
using Converty.Contracts;
using Converty.Contracts.Jobs;

namespace Converty.Host.Jobs;

public sealed class HostJobJournal : IHostJobJournal
{
    public const int MaximumEntries = 4096;
    public const long MaximumJournalBytes = 8L * 1024 * 1024;

    private const int JournalSchemaVersion = 1;
    private const string InterruptedMessage = "Interrupted by Host restart.";

    private static readonly HashSet<string> RootMembers = new(StringComparer.Ordinal)
    {
        "schemaVersion",
        "jobs",
    };

    private static readonly HashSet<string> JobMembers = new(StringComparer.Ordinal)
    {
        "schemaVersion",
        "jobId",
        "requestId",
        "state",
        "progress",
        "message",
    };

    private readonly string _path;
    private readonly string _temporaryPath;

    public HostJobJournal(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Journal path is required.", nameof(path));
        }

        _path = Path.GetFullPath(path);
        _temporaryPath = _path + ".tmp";
    }

    public IReadOnlyList<JobStatusSnapshot> LoadForRecovery()
    {
        RemoveOrphanTemporaryFile();
        if (!File.Exists(_path))
        {
            return [];
        }

        var info = new FileInfo(_path);
        if (info.Length is < 1 or > MaximumJournalBytes)
        {
            throw new InvalidDataException("Host job journal length is outside the allowed bounds.");
        }

        byte[] bytes = File.ReadAllBytes(_path);
        try
        {
            using JsonDocument document = JsonDocument.Parse(bytes);
            return ParseRoot(document.RootElement);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Host job journal is not valid strict JSON.", error);
        }
    }

    public void Commit(IReadOnlyCollection<JobStatusSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        if (snapshots.Count > MaximumEntries)
        {
            throw new InvalidDataException($"Host job journal exceeds the {MaximumEntries}-entry limit.");
        }

        JobStatusSnapshot[] ordered = ValidateAndOrderSnapshots(snapshots);
        string? directory = Path.GetDirectoryName(_path);
        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException("Journal path must have a parent directory.");
        }

        Directory.CreateDirectory(directory);
        try
        {
            using (var stream = new FileStream(
                       _temporaryPath,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 16 * 1024,
                       FileOptions.WriteThrough))
            {
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("schemaVersion", JournalSchemaVersion);
                    writer.WritePropertyName("jobs");
                    writer.WriteStartArray();
                    foreach (JobStatusSnapshot snapshot in ordered)
                    {
                        WriteSnapshot(writer, snapshot);
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.Flush();
                }

                if (stream.Length > MaximumJournalBytes)
                {
                    throw new InvalidDataException("Serialized Host job journal exceeds its byte limit.");
                }

                stream.Flush(flushToDisk: true);
            }

            File.Move(_temporaryPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(_temporaryPath))
            {
                File.Delete(_temporaryPath);
            }
        }
    }

    private static List<JobStatusSnapshot> ParseRoot(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Host job journal root must be an object.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        int? schemaVersion = null;
        JsonElement? jobs = null;
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!RootMembers.Contains(property.Name) || !seen.Add(property.Name))
            {
                throw new InvalidDataException($"Unknown or duplicate journal member '{property.Name}'.");
            }

            switch (property.Name)
            {
                case "schemaVersion":
                    schemaVersion = ReadRequiredInt(property.Value, "journal schemaVersion");
                    break;
                case "jobs":
                    jobs = property.Value;
                    break;
            }
        }

        if (schemaVersion != JournalSchemaVersion || jobs is null || jobs.Value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Host job journal schema version or jobs array is invalid.");
        }

        var result = new List<JobStatusSnapshot>();
        var jobIds = new HashSet<Guid>();
        var requestIds = new HashSet<Guid>();
        foreach (JsonElement element in jobs.Value.EnumerateArray())
        {
            if (result.Count >= MaximumEntries)
            {
                throw new InvalidDataException($"Host job journal exceeds the {MaximumEntries}-entry limit.");
            }

            JobStatusSnapshot parsed = ParseSnapshot(element);
            if (!jobIds.Add(parsed.JobId) || !requestIds.Add(parsed.RequestId))
            {
                throw new InvalidDataException("Host job journal contains duplicate job or request IDs.");
            }

            result.Add(RecoverSnapshot(parsed));
        }

        return result;
    }

    private static JobStatusSnapshot ParseSnapshot(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Host job journal entries must be objects.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        int? schemaVersion = null;
        Guid? jobId = null;
        Guid? requestId = null;
        ConversionJobState? state = null;
        double? progress = null;
        bool progressSeen = false;
        string? message = null;
        bool messageSeen = false;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!JobMembers.Contains(property.Name) || !seen.Add(property.Name))
            {
                throw new InvalidDataException($"Unknown or duplicate journal job member '{property.Name}'.");
            }

            switch (property.Name)
            {
                case "schemaVersion":
                    schemaVersion = ReadRequiredInt(property.Value, "job schemaVersion");
                    break;
                case "jobId":
                    jobId = ReadRequiredGuid(property.Value, "jobId");
                    break;
                case "requestId":
                    requestId = ReadRequiredGuid(property.Value, "requestId");
                    break;
                case "state":
                    state = ParseState(ReadRequiredString(property.Value, "state"));
                    break;
                case "progress":
                    progressSeen = true;
                    progress = property.Value.ValueKind switch
                    {
                        JsonValueKind.Null => null,
                        JsonValueKind.Number when property.Value.TryGetDouble(out double value) => value,
                        _ => throw new InvalidDataException("Journal progress must be a number or null."),
                    };
                    break;
                case "message":
                    messageSeen = true;
                    message = property.Value.ValueKind switch
                    {
                        JsonValueKind.Null => null,
                        JsonValueKind.String => property.Value.GetString(),
                        _ => throw new InvalidDataException("Journal message must be a string or null."),
                    };
                    break;
            }
        }

        if (schemaVersion != SchemaVersions.Current || jobId is null || requestId is null || state is null || !progressSeen || !messageSeen)
        {
            throw new InvalidDataException("Host job journal entry is missing required data or uses an unsupported schema version.");
        }

        try
        {
            return new JobStatusSnapshot(schemaVersion.Value, jobId.Value, requestId.Value, state.Value, progress, message);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException("Host job journal entry failed bounded domain validation.", error);
        }
    }

    private static JobStatusSnapshot RecoverSnapshot(JobStatusSnapshot snapshot)
    {
        if (snapshot.State is >= ConversionJobState.Probing and <= ConversionJobState.Committing)
        {
            return new JobStatusSnapshot(
                snapshot.SchemaVersion,
                snapshot.JobId,
                snapshot.RequestId,
                ConversionJobState.Failed,
                snapshot.Progress,
                InterruptedMessage);
        }

        return snapshot;
    }

    private static JobStatusSnapshot[] ValidateAndOrderSnapshots(IReadOnlyCollection<JobStatusSnapshot> snapshots)
    {
        var jobIds = new HashSet<Guid>();
        var requestIds = new HashSet<Guid>();
        var result = new JobStatusSnapshot[snapshots.Count];
        int index = 0;
        foreach (JobStatusSnapshot snapshot in snapshots)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            if (!jobIds.Add(snapshot.JobId) || !requestIds.Add(snapshot.RequestId))
            {
                throw new InvalidDataException("Cannot commit duplicate Host job or request IDs.");
            }

            result[index++] = snapshot;
        }

        Array.Sort(result, static (left, right) => left.JobId.CompareTo(right.JobId));
        return result;
    }

    private static void WriteSnapshot(Utf8JsonWriter writer, JobStatusSnapshot snapshot)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", snapshot.SchemaVersion);
        writer.WriteString("jobId", snapshot.JobId.ToString("D"));
        writer.WriteString("requestId", snapshot.RequestId.ToString("D"));
        writer.WriteString("state", StateName(snapshot.State));
        if (snapshot.Progress is { } progress)
        {
            writer.WriteNumber("progress", progress);
        }
        else
        {
            writer.WriteNull("progress");
        }

        if (snapshot.Message is { } message)
        {
            writer.WriteString("message", message);
        }
        else
        {
            writer.WriteNull("message");
        }

        writer.WriteEndObject();
    }

    private static int ReadRequiredInt(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int parsed))
        {
            throw new InvalidDataException($"Journal {name} must be an integer.");
        }

        return parsed;
    }

    private static Guid ReadRequiredGuid(JsonElement value, string name)
    {
        string text = ReadRequiredString(value, name);
        if (!Guid.TryParseExact(text, "D", out Guid parsed) || parsed == Guid.Empty)
        {
            throw new InvalidDataException($"Journal {name} must be a non-empty canonical GUID.");
        }

        return parsed;
    }

    private static string ReadRequiredString(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.String || value.GetString() is not { } text)
        {
            throw new InvalidDataException($"Journal {name} must be a string.");
        }

        return text;
    }

    private static ConversionJobState ParseState(string value) => value switch
    {
        "queued" => ConversionJobState.Queued,
        "probing" => ConversionJobState.Probing,
        "planning" => ConversionJobState.Planning,
        "staging" => ConversionJobState.Staging,
        "converting" => ConversionJobState.Converting,
        "validating" => ConversionJobState.Validating,
        "committing" => ConversionJobState.Committing,
        "completed" => ConversionJobState.Completed,
        "failed" => ConversionJobState.Failed,
        "cancelled" => ConversionJobState.Cancelled,
        "rejected" => ConversionJobState.Rejected,
        _ => throw new InvalidDataException("Host job journal contains an unsupported job state."),
    };

    private static string StateName(ConversionJobState state) => state switch
    {
        ConversionJobState.Queued => "queued",
        ConversionJobState.Probing => "probing",
        ConversionJobState.Planning => "planning",
        ConversionJobState.Staging => "staging",
        ConversionJobState.Converting => "converting",
        ConversionJobState.Validating => "validating",
        ConversionJobState.Committing => "committing",
        ConversionJobState.Completed => "completed",
        ConversionJobState.Failed => "failed",
        ConversionJobState.Cancelled => "cancelled",
        ConversionJobState.Rejected => "rejected",
        _ => throw new InvalidDataException("Cannot serialize an unsupported Host job state."),
    };

    private void RemoveOrphanTemporaryFile()
    {
        if (File.Exists(_temporaryPath))
        {
            File.Delete(_temporaryPath);
        }
    }
}
