using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Tests.Identifiers;

public sealed class IdentifierTests
{
    [Theory]
    [InlineData("audio")]
    [InlineData("audio.mp3")]
    [InlineData("provider-1")]
    [InlineData("image_avif")]
    public void FileFamilyId_accepts_canonical_identifier_text(string value) => Assert.Equal(value, FileFamilyId.Parse(value).Value);

    [Theory]
    [InlineData("")]
    [InlineData("Audio")]
    [InlineData(" audio")]
    [InlineData("audio/mp3")]
    [InlineData("audio mp3")]
    public void FileFamilyId_rejects_noncanonical_identifier_text(string value) => Assert.Throws<ArgumentException>(() => FileFamilyId.Parse(value));

    [Fact]
    public void FormatId_rejects_identifier_longer_than_64_characters() => Assert.Throws<ArgumentException>(() => FormatId.Parse(new string('a', 65)));
}
