using System.Collections.Generic;
using System.Windows.Input;
using Kit.Core;
using Kit.Graphics.Drawing;
using System.Windows.Media;
using Kit.Graphics.Types;
using Kit.Core.Delegates;

namespace Kit.Graphics.Components
{
    class KitTextBox : KitComponent
    {
        private double lastFlashTime;
        protected bool dashOn;

        public KitText TextField { get; set; }

        public Color BackColor { get; set; }

        public Color HighlightColor { get; set; }

        protected Vector2 boxSize;

        protected TextIOFormatter formatter;

        public event StringDelegate TextChanged;

        public KitTextBox(string font, double fontSize, double maxWidth, Vector2 location = default(Vector2))
            : base(location)
        {
            TextField = new KitText("", font, fontSize)
            {
                Anchor = KitAnchoring.LeftCenter,
                Origin = KitAnchoring.LeftCenter,
                TextColor = Colors.Black,
                ShouldDraw = false,
                ComponentDepth = double.MinValue,
                Size = Size,
                Masked = true
            };
            formatter = new TextIOFormatter(TextField);

            AddChild(TextField);
            Masked = true;
            Vector2 TextMetrics = KitBrush.GetTextBounds("|", TextField.Font);
            Size = new Vector2(maxWidth, TextMetrics.Y + 4);
            boxSize = Size;
            lastFlashTime = time;
            BackColor = Color.FromArgb(0x7f, 0xff, 0xff, 0xff);
            HighlightColor = Color.FromArgb(0x7f, 0, 0, 0xff);
            RoundedMask = true;
            RoundingRadius = 5;

            Resize += handleBoxSizeChanged;
        }

        public override void PreDrawComponent(KitBrush brush)
        {
            foreach (KitComponent child in Children)
            {
                child.ComponentDepth = ComponentDepth + 0.01;
            }
            base.PreDrawComponent(brush);
        }

        protected override void OnUpdate()
        {
            if (!Focused && !TextField.Focused)
            {
                lastFlashTime = -1;
            }
            else
            {
                if (lastFlashTime == -1)
                {
                    dashOn = true;
                    lastFlashTime = time;
                    Redraw = true;
                }

                if (time - lastFlashTime > 600)
                {
                    lastFlashTime = time;
                    dashOn = !dashOn;
                    Redraw = true;
                }
            }
            base.OnUpdate();
        }

        protected void forceRedrawCursor()
        {
            lastFlashTime = time;
            dashOn = true;
            Redraw = true;
        }

        protected override void OnTextInput(string text)
        {
            if ((Focused || TextField.Focused)
                && text.Length > 0 && text[0] >= ' ')
            {
                formatter.InsertText(text);
                forceRedrawCursor();
                TextChanged?.Invoke(TextField.Text);
            }
            base.OnTextInput(text);
        }

        protected override void OnKeyInput(Key key, KeyState state)
        {
            if (Focused || TextField.Focused)
            {
                formatter.OnKey(key, state);
                if (state == KeyState.Press || state == KeyState.Hold)
                {
                    if (formatter.HandleKeyPress(key))
                    {
                        forceRedrawCursor();
                    }
                    if(key == Key.Delete || key == Key.Back)
                    {
                        TextChanged?.Invoke(TextField.Text);
                    }
                }
            }
            base.OnKeyInput(key, state);
        }

        protected override void OnMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            if ((Focused || TextField.Focused) && mouseFlags == MouseState.LeftDown)
            {
                Vector2 relativeClick = clickLocation - TextField.GetAbsoluteLocation();
                formatter.InsertCursorAt(relativeClick);
                forceRedrawCursor();
                formatter.EndHighlight();
            }
            else if ((mouseFlags & MouseState.Down) == MouseState.Down)
            {
                forceRedrawCursor();
            }
            base.OnMouseInput(clickLocation, mouseFlags);
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if ((Focused || TextField.Focused) && state == (MouseState.Left | MouseState.Down))
            {
                if (!formatter.HighlightEnabled())
                {
                    formatter.BeginHighlight();
                    forceRedrawCursor();
                }
                if (formatter.InsertHighlightEndAt(end - TextField.GetAbsoluteLocation()))
                {
                    forceRedrawCursor();
                }
            }
            return false;
        }

        protected virtual void DrawCursor(KitBrush brush, Vector2 absLoc)
        {
            Vector2 lineStart = absLoc;
            Vector2 lineEnd = new Vector2(lineStart.X, lineStart.Y + boxSize.Y);

            double pixelCursorOffset = formatter.GetCursorOffset();

            lineStart.X += pixelCursorOffset + TextField.Location.X;
            lineEnd.X += pixelCursorOffset + TextField.Location.X;
            lineStart.Y += 1;
            lineEnd.Y -= 1;

            lineStart.X = System.Math.Round(lineStart.X) + 0.5;
            lineEnd.X = System.Math.Round(lineEnd.X) + 0.5;
            brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
            brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
        }

        protected virtual void DrawHighlighting(KitBrush brush)
        {
            Box highlightRect = formatter.GetHighlightRect(TextField.Text);
            highlightRect.Pos += TextField.GetAbsoluteLocation();
            brush.DrawRectangle(highlightRect, true, HighlightColor);
        }

        protected virtual void SetContentLocation()
        {
            TextField.Location = formatter.GetVisibleOffset(TextField.Text, boxSize.X, formatter.CursorLoc);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            Vector2 absLoc = GetAbsoluteLocation();

            brush.DrawRoundedRectangle(new Box(absLoc, boxSize), true, BackColor, 5, 5);

            SetContentLocation();

            pushNecessaryClips(brush);

            if (formatter.Highlighting())
            {
                DrawHighlighting(brush);
            }

            if (dashOn && (Focused || TextField.Focused))
            {
                DrawCursor(brush, absLoc);
            }

            popNecessaryClips(brush);

            TextField._DrawComponent(brush);
            base.DrawComponent(brush);
        }

        protected void handleBoxSizeChanged()
        {
            boxSize = Size;
        }
    }
}