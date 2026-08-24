using FileConvert.Core.Output;

namespace FileConvert.Core.Tests.Output;

public sealed class OutputPathResolverPropertyTests
{
    [Fact]
    public void ResolvePreservesSeededUnicodeBasenamesAndNeverReturnsKnownCollision()
    {
        var random = new Random(0x0F17E0);
        string[] fragments = ["chapter", "Hörbuch", "日本語", "résumé", "данные", "mix_01", "δοκιμή"];

        for (var iteration = 0; iteration < 500; iteration++)
        {
            var basename = string.Join("-", Enumerable.Range(0, random.Next(1, 4)).Select(_ => fragments[random.Next(fragments.Length)]));
            var collisionCount = random.Next(0, 8);
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var directory = Path.Combine("work", $"case-{iteration}");
            var plain = Path.Combine(directory, basename + ".mp3");
            if (collisionCount > 0)
            {
                existing.Add(plain);
                for (var copy = 1; copy < collisionCount; copy++) existing.Add(Path.Combine(directory, $"{basename} ({copy}).mp3"));
            }

            var resolver = new OutputPathResolver(existing.Contains, maxCollisionAttempts: 16);
            var result = resolver.Resolve(Path.Combine(directory, basename + ".wav"), ".mp3");

            Assert.False(existing.Contains(result));
            Assert.Equal(directory, Path.GetDirectoryName(result));
            Assert.True(Path.GetFileNameWithoutExtension(result).StartsWith(basename, StringComparison.Ordinal));
            Assert.Equal(".mp3", Path.GetExtension(result));
        }
    }
}
