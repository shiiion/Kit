using System.Collections.Generic;
using System.Windows.Input;
using Kit.Core;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    class KitTextBox : KitComponent
    {
        private double lastFlashTime;
        private bool dashOn;

        public KitText TextField { get; set; }

        private TextIOFormatter formatter;

        public KitTextBox(double fontSize, double maxWidth, Vector2 location = default(Vector2))
            : base(location)
        {
            TextField = new KitText("TextField", "Consolas", fontSize, Vector2.Zero, Size)
            {
                Anchor = KitAnchoring.LeftCenter,
                Origin = KitAnchoring.LeftCenter,
                TextColor = System.Windows.Media.Colors.Black,
                ShouldDraw = false
            };
            formatter = new TextIOFormatter(TextField);

            AddChild(TextField);
            Masked = true;
            Vector2 TextMetrics = KitBrush.GetTextBounds("|", TextField.Font);
            Size = new Vector2(maxWidth, TextMetrics.Y + 4);
            lastFlashTime = time;
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

        protected override void OnTextInput(string text)
        {
            if ((Focused || TextField.Focused)
                && text.Length > 0 && text[0] >= ' ')
            {
                formatter.InsertText(text);
                lastFlashTime = time;
                dashOn = true;
                Redraw = true;
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
                        lastFlashTime = time;
                        dashOn = true;
                        Redraw = true;
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
                dashOn = true;
                Redraw = true;
                lastFlashTime = time;
                formatter.EndHighlight();
            }
            else if ((mouseFlags & MouseState.Down) == MouseState.Down)
            {
                dashOn = false;
                lastFlashTime = time;
                Redraw = true;
            }
            base.OnMouseInput(clickLocation, mouseFlags);
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if ((Focused || TextField.Focused) && state == MouseState.Left)
            {
                if(!formatter.HighlightEnabled())
                {
                    formatter.BeginHighlight();
                    Redraw = true;
                    dashOn = false;
                    lastFlashTime = time;
                }
                if(formatter.InsertHighlightEndAt(end - TextField.GetAbsoluteLocation()))
                {
                    Redraw = true;
                    dashOn = false;
                    lastFlashTime = time;
                }
            }
            return false;
        }

        protected override void DrawComponent(KitBrush brush)
        {
            Vector2 lineStart = GetAbsoluteLocation();
            Vector2 lineEnd = new Vector2(lineStart.X, lineStart.Y + Size.Y);
            System.Windows.Media.Color nc = System.Windows.Media.Color.FromArgb(0x7F, 0xFF, 0, 0);
            brush.DrawRoundedRectangle(new Box(new Vector2(lineStart.X - 2, lineStart.Y), new Vector2(Size.X + 4, Size.Y)), true, nc, 5, 5);

            double pixelCursorOffset = formatter.GetCursorOffset();

            TextField.Location = formatter.GetVisibleOffset(Size.X);

            lineStart.X += pixelCursorOffset + TextField.Location.X;
            lineEnd.X += pixelCursorOffset + TextField.Location.X;
            lineStart.Y += 1;
            lineEnd.Y -= 1;

            pushNecessaryClips(brush);

            if (formatter.Highlighting())
            {
                System.Windows.Media.Color hColor = System.Windows.Media.Color.FromArgb(0x7F, 0, 0, 0xFF);
                Box highlightRect = formatter.GetHighlightRect();
                highlightRect.Pos += TextField.GetAbsoluteLocation();
                brush.DrawRectangle(highlightRect, true, hColor);
            }

            if (dashOn && (Focused || TextField.Focused))
            {
                lineStart.X = System.Math.Round(lineStart.X) + 0.5;
                lineEnd.X = System.Math.Round(lineEnd.X) + 0.5;
                brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
                brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
            }
            popNecessaryClips(brush);


            TextField._DrawComponent(brush);
            base.DrawComponent(brush);
        }
    }
}