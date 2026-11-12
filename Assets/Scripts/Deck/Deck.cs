using System;
using System.Collections.Generic;

namespace Stokehold.Core
{
    /// <summary>
    /// Колода из трёх стопок: добор, рука, сброс.
    /// </summary>
    public class Deck
    {
        private readonly List<Card> _draw = new List<Card>();
        private readonly List<Card> _hand = new List<Card>();
        private readonly List<Card> _discard = new List<Card>();
        private readonly Random _rng;

        /// <summary>Карта ушла из добора в руку.</summary>
        public event Action<Card> CardDrawn;

        /// <summary>Карта ушла в сброс.</summary>
        public event Action<Card> CardDiscarded;

        /// <summary>Любое изменение состава стопок (для перерисовки UI целиком).</summary>
        public event Action PilesChanged;

        /// <summary>Если добор пуст — автоматически перетасовать сброс обратно в добор.</summary>
        public bool ReshuffleDiscardWhenEmpty = true;

        /// <summary>Сколько карт максимум держим в руке.</summary>
        public int HandLimit = int.MaxValue;

        public IReadOnlyList<Card> DrawPile { get { return _draw; } }
        public IReadOnlyList<Card> Hand { get { return _hand; } }
        public IReadOnlyList<Card> DiscardPile { get { return _discard; } }

        public int TotalCards { get { return _draw.Count + _hand.Count + _discard.Count; } }

        public Deck(IEnumerable<Card> cards, int seed = 0)
        {
            if (cards == null) throw new ArgumentNullException("cards");

            foreach (Card card in cards)
            {
                if (card == null) throw new ArgumentNullException("cards", "В колоде не может быть null-карт");
                _draw.Add(card);
            }

            _rng = new Random(seed);
        }

        /// <summary>Добавить карту в добор.</summary>
        public void AddToDrawPile(Card card)
        {
            if (card == null) throw new ArgumentNullException("card");
            _draw.Add(card);
            RaiseChanged();
        }

        /// <summary>Перетасовка Фишера–Йетса. Состав стопки не меняется, только порядок.</summary>
        public void Shuffle()
        {
            for (int i = _draw.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                Card tmp = _draw[i];
                _draw[i] = _draw[j];
                _draw[j] = tmp;
            }
            RaiseChanged();
        }

        /// <summary>
        /// Взять одну карту в руку. Возвращает null, если рука заполнена или брать нечего.
        /// </summary>
        public Card DrawToHand()
        {
            if (_hand.Count >= HandLimit) return null;

            if (_draw.Count == 0)
            {
                if (!ReshuffleDiscardWhenEmpty || _discard.Count == 0) return null;
                ReshuffleDiscardIntoDraw();
            }

            if (_draw.Count == 0) return null;

            int last = _draw.Count - 1;
            Card card = _draw[last];
            _draw.RemoveAt(last);
            _hand.Add(card);

            if (CardDrawn != null) CardDrawn(card);
            RaiseChanged();
            return card;
        }

        /// <summary>Добрать до count карт. Возвращает, сколько реально удалось взять.</summary>
        public int DrawToHand(int count)
        {
            int drawn = 0;
            for (int i = 0; i < count; i++)
            {
                if (DrawToHand() == null) break;
                drawn++;
            }
            return drawn;
        }

        /// <summary>Сбросить карту из руки. Возвращает false, если такой карты в руке нет.</summary>
        public bool DiscardFromHand(Card card)
        {
            if (card == null) return false;
            if (!_hand.Remove(card)) return false;

            _discard.Add(card);
            if (CardDiscarded != null) CardDiscarded(card);
            RaiseChanged();
            return true;
        }

        /// <summary>
        /// Взять конкретную карту из сброса в руку (воскрешение/возврат).
        /// Возвращает null, если карты там нет или рука заполнена.
        /// </summary>
        public Card TakeFromDiscard(Card card)
        {
            if (card == null) return null;
            if (_hand.Count >= HandLimit) return null;
            if (!_discard.Remove(card)) return null;

            _hand.Add(card);
            if (CardDrawn != null) CardDrawn(card);
            RaiseChanged();
            return card;
        }

        /// <summary>Сбросить карту, которой нет в руке (разыгранная, уничтоженная и т.п.).</summary>
        public void Discard(Card card)
        {
            if (card == null) throw new ArgumentNullException("card");
            _discard.Add(card);
            if (CardDiscarded != null) CardDiscarded(card);
            RaiseChanged();
        }

        /// <summary>Перемешать сброс обратно в добор.</summary>
        public void ReshuffleDiscardIntoDraw()
        {
            if (_discard.Count == 0) return;
            _draw.AddRange(_discard);
            _discard.Clear();
            Shuffle();
        }

        private void RaiseChanged()
        {
            if (PilesChanged != null) PilesChanged();
        }
    }
}
