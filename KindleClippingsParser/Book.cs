using System;

namespace KindleClippingsParser
{
    public class Book : IEquatable<Book>
    {
        public Book(string title, string author)
        {
            Title = title;
            Author = author;
        }

        public string Title { get; set; }
        public string Author { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Book);
        }

        public bool Equals(Book other)
        {
            return other != null &&
                   Title == other.Title &&
                   Author == other.Author;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, Author);
        }

        public override string ToString()
        {
            return Author + " - " + Title;
        }
    }
}
