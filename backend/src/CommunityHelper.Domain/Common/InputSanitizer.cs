using System.Globalization;
using System.Text;

namespace CommunityHelper.Domain.Common;

/// <summary>
/// Normalizes free-text user input before it reaches domain entities.
///
/// - Trims surrounding whitespace.
/// - Strips NUL bytes (BSON forbids them in strings) and other C0/C1
///   control characters that enable log/terminal injection.
/// - Strips invisible format characters (Unicode category Cf: bidi marks,
///   zero-width spaces), a known homoglyph/spoofing vector.
///
/// HTML-encoding is deliberately NOT done here: output-encoding stays the
/// consumers' job (Angular auto-escapes interpolations; Flutter
/// <c>Text</c> never interprets markup), so stored data stays canonical.
/// </summary>
public static class InputSanitizer
{
    /// <summary>Sanitizes single-line input (names, locations, skills, ...).</summary>
    public static string Clean(string value) => CleanInternal(value, allowNewlines: false);

    /// <summary>Sanitizes multi-line input (bios, descriptions); preserves CR/LF.</summary>
    public static string CleanMultiline(string value) => CleanInternal(value, allowNewlines: true);

    private static string CleanInternal(string value, bool allowNewlines)
    {
        ArgumentNullException.ThrowIfNull(value);

        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (c == '\t')
            {
                sb.Append(c);
                continue;
            }

            if (allowNewlines && (c == '\r' || c == '\n'))
            {
                sb.Append(c);
                continue;
            }

            var category = char.GetUnicodeCategory(c);
            if (category is UnicodeCategory.Control or UnicodeCategory.Format)
            {
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString().Trim();
    }
}
