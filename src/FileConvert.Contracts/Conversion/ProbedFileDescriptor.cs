using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Conversion;

/// <summary>
/// Trusted description produced by a future isolated probe worker. This contract contains no parser logic.
/// </summary>
public sealed class ProbedFileDescriptor
{
    public const int MaximumPathLength = 32767;

    public ProbedFileDescriptor(string path, FileFamilyId familyId, FormatId formatId, long length)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > MaximumPathLength || path.Contains('\0'))
        {
            throw new ArgumentException($"Path must contain 1-{MaximumPathLength} characters.", nameof(path));
        }

        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(formatId);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Path = path;
        FamilyId = familyId;
        FormatId = formatId;
        Length = length;
    }

    public string Path { get; }
    public FileFamilyId FamilyId { get; }
    public FormatId FormatId { get; }
    public long Length { get; }
}
