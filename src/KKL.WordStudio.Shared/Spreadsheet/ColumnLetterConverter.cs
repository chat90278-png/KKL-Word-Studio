namespace KKL.WordStudio.Shared.Spreadsheet;

/// <summary>
/// Converts between 1-based column indices and A1-style column letters
/// ("A", "B", ..., "Z", "AA", ...). Lives in Shared because both Domain
/// (DataRange.RangeReference) and Infrastructure (the OpenXML sheet reader)
/// need the exact same conversion — duplicating it in two layers would risk
/// the two ever disagreeing on off-by-one behavior.
/// </summary>
public static class ColumnLetterConverter
{
    public static string ToLetters(int columnIndex1Based)
    {
        if (columnIndex1Based < 1)
            throw new ArgumentOutOfRangeException(nameof(columnIndex1Based), "Column index must be 1-based (>= 1).");

        var result = string.Empty;
        var index = columnIndex1Based;
        while (index > 0)
        {
            var remainder = (index - 1) % 26;
            result = (char)('A' + remainder) + result;
            index = (index - 1) / 26;
        }
        return result;
    }

    public static int ToIndex(string columnLetters)
    {
        if (string.IsNullOrWhiteSpace(columnLetters))
            throw new ArgumentException("Column letters cannot be null or empty.", nameof(columnLetters));

        var index = 0;
        foreach (var c in columnLetters.ToUpperInvariant())
        {
            if (c is < 'A' or > 'Z')
                throw new ArgumentException($"Invalid column letter character: '{c}'.", nameof(columnLetters));
            index = index * 26 + (c - 'A' + 1);
        }
        return index;
    }

    /// <summary>Splits a cell reference like "B7" into its column letters ("B") and row number (7).</summary>
    public static (string ColumnLetters, int RowNumber) SplitCellReference(string cellReference)
    {
        var i = 0;
        while (i < cellReference.Length && char.IsLetter(cellReference[i])) i++;
        var letters = cellReference[..i];
        var row = int.Parse(cellReference[i..]);
        return (letters, row);
    }
}
