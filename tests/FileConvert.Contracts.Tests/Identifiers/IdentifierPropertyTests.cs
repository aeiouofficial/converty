using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Tests.Identifiers;

public sealed class IdentifierPropertyTests
{
    [Fact]
    public void FormatIdParseMatchesCanonicalAsciiGrammarForSeededInputs()
    {
        var random = new Random(0x5A17C0DE);
        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789._-ABCDEFGHIJKLMNOPQRSTUVWXYZ /\\:@";
        for (var iteration = 0; iteration < 5_000; iteration++)
        {
            var length = random.Next(0, 72);
            var chars = new char[length];
            for (var index = 0; index < chars.Length; index++) chars[index] = alphabet[random.Next(alphabet.Length)];
            var candidate = new string(chars);
            if (IsCanonical(candidate)) Assert.Equal(candidate, FormatId.Parse(candidate).Value);
            else Assert.Throws<ArgumentException>(() => FormatId.Parse(candidate));
        }
    }

    private static bool IsCanonical(string value)
    {
        if (value.Length is < 1 or > 64 || !IsLowerAsciiLetterOrDigit(value[0])) return false;
        return value.All(character => IsLowerAsciiLetterOrDigit(character) || character is '.' or '_' or '-');
    }

    private static bool IsLowerAsciiLetterOrDigit(char value) => value is >= 'a' and <= 'z' or >= '0' and <= '9';
}
