using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Nekres.Notes.UI.Controls
{
    internal class OnlineBook : ReadOnlyBook
    {
        public string Author { get; private set; }

        public Guid AuthorId { get; private set; }

        public int PositiveRatings { get; private set; }

        public int NegativeRatings { get; private set; }

        private Rectangle _positiveRateButtonBounds;
        private bool _mouseOverPositiveRate;
        private Rectangle _negativeRateButtonBounds;
        private bool _mouseOverNegativeRate;

        public OnlineBook(Guid id, string author, Guid authorId, int positiveRatings, int negativeRatings, string title, IEnumerable<(string, string)> pages) : base(id, title, pages)
        {
            Author = author;
            AuthorId = authorId;
            PositiveRatings = positiveRatings;
            NegativeRatings = negativeRatings;
        }

        public OnlineBook(Guid id, string author, Guid authorId, int positiveRatings, int negativeRatings, string title, IEnumerable<string> contentPages) : base(id, title, contentPages)
        {
            Author = author;
            AuthorId = authorId;
            PositiveRatings = positiveRatings;
            NegativeRatings = negativeRatings;
        }

        public OnlineBook(Guid id, string author, Guid authorId, int positiveRatings, int negativeRatings, string title, string content) : base(id, title, content)
        {
            Author = author;
            AuthorId = authorId;
            PositiveRatings = positiveRatings;
            NegativeRatings = negativeRatings;
        }

        public OnlineBook(Guid id, string author, Guid authorId, int positiveRatings, int negativeRatings, string title) : base(id, title)
        {
            Author = author;
            AuthorId = authorId;
            PositiveRatings = positiveRatings;
            NegativeRatings = negativeRatings;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            var relPos = RelativeMousePosition;
            _mouseOverPositiveRate = _positiveRateButtonBounds.Contains(relPos);
            _mouseOverNegativeRate = _negativeRateButtonBounds.Contains(relPos);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
        }

        private void Rate(bool rating)
        {
            // TODO: PUT rate request
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            // TODO: Draw Author, draw ratings and rate buttons
        }
    }
}
