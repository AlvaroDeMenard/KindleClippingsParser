using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KindleClippingsParser
{
    internal class Program
    {
        //Format:
        //Title (Author)
        //- Your Highlight on page X | Location X-X | Added on LongDate
        //
        //Highlight string
        //==========

        private static Regex _pattern = new Regex(@"^(?<title>.+) \((?<author>.+)\)\r\n- ((Your Highlight on page (?<page>\d+) \| Location (?<locationStart>\d+)-(?<locationEnd>\d+))|(Your Highlight at location (?<locationStart>\d+)-(?<locationEnd>\d+))) \| Added on (?<date>.+)\r\n\r\n(?<highlight>.+)\r\n==========",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand(
              description: "Parses Kindle highlight files into useful formats.");

            rootCommand.AddOption(new Option<string>(new string[] { "--format", "--f" },
                () => "",
                "Select the output format. Default: pages and nopages. Also available: json. For multiple formats, separate them by comma."));

            rootCommand.AddArgument(new Argument<FileInfo>("highlightsFile",
                () => new FileInfo("My Clippings.txt"),
                "The path to the highlights file."));

            rootCommand.Handler = CommandHandler.Create<FileInfo, string>(ParseAndOutput);

            return rootCommand.Invoke(args);
        }

        private static void ParseAndOutput(FileInfo highlightsFile, string format)
        {
            if (highlightsFile == null || !File.Exists(highlightsFile.FullName))
            {
                Console.WriteLine("No such file");
                return;
            }

            string file = File.ReadAllText(highlightsFile.FullName);
            Dictionary<Book, List<Highlight>> parsed = Parse(file);
            Output(parsed, GetSelectedOutputFormats(format));
        }

        private static bool ContainsOrIsContained(string str1, string str2)
        {
            return str1.Length > str2.Length ? str1.Contains(str2) : str2.Contains(str1);
        }

        private static Dictionary<string, IOutputFormat> GetOutputClasses()
        {
            var outputInterface = typeof(IOutputFormat);

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => outputInterface.IsAssignableFrom(x) && !x.IsInterface)
                .ToList();

            return types.Select(x => (IOutputFormat)Activator.CreateInstance(x)).ToDictionary(x => x.Name, x => x);
        }

        private static Dictionary<Book, List<Highlight>> Parse(string str)
        {
            var highlightsTmp = new Dictionary<Book, List<Match>>();
            var highlights = new Dictionary<Book, List<Highlight>>();

            var matches = _pattern.Matches(str);
            //split them by book first
            for (int i = 0; i < matches.Count; i++)
            {
                AddMatch(highlightsTmp, matches[i]);
            }

            foreach (var kvp in highlightsTmp)
            {
                var key = kvp.Key;
                var bookMatches = kvp.Value;
                highlights.Add(key, new List<Highlight>());
                string prevTxt = "";

                for (int i = 0; i < bookMatches.Count; i++)
                {
                    var match = bookMatches[i];

                    string highlight = match.Groups["highlight"].Value;

                    if (prevTxt != "" && !ContainsOrIsContained(prevTxt, highlight))
                    {
                        highlights[key].Add(new Highlight(bookMatches[i - 1]));
                    }

                    if (i == bookMatches.Count - 1)
                    {
                        highlights[key].Add(new Highlight(match));
                    }
                    prevTxt = highlight;
                }
            }

            return highlights;
        }

        private static void AddMatch(Dictionary<Book, List<Match>> highlights, Match match)
        {
            var book = new Book(match.Groups["title"].Value, match.Groups["author"].Value);

            if (!highlights.ContainsKey(book))
            {
                highlights.Add(book, new List<Match>());
            }

            highlights[book].Add(match);
        }

        private static List<string> GetSelectedOutputFormats(string format)
        {
            List<string> selectedOutputFormats = new List<string>();
            Dictionary<string, IOutputFormat> outputClasses = GetOutputClasses();

            if (string.IsNullOrEmpty(format))
            {
                //default
                selectedOutputFormats.Add("pages");
                selectedOutputFormats.Add("nopages");
            }
            else
            {
                foreach (var f in format.Split(','))
                {
                    if (!outputClasses.ContainsKey(f))
                    {
                        throw new Exception("Selected output format " + f + " not found");
                    }
                    selectedOutputFormats.Add(f);
                }
            }

            return selectedOutputFormats;
        }

        private static void Output(Dictionary<Book, List<Highlight>> highlights, List<string> selectedOutputFormats)
        {
            Directory.CreateDirectory("notes");

            Dictionary<string, IOutputFormat> outputClasses = GetOutputClasses();

            foreach (var kvp in highlights)
            {
                foreach (var outputFormat in selectedOutputFormats)
                {
                    var toWrite = outputClasses[outputFormat].Output(kvp.Value);
                    var file = File.CreateText(Path.Combine("notes", ToSafeFileName(kvp.Key + "_" + outputFormat + ".txt")));
                    file.Write(toWrite);
                    file.Close();
                }
            }
        }

        private static string ToSafeFileName(string s)
        {
            return s
                .Replace("\\", "")
                .Replace("/", "")
                .Replace("\"", "")
                .Replace("*", "")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("|", "");
        }
    }
}