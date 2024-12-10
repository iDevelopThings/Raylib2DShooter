using System;
using System.Text;

namespace SourceGenerators;

public static class StringBuilderExtensions {

    public static void AppendIndentedLine(this StringBuilder builder, string line, int indent = 4) {
        builder.Append(' ', indent * 4);
        builder.AppendLine(line);
    }
    public static void AppendIndented(this StringBuilder builder, int indent, Action<StringBuilder> action) {
        // builder.Append(' ', indent * 4);
        var tempSb = new StringBuilder();
        action(tempSb);
        // add the indent to each line
        foreach (var line in tempSb.ToString().Split('\n')) {
            builder.Append(' ', indent * 4);
            builder.AppendLine(line);
        }
    }

}