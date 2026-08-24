using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Tests.Identifiers;

public sealed class IdentifierTests
{
    [Theory]
    [InlineData("audio")]
    [InlineData("audio.mp3")]
    [InlineData("provider-1")]
    [InlineData("image_avif")]
    public void FileFamilyIdAcceptsCanonicalIdentifierText(string value) => Assert.Equal(value, FileFamilyId.Parse(value).Value);

    [Theory]
    [InlineData("")]
    [InlineData("Audio")]
    [InlineData(" audio")]
    [InlineData("audio/mp3")]
    [InlineData("audio mp3")]
    public void FileFamilyIdRejectsNoncanonicalIdentifierText(string value) => Assert.Throws<ArgumentException>(() => FileFamilyId.Parse(value));

    [Fact]
    public void FormatIdRejectsIdentifierLongerThan64Characters() => Assert.Throws<ArgumentException>(() => FormatId.Parse(new string('a', 65)));
}
