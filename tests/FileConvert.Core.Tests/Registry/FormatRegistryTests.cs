using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;
using FileConvert.Core.Registry;

namespace FileConvert.Core.Tests.Registry;

public sealed class FormatRegistryTests
{
    [Fact]
    public void Registry_resolves_format_by_canonical_extension()
    {
        var wav = new FormatDescriptor(FormatId.Parse("audio.wav"), FileFamilyId.Parse("audio"), "WAV", ".wav", [".wav", ".wave"]);
        Assert.Equal(wav, new FormatRegistry([wav]).FindByExtension(".WAVE"));
    }

    [Fact]
    public void Registry_rejects_duplicate_format_id()
    {
        var first = new FormatDescriptor(FormatId.Parse("audio.wav"), FileFamilyId.Parse("audio"), "WAV", ".wav", [".wav"]);
        var second = new FormatDescriptor(FormatId.Parse("audio.wav"), FileFamilyId.Parse("audio"), "Wave", ".wave", [".wave"]);
        Assert.Throws<ArgumentException>(() => new FormatRegistry([first, second]));
    }

    [Fact]
    public void Registry_rejects_extension_collision_between_formats()
    {
        var first = new FormatDescriptor(FormatId.Parse("image.jpg"), FileFamilyId.Parse("image"), "JPEG", ".jpg", [".jpg", ".jpeg"]);
        var second = new FormatDescriptor(FormatId.Parse("image.other"), FileFamilyId.Parse("image"), "Other", ".other", [".jpeg"]);
        Assert.Throws<ArgumentException>(() => new FormatRegistry([first, second]));
    }
}
