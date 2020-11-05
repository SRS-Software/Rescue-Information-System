#region

using System;

#endregion

namespace RIS.Core.Helper
{
    public static class Text
    {
        /// <summary>
        ///     takes a substring between two anchor strings (or the end of the string if that anchor is null) with trimmed start
        ///     and end
        /// </summary>
        /// <param name="_input">string to search</param>
        /// <param name="_start">an optional string to search after</param>
        /// <param name="_stop">an optional string to search before</param>
        /// <param name="_comparison">an optional comparison for the search</param>
        /// <returns>substring based on the search; null if error occured</returns>
        public static string FindString(string _input, string _start = null, string _stop = null,
            StringComparison _comparison = StringComparison.InvariantCulture)
        {
            if (string.IsNullOrEmpty(_input)) return null;

            var fromLength = (_start ?? string.Empty).Length;
            var startIndex = string.IsNullOrEmpty(_start) ? 0 : _input.IndexOf(_start, _comparison) + fromLength;
            if (startIndex < fromLength) return null;

            var endIndex = string.IsNullOrEmpty(_stop) ? _input.Length : _input.IndexOf(_stop, startIndex, _comparison);
            if (endIndex < 0) endIndex = _input.Length;

            var _result = _input.Substring(startIndex, endIndex - startIndex);
            return string.IsNullOrEmpty(_result) ? null : _result.TrimStart(' ').TrimEnd(' ');
        }

        public static string FindLine(string input, int lineNumber)
        {
            if (string.IsNullOrEmpty(input)) return null;

            var lines = input.Replace("\r", "").Split('\n');
            if (lines.Length < lineNumber)
                return null;

            var _result = lines[lineNumber - 1];
            return string.IsNullOrEmpty(_result) ? null : _result.TrimStart(' ').TrimEnd(' ');
        }

        public static string FindLine(string input, int lineNumberStart, int lineNumberStop)
        {
            if (string.IsNullOrEmpty(input)) return null;

            var lines = input.Replace("\r", "").Split('\n');

            if (lineNumberStop == -1)
                lineNumberStop = lines.Length;

            if (lines.Length < lineNumberStart || lines.Length < lineNumberStop)
                return null;

            var result = string.Empty;
            for (var lineNumber = lineNumberStart - 1; lineNumber < lineNumberStop; lineNumber++)
                result += lines[lineNumber] + Environment.NewLine;

            return string.IsNullOrEmpty(result) ? null : result.TrimStart(' ').TrimEnd(' ');
        }

        public static string FindLine(string input, string lineNumber)
        {
            if (!int.TryParse(lineNumber, out var rowNumber))
                return null;
            return FindLine(input, rowNumber);
        }

        public static string FindLine(string input, string lineNumberStart, string lineNumberStop)
        {
            if (!int.TryParse(lineNumberStart, out var startLine))
                return null;

            // if stop line exists read given lines
            if (!int.TryParse(lineNumberStop, out var stopLine))
                return FindLine(input, startLine);
            return FindLine(input, startLine, stopLine);
        }

        /// <summary>
        ///     check if a string contains a other string
        /// </summary>
        /// <param name="_input">string to search in</param>
        /// <param name="_search">string to search</param>
        /// <param name="_comparison"></param>
        /// <returns>true if string is in other string</returns>
        public static bool CheckString(string _input, string _search,
            StringComparison _comparison = StringComparison.InvariantCulture)
        {
            if (string.IsNullOrEmpty(_input)) return false;

            var fromLength = (_search ?? string.Empty).Length;
            var startIndex = !string.IsNullOrEmpty(_search) ? _input.IndexOf(_search, _comparison) + fromLength : 0;
            if (startIndex < fromLength) return false;

            return true;
        }
    }
}