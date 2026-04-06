namespace TPMS.Domain.ValueObjects;

public sealed record LicensePlate
{
    public LicensePlate(string value)
    {
        var normalized = new string(value
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("License plate is required.", nameof(value));
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
