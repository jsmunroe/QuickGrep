using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickGrep.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!ParseArgs(args, out var options, out var path, out var text))
            {
                WriteInstructions();
                return;
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                WriteError("QG cannot find the path specified.");
                return;
            }

            var isRecursive = options.ContainsKey("r");
            var isPattern = options.ContainsKey("p");

            options.TryGetValue("f", out var searchPattern);

            var grepper = new Grepper(path, isRecursive);

            var results = Enumerable.Empty<GrepMatch>();

            if (isPattern)
            {
                results = grepper.FindPattern(text);
            }
            else
            {
                results = grepper.Find(text);
            }

            foreach (var result in results)
            {
                WriteResult(result);
            }
        }

        private static bool ParseArgs(string[] args, out Dictionary<string, string> options, out string path, out string text)
        {
            options = null;
            path = null;
            text = null;

            var optionArgs = args.Where(p => p.StartsWith("-") || p.StartsWith("/"));
            var nonOptionArgs = args.Where(p => !p.StartsWith("-") && !p.StartsWith("/")).ToArray();

            options = optionArgs.SelectMany(GenericUriParserOptionArg).ToDictionary(p => p.Key, p => p.Value);

            switch (nonOptionArgs.Length)
            {
                case 2:
                    path = nonOptionArgs[0];
                    text = nonOptionArgs[1];
                    break;
                case 1:
                    path = Directory.GetCurrentDirectory();
                    text = nonOptionArgs[0];
                    break;
                default:
                    return false;
            }
            
            return true;
        }

        private static IEnumerable<KeyValuePair<string, string>> GenericUriParserOptionArg(string optionArg)
        {
            optionArg = optionArg.TrimStart('-', '/');

            if (optionArg.Length == 1)
            {
                return new Dictionary<string, string>
                {
                    { optionArg.ToLower(), null }
                };
            }

            if (optionArg.Contains(":"))
            {
                var parts = optionArg.Split(':');

                var optionKey = parts[0];
                var optionValue = string.Join(":", parts.Skip(1));

                return new Dictionary<string, string>
                {
                    { optionKey, optionValue }
                };
            }

            if (Regex.IsMatch(optionArg, @"^[a-z]$", RegexOptions.IgnoreCase))
            {
                return optionArg.Select(p => new KeyValuePair<string, string>(p.ToString(), null));
            }

            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        private static void WriteError(string errorMessage)
        {
            System.Console.WriteLine(errorMessage);
            System.Console.WriteLine();
        }

        private static void WriteInstructions()
        {
            System.Console.WriteLine("Performs a quick search on the file or directory of files.");
            System.Console.WriteLine();
            System.Console.WriteLine("QG [drive:][path][filename] <text> [/r] [/p] [/c] [/w] [/f:<filepattern>]");
            System.Console.WriteLine("  [drive:][path][filename]");
            System.Console.WriteLine("              Specifies drive, directory, and/or files to search.");
            System.Console.WriteLine();
            System.Console.WriteLine("  text        Text to search for in the files.");
            System.Console.WriteLine();
            System.Console.WriteLine("  /r          Searches recursively.");
            System.Console.WriteLine("  /p          Text is a regular expression pattern.");
            System.Console.WriteLine("  /c          The search should be case sensitive.");
            System.Console.WriteLine("  /w          The search should match the whole word.");
            System.Console.WriteLine("  /f          Use the filepattern value as a file search pattern.");
            System.Console.WriteLine();
        }
        private static void WriteResult(GrepMatch result)
        {
            System.Console.WriteLine($"\"{result.FilePath}\"");
            System.Console.WriteLine($"  Line: {result.Line}    Column: {result.Column}");
            System.Console.WriteLine();
        }

    }
}
