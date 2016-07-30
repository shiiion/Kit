using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Types;
using Kit.Core;
using Kit.Graphics.Drawing;
using System.Windows.Input;

namespace Kit.Graphics.Components
{
    class KitTextArea : KitTextBox, IScrollable
    {
        private KitScrollbar scrollbar;
        private MultilineIOFormatter mlFormatter;
        private double textHeight;

        public KitTextArea(string font, double fontSize, Vector2 size, Vector2 location = default(Vector2))
            : base(font, fontSize, size.X, location)
        {
            Size = size;
            TextField.Anchor = KitAnchoring.TopLeft;
            TextField.Origin = KitAnchoring.TopLeft;
            scrollbar = null;
            formatter = mlFormatter = new MultilineIOFormatter(TextField);
            textHeight = KitBrush.GetTextBounds("|", TextField.Font).Y;
        }

        public bool ContentLargerThanArea()
        {
            if (TextField.Text.Length > 0 && TextField.Text.Last() == '\n')
            {
                return TextField.Size.Y + textHeight > Size.Y;
            }
            return TextField.Size.Y > Size.Y;
        }

        public Vector2 ContentDimensions()
        {
            if (TextField.Text.Length > 0 && TextField.Text.Last() == '\n')
            {
                return new Vector2(TextField.Size.X, TextField.Size.Y + textHeight);
            }
            return TextField.Size;
        }

        public void SetScrollbar(KitScrollbar scrollbar)
        {
            this.scrollbar = scrollbar;
            scrollbar.RegisterScrollbar(this);
            Size = new Vector2(Size.X - 16, Size.Y);
            scrollbar.ScrollStep = textHeight;
        }

        private void trackCursor()
        {
            double Y = mlFormatter.GetCursorOffset().Y;
            if (Y + textHeight > Size.Y + (scrollbar.ScrollStep * getScrollOffset()))
            {
                scrollbar.SetScrollLocation((Y - Size.Y + textHeight) / scrollbar.ScrollStep + (scrollbar.ScrollLocation / (TextField.Size.Y / scrollbar.ScrollStep)));
            }
            else if(Y < (scrollbar.ScrollStep * getScrollOffset()))
            {
                scrollbar.SetScrollLocation(Y / scrollbar.ScrollStep + (scrollbar.ScrollLocation / (TextField.Size.Y / scrollbar.ScrollStep)));
            }
        }

        public bool ContainsCursor(Vector2 cursorLoc)
        {
            return Contains(cursorLoc);
        }

        protected override void OnTextInput(string text)
        { 
            if ((Focused || TextField.Focused) && text.Length > 0 && text[0] == '\r')
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

            if (scrollbar != null)
            {
                scrollbar.OnContentSizeChange();
                if (scrollbar.Enabled)
                {
                    trackCursor();
                    trackCursor();
                }
            }
        }

        protected override void OnKeyInput(Key key, KeyState state)
        {
            base.OnKeyInput(key, state);
            if (scrollbar != null)
            {
                scrollbar.OnContentSizeChange();
                if (scrollbar.Enabled && (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right))
                {
                    //HACK: run twice to scroll exactly to cursor
                    trackCursor();
                    trackCursor();
                }
            }
        }

        protected override void DrawCursor(KitBrush brush, Vector2 absLoc)
        {
            Vector2 lineStart = absLoc;
            Vector2 lineEnd = new Vector2(lineStart.X, lineStart.Y + textHeight);

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

        private double getScrollOffset() { return (scrollbar.ScrollLocation - (scrollbar.ScrollLocation / (TextField.Size.Y / scrollbar.ScrollStep))); }

        protected override void SetContentLocation()
        {
            if (scrollbar == null)
            {
                TextField.Location = mlFormatter.GetVisibleOffset(Size.X, Size.Y, mlFormatter.CursorLoc);
            }
            else
            {
                if (scrollbar.Enabled)
                {
                    double x = mlFormatter.GetVisibleOffset(Size.X, Size.Y, mlFormatter.CursorLoc).X;
                    double y = -getScrollOffset() * scrollbar.ScrollStep;
                    TextField.Location = new Vector2(x, y);
                }
                else
                {
                    double x = mlFormatter.GetVisibleOffset(Size.X, Size.Y, mlFormatter.CursorLoc).X;
                    TextField.Location = new Vector2(x, 0);
                }
            }
        }
    }
}
