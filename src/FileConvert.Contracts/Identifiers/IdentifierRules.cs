namespace FileConvert.Contracts.Identifiers;

internal static class IdentifierRules
{
    internal const int MaximumLength = 64;

    internal static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaximumLength)
        {
            return false;
        }

        if (!IsLowerAsciiLetterOrDigit(value[0]))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (IsLowerAsciiLetterOrDigit(character) || character is '.' or '_' or '-')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool IsLowerAsciiLetterOrDigit(char value) =>
        value is >= 'a' and <= 'z' or >= '0' and <= '9';
}
