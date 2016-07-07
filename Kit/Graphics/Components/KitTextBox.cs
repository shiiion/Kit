using System.Collections.Generic;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    class KitTextBox : KitComponent
    {
        private double lastFlashTime;
        private bool dashOn;

        private int cursorLoc;

        private static readonly int CURSORLOC_END = -1;

        public KitText TextField { get; set; } 

        public KitTextBox(double fontSize, double maxWidth, Vector2 location = default(Vector2))
            : base(location)
        {
            Anchor = KitAnchoring.LeftCenter;
            TextField = new KitText("TextField", "Veranda", fontSize, Vector2.Zero, Size)
            {
                Origin = KitAnchoring.LeftCenter,
                TextColor = System.Windows.Media.Colors.Black
            };
            
            AddChild(TextField);
            Masked = true;
            Vector2 TextMetrics = KitBrush.GetTextBounds("|", TextField.Font);
            Size = new Vector2(maxWidth, TextMetrics.Y + 4);
            lastFlashTime = time;
            cursorLoc = 4;
        }
        

        public override void PreDrawComponent(KitBrush brush)
        {
            foreach(KitComponent child in Children)
            {
                child.ComponentDepth = ComponentDepth + 0.01;
            }
            base.PreDrawComponent(brush);
        }

        protected override void OnUpdate()
        {
            if(!Focused && !TextField.Focused)
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

                if (time - lastFlashTime > 800)
                {
                    lastFlashTime = time;
                    dashOn = !dashOn;
                    Redraw = true;
                }
            }
            base.OnUpdate();
        }

        protected override void DrawComponent(KitBrush brush)
        {
            Vector2 lineStart = GetAbsoluteLocation();
            Vector2 lineEnd = new Vector2(lineStart.X, lineStart.Y + Size.Y);
            if(TextField.Text.Length != 0)
            {
                if(cursorLoc == CURSORLOC_END)
                {
                    Vector2 textDims = KitBrush.GetTextBounds(TextField.Text, TextField.Font);
                    lineStart.X += textDims.X;
                    lineEnd.X += textDims.X;
                }
                else if(cursorLoc > 0)
                {
                    Vector2 textDims = KitBrush.GetTextBounds(TextField.Text.Substring(0, cursorLoc), TextField.Font);
                    lineStart.X += textDims.X;
                    lineEnd.X += textDims.X;
                }
            }
            if (dashOn && (Focused || TextField.Focused))
            {
                lineStart.X = System.Math.Ceiling(lineStart.X) + 0.5;
                lineEnd.X = System.Math.Ceiling(lineEnd.X) + 0.5;
                brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
                brush.DrawLine(lineStart, lineEnd, TextField.TextColor, 1);
            }
            base.DrawComponent(brush);
        }
    }
}