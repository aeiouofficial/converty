namespace FileConvert.Contracts.Identifiers;

public sealed record FormatId
{
    private FormatId(string value) => Value = value;

    public string Value { get; }

    public static FormatId Parse(string value)
    {
        if (!IdentifierRules.IsValid(value))
        {
            throw new ArgumentException("Format IDs must be 1-64 lowercase ASCII identifier characters.", nameof(value));
        }

        return new FormatId(value);
    }

    public override string ToString() => Value;
}
