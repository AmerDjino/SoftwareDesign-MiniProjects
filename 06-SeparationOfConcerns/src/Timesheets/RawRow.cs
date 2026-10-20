namespace Timesheets;

/// <summary>One non-blank data line of the CSV text, split into trimmed fields that have not been interpreted yet.</summary>
/// <param name="LineNumber">The 1-based physical line in the CSV text (the header is line 1).</param>
/// <param name="Fields">The trimmed fields, as many as the line had.</param>
public sealed record RawRow(int LineNumber, IReadOnlyList<string> Fields);
