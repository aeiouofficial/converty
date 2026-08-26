using Converty.Core.Output;

namespace Converty.Core.Tests.Output;

public sealed class OutputPathResolverTests
{
    [Fact]
    public void ResolveReplacesExtensionWhenDestinationIsFree()
    {
        var resolver = new OutputPathResolver(_ => false);
        var input = Path.Combine("work", "chapter.wav");
        var result = resolver.Resolve(input, ".mp3");
        Assert.Equal(Path.Combine("work", "chapter.mp3"), result);
    }

    [Fact]
    public void ResolveNumbersExistingDestinationWithoutOverwrite()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.Combine("work", "chapter.mp3"), Path.Combine("work", "chapter (1).mp3") };
        var resolver = new OutputPathResolver(existing.Contains);
        var result = resolver.Resolve(Path.Combine("work", "chapter.wav"), ".mp3");
        Assert.Equal(Path.Combine("work", "chapter (2).mp3"), result);
    }

    [Fact]
    public void ResolveTreatsDestinationDirectoryAsCollision()
    {
        string root = Path.Combine(Path.GetTempPath(), "converty-output-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string input = Path.Combine(root, "chapter.wav");
            string collidingDirectory = Path.Combine(root, "chapter.mp3");
            File.WriteAllBytes(input, [1]);
            Directory.CreateDirectory(collidingDirectory);

            var resolver = new OutputPathResolver();
            string result = resolver.Resolve(input, ".mp3");

            Assert.Equal(Path.Combine(root, "chapter (1).mp3"), result);
            Assert.True(Directory.Exists(collidingDirectory));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ResolvePreservesUnicodeBasename()
    {
        var resolver = new OutputPathResolver(_ => false);
        var result = resolver.Resolve(Path.Combine("work", "Hörbuch_日本語.wav"), ".flac");
        Assert.Equal(Path.Combine("work", "Hörbuch_日本語.flac"), result);
    }

    [Fact]
    public void ResolveRejectsExtensionContainingPathSeparator()
    {
        var resolver = new OutputPathResolver(_ => false);
        Assert.Throws<ArgumentException>(() => resolver.Resolve("chapter.wav", ".mp3/evil"));
    }

    [Fact]
    public void ResolveFailsWhenCollisionSearchLimitIsExhausted()
    {
        var resolver = new OutputPathResolver(_ => true, maxCollisionAttempts: 2);
        Assert.Throws<IOException>(() => resolver.Resolve("chapter.wav", ".mp3"));
    }
}
