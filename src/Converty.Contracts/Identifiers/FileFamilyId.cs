namespace Converty.Contracts.Identifiers;

public sealed record FileFamilyId
{
    private FileFamilyId(string value) => Value = value;

    public string Value { get; }

    public static FileFamilyId Parse(string value)
    {
        if (!IdentifierRules.IsValid(value))
        {
            throw new ArgumentException("File family IDs must be 1-64 lowercase ASCII identifier characters.", nameof(value));
        }

        return new FileFamilyId(value);
    }

    public override string ToString() => Value;
}
