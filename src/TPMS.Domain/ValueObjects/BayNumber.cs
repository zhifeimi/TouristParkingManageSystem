namespace TPMS.Domain.ValueObjects;

public sealed record BayNumber
{
    public BayNumber(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Bay number is required.", nameof(value));
        }

        Value = trimmed;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
