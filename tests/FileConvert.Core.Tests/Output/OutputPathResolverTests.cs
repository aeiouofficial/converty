using FileConvert.Core.Output;

namespace FileConvert.Core.Tests.Output;

public sealed class OutputPathResolverTests
{
    [Fact]
    public void Resolve_replaces_extension_when_destination_is_free()
    {
        var resolver = new OutputPathResolver(_ => false);
        var input = Path.Combine("work", "chapter.wav");
        var result = resolver.Resolve(input, ".mp3");
        Assert.Equal(Path.Combine("work", "chapter.mp3"), result);
    }

    [Fact]
    public void Resolve_numbers_existing_destination_without_overwrite()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.Combine("work", "chapter.mp3"), Path.Combine("work", "chapter (1).mp3") };
        var resolver = new OutputPathResolver(existing.Contains);
        var result = resolver.Resolve(Path.Combine("work", "chapter.wav"), ".mp3");
        Assert.Equal(Path.Combine("work", "chapter (2).mp3"), result);
    }

    [Fact]
    public void Resolve_preserves_unicode_basename()
    {
        var resolver = new OutputPathResolver(_ => false);
        var result = resolver.Resolve(Path.Combine("work", "Hörbuch_日本語.wav"), ".flac");
        Assert.Equal(Path.Combine("work", "Hörbuch_日本語.flac"), result);
    }

    [Fact]
    public void Resolve_rejects_extension_containing_path_separator()
    {
        var resolver = new OutputPathResolver(_ => false);
        Assert.Throws<ArgumentException>(() => resolver.Resolve("chapter.wav", ".mp3/evil"));
    }

    [Fact]
    public void Resolve_fails_when_collision_search_limit_is_exhausted()
    {
        var resolver = new OutputPathResolver(_ => true, maxCollisionAttempts: 2);
        Assert.Throws<IOException>(() => resolver.Resolve("chapter.wav", ".mp3"));
    }
}
