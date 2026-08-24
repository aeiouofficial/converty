namespace Converty.Contracts.Identifiers;

public sealed record ProviderId
{
    private ProviderId(string value) => Value = value;

    public string Value { get; }

    public static ProviderId Parse(string value)
    {
        if (!IdentifierRules.IsValid(value))
        {
            throw new ArgumentException("Provider IDs must be 1-64 lowercase ASCII identifier characters.", nameof(value));
        }

        return new ProviderId(value);
    }

    public override string ToString() => Value;
}
