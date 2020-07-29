using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KindleClippingsParser
{
    public interface IOutputFormat
    {
        public string Output(IList<Highlight> highlights);

        public string Name { get; }
    }

    public class Pages : IOutputFormat
    {
        public string Name => "pages";

        public string Output(IList<Highlight> highlights)
        {
            var sb = new StringBuilder();
            foreach (var highlight in highlights)
            {
                sb.Append(highlight.Page.HasValue
                    ? "Page: " + highlight.Page.Value
                    : "Loc: " + highlight.Loc.Value);
                sb.Append($"\r\n\r\n{highlight.Text}\r\n\r\n----\r\n\r\n");
            }

            return sb.ToString();
        }
    }

    public class NoPages : IOutputFormat
    {
        public string Name => "nopages";

        public string Output(IList<Highlight> highlights)
        {
            return string.Join("", highlights.Select(x => "\r\n\r\n\"" + x.Text + "\""));
        }
    }

    public class Json : IOutputFormat
    {
        public string Name => "json";

        public string Output(IList<Highlight> highlights)
        {
            return JsonConvert.SerializeObject(highlights, Formatting.Indented);
        }
    }
}