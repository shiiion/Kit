using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Types;
using Kit.Core;
using Kit.Graphics.Drawing;

namespace Kit.Graphics.Components
{
    class KitTextArea : KitTextBox, IScrollable
    {
        private KitScrollbar scrollbar;
        private MultilineIOFormatter mlFormatter;

        public KitTextArea(string font, double fontSize, Vector2 size, Vector2 location = default(Vector2))
            : base(font, fontSize, size.X, location)
        {
            Size = size;
            TextField.Anchor = KitAnchoring.TopLeft;
            TextField.Origin = KitAnchoring.TopLeft;
            scrollbar = null;
            formatter = mlFormatter = new MultilineIOFormatter(TextField);
        }

        public bool ContentLargerThanArea()
        {
            return TextField.Size.Y > Size.Y;
        }

        public Vector2 ContentDimensions()
        {
            return TextField.Size;
        }

        public void SetContentLocation(Vector2 location)
        {
            TextField.Location = location;
        }

        public void SetScrollbar(KitScrollbar scrollbar)
        {
            this.scrollbar = scrollbar;
        }

        protected override void OnTextInput(string text)
        {
            if((Focused || TextField.Focused) && text.Length > 0 && text[0] == '\r')
            {
                formatter.InsertText("\n");
                forceRedrawCursor();
            }
            if ((Focused || TextField.Focused) && text.Length > 0 && text[0] == '\t')
            {
                formatter.InsertText("\t");
                forceRedrawCursor();
            }
            base.OnTextInput(text);
        }

        protected override void DrawCursor(KitBrush brush, Vector2 absLoc)
        {
            Vector2 lineStart = absLoc;
            Vector2 lineEnd = new Vector2(lineStart.X, lineStart.Y + KitBrush.GetTextBounds("|", TextField.Font).Y);

            Vector2 pixelCursorOffset = mlFormatter.GetCursorOffset();

            lineStart.AddThis(pixelCursorOffset + TextField.Location);
            lineEnd.AddThis(pixelCursorOffset + TextField.Location);
            lineStart.Y += 1;
            lineEnd.Y -= 1;

            lineStart.X = Math.Round(lineStart.X) + 0.5;
            lineEnd.X = Math.Round(lineEnd.X) + 0.5;
            brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
            brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
        }

        protected override void DrawHighlighting(KitBrush brush)
        {
            Box[] highlightRects = mlFormatter.GetHighlightRects(TextField.Text);
            for (int a = 0; a < highlightRects.Length; a++)
            {
                highlightRects[a].Pos += TextField.GetAbsoluteLocation();
                brush.DrawRectangle(highlightRects[a], true, HighlightColor);
            }
        }

        protected override void SetContentLocation()
        {
            if (scrollbar == null)
            {
                TextField.Location = mlFormatter.GetVisibleOffset(Size.X, Size.Y, mlFormatter.CursorLoc);
            }
            else
            {
                //TODO
            }
        }
    }
}
