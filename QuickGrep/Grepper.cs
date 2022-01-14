using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickGrep
{
    public class Grepper
    {
        private readonly Regex _carriageReturnRegex = new Regex(@"(?:([\n]+?))", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly string _path;
        private readonly bool _isRecursive;

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
        public Grepper(string path, bool isRecursive = false)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _isRecursive = isRecursive;
        }

        /// <summary>
        /// Find the given text (<paramref name="text"/>) in the file or files that this class is grepping on. 
        /// </summary>
        /// <param name="text">Text to find.</param>
        /// <param name="matchCase">Whether to match case in search.</param>
        /// <param name="matchWholeWord">Whether to match whole word in search.</param>
        /// <param name="searchPattern">File search pattern.</param>
        /// <returns>Found matches.</returns>
        public IEnumerable<GrepMatch> Find(string text, bool matchCase = false, bool matchWholeWord = false, string searchPattern = "*")
        {
            var pattern = Regex.Escape(text);

            if (matchWholeWord)
            {
                pattern = $@"\b{pattern}\b";
            }

            return FindPattern(pattern, matchCase);
        }

        /// <summary>
        /// Find the given regular expression pattern (<paramref name="pattern"/>) in the file or files that this class is grepping on. 
        /// </summary>
        /// <param name="pattern">Text to find.</param>
        /// <param name="matchCase">Whether to match case in search.</param>
        /// <param name="matchWholeWord">Whether to match whole word in search.</param>
        /// <param name="searchPattern">File search pattern.</param>
        /// <returns>Found matches.</returns>
        public IEnumerable<GrepMatch> FindPattern(string pattern, bool matchCase = false, bool matchWholeWord = false, string searchPattern = "*")
        {
            var regexOptions = RegexOptions.Compiled;

            if (matchCase == false)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            var regex = new Regex(pattern, regexOptions);

            return FindPattern(regex);
        }

        /// <summary>
        /// Use the given regular expression instance (<paramref name="regex"/>) to find matches in the file or files that this class is grepping on.
        /// </summary>
        /// <param name="regex">Regular expression instance.</param>
        /// <param name="searchPattern">File search pattern.</param>
        /// <returns>Found matches.</returns>
        public IEnumerable<GrepMatch> FindPattern(Regex regex, string searchPattern = "*")
        {
            var findContext = new FindContext
            {
                SearchPattern = searchPattern,
            };

            var attributes = File.GetAttributes(_path);

            if (attributes.HasFlag(FileAttributes.Directory))
            {
                var directory = new DirectoryInfo(_path);
                return FindInDirectory(directory, regex, findContext);
            }

            var file = new FileInfo(_path);
            return FineInFile(file, regex, findContext);
        }

        private IEnumerable<GrepMatch> FineInFile(FileInfo file, Regex regex, FindContext context)
        {
            using (var reader = file.OpenText())
            {
                var fileText = reader.ReadToEnd();

                return regex.Matches(fileText).OfType<Match>().Select(p => BuildMatch(file.FullName, fileText, p));
            }
        }

        private IEnumerable<GrepMatch> FindInDirectory(DirectoryInfo directory, Regex regex, FindContext context)
        {
            var directoryFiles = Enumerable.Empty<GrepMatch>();

            if (_isRecursive)
            {
                directoryFiles = directory.EnumerateDirectories().SelectMany(p => FindInDirectory(p, regex, context));
            }

            var files = directory.EnumerateFiles(context.SearchPattern).SelectMany(p => FineInFile(p, regex, context)).AsParallel().ToArray();

            return files.Concat(directoryFiles);
        }

        private GrepMatch BuildMatch(string filePath, string fileText, Match match)
        {
            var (column, row) = CoordinatesFromPosition(fileText, match.Index);

            return new GrepMatch
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Text = match.Value,
                Position = match.Index,
                Line = row + 1,
                Column = column + 1
            };
        }

        private (int, int) CoordinatesFromPosition(string input, int indexPosition)
        {
            var newLineIndices = _carriageReturnRegex.Matches(input)
                                                     .OfType<Match>()
                                                     .Select(p => p.Groups[1].Index)
                                                     .ToList();

            var index = newLineIndices.Count(p => p < indexPosition) - 1;

            var newLineIndex = -1;
            if (index >= 0)
            {
                newLineIndex = newLineIndices[index];
            }

            var column = indexPosition - newLineIndex - 1;
            var row = index + 1;

            return (column, row);
        }
    }

    internal class FindContext
    {
        public string SearchPattern { get; set; }
    }

    public class GrepMatch
    {
        /// <summary>
        /// Path to the matching file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Name of the matching file.
        /// </summary>
        public string FileName { get; set; }
        

        /// <summary>
        /// Matching text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Position in the text.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Line number within the text.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Column number within the line.
        /// </summary>
        public int Column { get; set; }

    }
}
