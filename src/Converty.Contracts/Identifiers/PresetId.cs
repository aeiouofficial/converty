namespace Converty.Contracts.Identifiers;

public sealed record PresetId
{
    private PresetId(string value) => Value = value;

    public string Value { get; }

    public static PresetId Parse(string value)
    {
        if (!IdentifierRules.IsValid(value))
        {
            throw new ArgumentException("Preset IDs must be 1-64 lowercase ASCII identifier characters.", nameof(value));
        }

        return new PresetId(value);
    }

    public override string ToString() => Value;
}
