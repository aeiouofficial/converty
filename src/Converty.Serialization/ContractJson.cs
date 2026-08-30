using System.Text.Json;
using System.Text.Json.Serialization;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Contracts.Jobs;
using Converty.Serialization.V1;

namespace Converty.Serialization;

/// <summary>
/// Strict, engine-independent JSON adapter for persisted/wire-facing contracts.
/// It performs no transport, filesystem, media parsing, process launch, or provider execution.
/// </summary>
public static class ContractJson
{
    private const int MaximumDepth = 32;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowTrailingCommas = false,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = MaximumDepth,
        WriteIndented = false,
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = MaximumDepth,
    };

    public static string Serialize(ConversionRequest value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var wire = new ConversionRequestWire
        {
            SchemaVersion = value.SchemaVersion,
            RequestId = value.RequestId.ToString("D"),
            Action = WireEnumText.ToWire(value.Action),
            Files = value.Files.ToArray(),
            TargetFormat = value.TargetFormat?.Value,
            PresetId = value.PresetId?.Value,
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static string Serialize(ConversionPreset value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var sortedOptions = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in value.Options)
        {
            sortedOptions.Add(pair.Key, pair.Value);
        }

        var wire = new ConversionPresetWire
        {
            SchemaVersion = value.SchemaVersion,
            Id = value.Id.Value,
            DisplayName = value.DisplayName,
            FamilyId = value.FamilyId.Value,
            OutputFormat = value.OutputFormat.Value,
            PreferredProvider = value.PreferredProvider?.Value,
            Options = sortedOptions,
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static string Serialize(CapabilityDescriptor value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var wire = new CapabilityDescriptorWire
        {
            SchemaVersion = value.SchemaVersion,
            ProviderId = value.ProviderId.Value,
            SourceFormat = value.SourceFormat.Value,
            TargetFormat = value.TargetFormat.Value,
            Mode = WireEnumText.ToWire(value.Mode),
            Priority = value.Priority,
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static string Serialize(FormatDescriptor value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var wire = new FormatDescriptorWire
        {
            SchemaVersion = value.SchemaVersion,
            Id = value.Id.Value,
            FamilyId = value.FamilyId.Value,
            DisplayName = value.DisplayName,
            CanonicalExtension = value.CanonicalExtension,
            Extensions = value.Extensions.ToArray(),
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static string Serialize(ConversionPlan value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var wire = new ConversionPlanWire
        {
            SchemaVersion = value.SchemaVersion,
            RequestId = value.RequestId.ToString("D"),
            Source = ToWire(value.Source),
            TargetFormat = value.TargetFormat.Value,
            ProviderId = value.ProviderId.Value,
            Mode = WireEnumText.ToWire(value.Mode),
            PresetId = value.PresetId?.Value,
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static string Serialize(JobStatusSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(ToWire(value), SerializerOptions);
    }

    public static string Serialize(JobControlRequest value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var wire = new JobControlRequestWire
        {
            SchemaVersion = value.SchemaVersion,
            Operation = WireEnumText.ToWire(value.Operation),
            JobId = value.JobId.ToString("D"),
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static string Serialize(JobControlResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var wire = new JobControlResponseWire
        {
            SchemaVersion = value.SchemaVersion,
            Operation = WireEnumText.ToWire(value.Operation),
            JobId = value.JobId.ToString("D"),
            Succeeded = value.Succeeded,
            Status = value.Status is null ? null : ToWire(value.Status),
            Reason = value.Reason is null ? null : WireEnumText.ToWire(value.Reason.Value),
        };
        return JsonSerializer.Serialize(wire, SerializerOptions);
    }

    public static ConversionRequest DeserializeConversionRequest(string json) =>
        Dispatch(json, "conversion request", DeserializeConversionRequestV1);

    public static ConversionPreset DeserializeConversionPreset(string json) =>
        Dispatch(json, "conversion preset", DeserializeConversionPresetV1);

    public static CapabilityDescriptor DeserializeCapabilityDescriptor(string json) =>
        Dispatch(json, "provider capability", DeserializeCapabilityDescriptorV1);

    public static FormatDescriptor DeserializeFormatDescriptor(string json) =>
        Dispatch(json, "format descriptor", DeserializeFormatDescriptorV1);

    public static ConversionPlan DeserializeConversionPlan(string json) =>
        Dispatch(json, "conversion plan", DeserializeConversionPlanV1);

    public static JobStatusSnapshot DeserializeJobStatusSnapshot(string json) =>
        Dispatch(json, "job status snapshot", DeserializeJobStatusSnapshotV1);

    public static JobControlRequest DeserializeJobControlRequest(string json) =>
        Dispatch(json, "job control request", DeserializeJobControlRequestV1);

    public static JobControlResponse DeserializeJobControlResponse(string json) =>
        Dispatch(json, "job control response", DeserializeJobControlResponseV1);

    private static T Dispatch<T>(string json, string contractName, Func<string, T> v1Reader)
    {
        var version = ReadSchemaVersion(json, contractName);
        return version switch
        {
            SchemaVersions.Current => v1Reader(json),
            _ => throw new JsonException($"Unsupported schema version {version} for {contractName}."),
        };
    }

    private static int ReadSchemaVersion(string json, string contractName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException($"{contractName} JSON is required.");
        }

        using var document = JsonDocument.Parse(json, DocumentOptions);
        RejectDuplicateProperties(document.RootElement);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"{contractName} root must be a JSON object.");
        }

        if (!document.RootElement.TryGetProperty("schemaVersion", out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var version))
        {
            throw new JsonException($"{contractName} requires an integer schemaVersion.");
        }

        return version;
    }

    private static void RejectDuplicateProperties(JsonElement element, string path = "$")
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw new JsonException($"Duplicate JSON property '{property.Name}' at {path}.");
                }

                RejectDuplicateProperties(property.Value, $"{path}.{property.Name}");
            }

            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                RejectDuplicateProperties(item, $"{path}[{index}]");
                index++;
            }
        }
    }

    private static ConversionRequest DeserializeConversionRequestV1(string json)
    {
        var wire = DeserializeWire<ConversionRequestWire>(json, "conversion request");
        return MapDomain("conversion request", () => new ConversionRequest(
            wire.SchemaVersion,
            ParseGuid(wire.RequestId, "requestId"),
            WireEnumText.ParseConversionAction(wire.Action),
            RequireArray(wire.Files, "files"),
            ParseOptionalFormatId(wire.TargetFormat),
            ParseOptionalPresetId(wire.PresetId)));
    }

    private static ConversionPreset DeserializeConversionPresetV1(string json)
    {
        var wire = DeserializeWire<ConversionPresetWire>(json, "conversion preset");
        return MapDomain("conversion preset", () => new ConversionPreset(
            wire.SchemaVersion,
            PresetId.Parse(RequireText(wire.Id, "id")),
            RequireText(wire.DisplayName, "displayName"),
            FileFamilyId.Parse(RequireText(wire.FamilyId, "familyId")),
            FormatId.Parse(RequireText(wire.OutputFormat, "outputFormat")),
            ParseOptionalProviderId(wire.PreferredProvider),
            RequireOptions(wire.Options)));
    }

    private static CapabilityDescriptor DeserializeCapabilityDescriptorV1(string json)
    {
        var wire = DeserializeWire<CapabilityDescriptorWire>(json, "provider capability");
        return MapDomain("provider capability", () => new CapabilityDescriptor(
            wire.SchemaVersion,
            ProviderId.Parse(RequireText(wire.ProviderId, "providerId")),
            FormatId.Parse(RequireText(wire.SourceFormat, "sourceFormat")),
            FormatId.Parse(RequireText(wire.TargetFormat, "targetFormat")),
            WireEnumText.ParseConversionMode(wire.Mode),
            wire.Priority));
    }

    private static FormatDescriptor DeserializeFormatDescriptorV1(string json)
    {
        var wire = DeserializeWire<FormatDescriptorWire>(json, "format descriptor");
        return MapDomain("format descriptor", () => new FormatDescriptor(
            FormatId.Parse(RequireText(wire.Id, "id")),
            FileFamilyId.Parse(RequireText(wire.FamilyId, "familyId")),
            RequireText(wire.DisplayName, "displayName"),
            RequireText(wire.CanonicalExtension, "canonicalExtension"),
            RequireArray(wire.Extensions, "extensions"),
            wire.SchemaVersion));
    }

    private static ConversionPlan DeserializeConversionPlanV1(string json)
    {
        var wire = DeserializeWire<ConversionPlanWire>(json, "conversion plan");
        return MapDomain("conversion plan", () => new ConversionPlan(
            wire.SchemaVersion,
            ParseGuid(wire.RequestId, "requestId"),
            FromWire(wire.Source),
            FormatId.Parse(RequireText(wire.TargetFormat, "targetFormat")),
            ProviderId.Parse(RequireText(wire.ProviderId, "providerId")),
            WireEnumText.ParseConversionMode(wire.Mode),
            ParseOptionalPresetId(wire.PresetId)));
    }

    private static JobStatusSnapshot DeserializeJobStatusSnapshotV1(string json)
    {
        var wire = DeserializeWire<JobStatusSnapshotWire>(json, "job status snapshot");
        return MapDomain("job status snapshot", () => FromWire(wire));
    }

    private static JobControlRequest DeserializeJobControlRequestV1(string json)
    {
        var wire = DeserializeWire<JobControlRequestWire>(json, "job control request");
        return MapDomain("job control request", () => new JobControlRequest(
            wire.SchemaVersion,
            WireEnumText.ParseJobControlOperation(wire.Operation),
            ParseCanonicalGuid(wire.JobId, "jobId")));
    }

    private static JobControlResponse DeserializeJobControlResponseV1(string json)
    {
        var wire = DeserializeWire<JobControlResponseWire>(json, "job control response");
        if (wire.Succeeded is null)
        {
            throw new JsonException("Property succeeded is required and must be a boolean.");
        }

        return MapDomain("job control response", () => new JobControlResponse(
            wire.SchemaVersion,
            WireEnumText.ParseJobControlOperation(wire.Operation),
            ParseCanonicalGuid(wire.JobId, "jobId"),
            wire.Succeeded.Value,
            wire.Status is null ? null : FromWire(wire.Status),
            wire.Reason is null ? null : WireEnumText.ParseJobControlFailureReason(wire.Reason)));
    }

    private static TWire DeserializeWire<TWire>(string json, string contractName)
        where TWire : class
    {
        try
        {
            return JsonSerializer.Deserialize<TWire>(json, SerializerOptions)
                ?? throw new JsonException($"{contractName} JSON did not contain an object.");
        }
        catch (JsonException)
        {
            throw;
        }
    }

    private static T MapDomain<T>(string contractName, Func<T> factory)
    {
        try
        {
            return factory();
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or OverflowException)
        {
            throw new JsonException($"Invalid {contractName} value.", exception);
        }
    }

    private static string RequireText(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException($"Property {propertyName} is required and must not be empty.");
        }

        return value;
    }

    private static string[] RequireArray(string[]? value, string propertyName) =>
        value ?? throw new JsonException($"Property {propertyName} is required.");

    private static SortedDictionary<string, string> RequireOptions(SortedDictionary<string, string>? value) =>
        value ?? throw new JsonException("Property options is required.");

    private static Guid ParseGuid(string? value, string propertyName)
    {
        if (!Guid.TryParse(value, out var parsed) || parsed == Guid.Empty)
        {
            throw new JsonException($"Property {propertyName} must be a non-empty UUID.");
        }

        return parsed;
    }

    private static Guid ParseCanonicalGuid(string? value, string propertyName)
    {
        if (!Guid.TryParseExact(value, "D", out Guid parsed) || parsed == Guid.Empty)
        {
            throw new JsonException($"Property {propertyName} must be a non-empty canonical UUID.");
        }

        return parsed;
    }

    private static FormatId? ParseOptionalFormatId(string? value) =>
        value is null ? null : FormatId.Parse(value);

    private static PresetId? ParseOptionalPresetId(string? value) =>
        value is null ? null : PresetId.Parse(value);

    private static ProviderId? ParseOptionalProviderId(string? value) =>
        value is null ? null : ProviderId.Parse(value);

    private static ProbedFileDescriptorWire ToWire(ProbedFileDescriptor value) => new()
    {
        Path = value.Path,
        FamilyId = value.FamilyId.Value,
        FormatId = value.FormatId.Value,
        Length = value.Length,
    };

    private static JobStatusSnapshotWire ToWire(JobStatusSnapshot value) => new()
    {
        SchemaVersion = value.SchemaVersion,
        JobId = value.JobId.ToString("D"),
        RequestId = value.RequestId.ToString("D"),
        State = WireEnumText.ToWire(value.State),
        Progress = value.Progress,
        Message = value.Message,
    };

    private static JobStatusSnapshot FromWire(JobStatusSnapshotWire value) => new(
        value.SchemaVersion,
        ParseGuid(value.JobId, "jobId"),
        ParseGuid(value.RequestId, "requestId"),
        WireEnumText.ParseConversionJobState(value.State),
        value.Progress,
        value.Message);

    private static ProbedFileDescriptor FromWire(ProbedFileDescriptorWire? value)
    {
        if (value is null)
        {
            throw new JsonException("Property source is required.");
        }

        return new ProbedFileDescriptor(
            RequireText(value.Path, "source.path"),
            FileFamilyId.Parse(RequireText(value.FamilyId, "source.familyId")),
            FormatId.Parse(RequireText(value.FormatId, "source.formatId")),
            value.Length);
    }
}
