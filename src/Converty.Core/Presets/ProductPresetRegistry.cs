using System.Collections.ObjectModel;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Presets;

public sealed class ProductPresetRegistry
{
    private static readonly string[] VideoExtensions = [".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".mpeg", ".mpg", ".wmv"];
    private static readonly string[] AudioExtensions = [".wav", ".flac", ".mp3", ".m4a", ".aac", ".ogg", ".opus", ".wma"];
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".webp", ".bmp", ".gif", ".tif", ".tiff"];

    private readonly ReadOnlyCollection<ProductPresetDefinition> _presets;
    private readonly ReadOnlyDictionary<PresetId, ProductPresetDefinition> _byId;

    public ProductPresetRegistry(IEnumerable<ProductPresetDefinition> presets)
    {
        ArgumentNullException.ThrowIfNull(presets);
        ProductPresetDefinition[] snapshot = presets.ToArray();
        if (snapshot.Length == 0)
        {
            throw new ArgumentException("At least one product preset is required.", nameof(presets));
        }

        var byId = new Dictionary<PresetId, ProductPresetDefinition>();
        foreach (ProductPresetDefinition preset in snapshot)
        {
            ArgumentNullException.ThrowIfNull(preset);
            if (!byId.TryAdd(preset.Id, preset))
            {
                throw new ArgumentException($"Duplicate product preset ID: {preset.Id}.", nameof(presets));
            }
        }

        _presets = Array.AsReadOnly(snapshot);
        _byId = new ReadOnlyDictionary<PresetId, ProductPresetDefinition>(byId);
    }

    public static ProductPresetRegistry Default { get; } = new(CreateDefaults());

    public IReadOnlyList<ProductPresetDefinition> Presets => _presets;

    public ProductPresetDefinition GetRequired(PresetId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _byId.TryGetValue(id, out ProductPresetDefinition? preset)
            ? preset
            : throw new KeyNotFoundException($"Unknown Converty product preset '{id}'.");
    }

    public IReadOnlyList<ProductPresetDefinition> GetApplicable(IReadOnlyList<string> inputPaths)
    {
        ArgumentNullException.ThrowIfNull(inputPaths);
        if (inputPaths.Count == 0 || inputPaths.Any(string.IsNullOrWhiteSpace))
        {
            return Array.Empty<ProductPresetDefinition>();
        }

        return _presets
            .Where(preset => inputPaths.All(preset.SupportsPath))
            .Where(preset => !inputPaths.All(path =>
                string.Equals(Path.GetExtension(path), preset.OutputExtension, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    private static IEnumerable<ProductPresetDefinition> CreateDefaults()
    {
        yield return new ProductPresetDefinition(
            PresetId.Parse("video.mp4.h264"),
            "Convert to MP4",
            "Video",
            ProductMediaKind.Video,
            VideoExtensions,
            ".mp4",
            ["-map", "0:v:0?", "-map", "0:a:0?", "-c:v", "libx264", "-preset", "medium", "-crf", "23", "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("video.webm.vp9"),
            "Convert to WebM",
            "Video",
            ProductMediaKind.Video,
            VideoExtensions,
            ".webm",
            ["-map", "0:v:0?", "-map", "0:a:0?", "-c:v", "libvpx-vp9", "-crf", "32", "-b:v", "0", "-c:a", "libopus", "-b:a", "128k"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("extract.audio.mp3"),
            "Extract Audio to MP3",
            "Extract Audio",
            ProductMediaKind.Video,
            VideoExtensions,
            ".mp3",
            ["-vn", "-c:a", "libmp3lame", "-b:a", "192k"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("audio.mp3"),
            "Convert to MP3",
            "Audio",
            ProductMediaKind.Audio,
            AudioExtensions,
            ".mp3",
            ["-vn", "-c:a", "libmp3lame", "-b:a", "320k"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("audio.flac"),
            "Convert to FLAC",
            "Audio",
            ProductMediaKind.Audio,
            AudioExtensions,
            ".flac",
            ["-vn", "-c:a", "flac"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("audio.wav"),
            "Convert to WAV",
            "Audio",
            ProductMediaKind.Audio,
            AudioExtensions,
            ".wav",
            ["-vn", "-c:a", "pcm_s16le"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("image.png"),
            "Convert to PNG",
            "Image",
            ProductMediaKind.Image,
            ImageExtensions,
            ".png",
            ["-frames:v", "1", "-c:v", "png"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("image.jpeg"),
            "Convert to JPEG",
            "Image",
            ProductMediaKind.Image,
            ImageExtensions,
            ".jpg",
            ["-frames:v", "1", "-c:v", "mjpeg", "-q:v", "2"]);

        yield return new ProductPresetDefinition(
            PresetId.Parse("image.webp"),
            "Convert to WebP",
            "Image",
            ProductMediaKind.Image,
            ImageExtensions,
            ".webp",
            ["-frames:v", "1", "-c:v", "libwebp", "-quality", "85"]);
    }
}
