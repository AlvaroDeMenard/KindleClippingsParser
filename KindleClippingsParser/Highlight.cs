using System;
using System.Text.RegularExpressions;

namespace KindleClippingsParser
{
    public class Highlight
    {
        public Highlight(Match match)
        {
            Book = new Book(match.Groups["title"].Value, match.Groups["author"].Value);
            Text = match.Groups["highlight"].Value;
            if (string.IsNullOrEmpty(match.Groups["page"].Value))
            {
                int loc;
                if (int.TryParse(match.Groups["locationStart"].Value, out loc))
                {
                    Loc = loc;
                }
                else
                {
                    Console.WriteLine("Failed to parse loc for " + Book);
                }
            }
            else
            {
                int page;
                if (int.TryParse(match.Groups["page"].Value, out page))
                {
                    Page = page;
                }
                else
                {
                    Console.WriteLine("Failed to parse page for " + Book);
                }
            }
        }

        public Book Book { get; set; }
        public string Text { get; set; }
        public int? Page { get; set; }
        public int? Loc { get; set; }

        public int PageOrLoc => Page.HasValue ? Page.Value : Loc.Value;
    }
}