namespace FileConvert.Core.Output;

public sealed class OutputPathResolver
{
    private readonly Func<string, bool> _exists;
    private readonly int _maxCollisionAttempts;

    public OutputPathResolver(Func<string, bool>? exists = null, int maxCollisionAttempts = 9999)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCollisionAttempts, 1);

        _exists = exists ?? File.Exists;
        _maxCollisionAttempts = maxCollisionAttempts;
    }

    public string Resolve(string inputPath, string targetExtension)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Input path is required.", nameof(inputPath));
        }

        var extension = NormalizeExtension(targetExtension);
        var directory = Path.GetDirectoryName(inputPath) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(inputPath);
        if (string.IsNullOrWhiteSpace(stem))
        {
            throw new ArgumentException("Input path must contain a filename.", nameof(inputPath));
        }

        var candidate = Path.Combine(directory, stem + extension);
        if (!_exists(candidate))
        {
            return candidate;
        }

        for (var index = 1; index <= _maxCollisionAttempts; index++)
        {
            candidate = Path.Combine(directory, $"{stem} ({index}){extension}");
            if (!_exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException($"Unable to resolve a free output path after {_maxCollisionAttempts} numbered attempts.");
    }

    private static string NormalizeExtension(string targetExtension)
    {
        if (string.IsNullOrWhiteSpace(targetExtension))
        {
            throw new ArgumentException("Target extension is required.", nameof(targetExtension));
        }

        var normalized = targetExtension.Trim().ToLowerInvariant();
        if (normalized.Contains('/') || normalized.Contains('\\'))
        {
            throw new ArgumentException("Target extension must not contain path separators.", nameof(targetExtension));
        }

        if (normalized[0] != '.')
        {
            normalized = "." + normalized;
        }

        if (normalized.Length is < 2 or > 17 || normalized[1..].Any(c => !char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException("Target extension must contain 1-16 ASCII alphanumeric characters.", nameof(targetExtension));
        }

        return normalized;
    }
}
