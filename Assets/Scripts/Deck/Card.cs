using System;

namespace Stokehold.Core
{
    /// <summary>
    /// Карта.
    /// </summary>
    public class Card
    {
        public string Id { get; private set; }
        public string Title { get; private set; }

        public Card(string id, string title = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("У карты должен быть непустой Id", "id");

            Id = id;
            Title = string.IsNullOrEmpty(title) ? id : title;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
