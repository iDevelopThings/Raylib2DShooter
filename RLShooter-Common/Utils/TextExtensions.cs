namespace RLShooter.Common.Utils;

/// <summary>
/// Provides extension methods for formatting data sizes into human-readable formats.
/// </summary>
public static class TextExtensions {
    /// <summary>
    /// Formats a <see cref="nuint"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this nuint value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="nint"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this nint value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="ulong"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this ulong value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="long"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this long value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="uint"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this uint value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="int"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this int value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="ushort"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this ushort value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="short"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this short value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="byte"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this byte value) {
        return DataSizeFormatHelper.Format(value);
    }

    /// <summary>
    /// Formats a <see cref="sbyte"/> data size into a human-readable format.
    /// </summary>
    /// <param name="value">The data size in bytes.</param>
    /// <returns>A human-readable format of the data size.</returns>
    public static string FormatDataSize(this sbyte value) {
        return DataSizeFormatHelper.Format(value);
    }

    public static string FormatMsTime(this float value) {
        return FormatMsTime((double) value);
    }
    // Format double (ms time value) to shortened string
    public static string FormatMsTime(this double value) {
        var time = value;
        var unit = "ms";
        if (time < 1) {
            time *= 1000;
            unit =  "µs";
        } else if (time > 1000) {
            time /= 1000;
            unit =  "s";
        } else if (time > 1000) {
            time /= 1000;
            unit =  "m";
        } else if (time > 1000) {
            time /= 1000;
            unit =  "h";
        }

        return $"{time:0.##} {unit}";
    }

    public static string ToPascalCase(this string value) {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToUpper(value[0]) + value[1..];
    }
}