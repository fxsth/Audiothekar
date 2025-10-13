namespace audiothekar_cli.Models;

public record ValueRepresentation<T> : IComparable<ValueRepresentation<T>>
{
    public ValueRepresentation(string displayName, T value)
    {
        DisplayName = displayName;
        Value = value;
    }
    public string DisplayName { get; set; } = string.Empty;
    public T Value { get; set; }
    public int CompareTo(ValueRepresentation<T>? other)
    {
        if (other == null) return 1;
        return string.Compare(DisplayName, other.DisplayName, StringComparison.Ordinal);
    }
}