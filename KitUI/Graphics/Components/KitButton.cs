using System.Windows.Media;
using Kit.Core;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using Kit.Core.Delegates;
using System;

namespace Kit.Graphics.Components
{
    public class KitButton : KitComponent
    {
        public KitComponent VisualComponentPressed { get; set; }
        public KitComponent VisualComponentReleased { get; set; }

        private KitComponent referencedVC;

        private bool isPressed;
        public bool IsPressed
        {
            get
            {
                return isPressed;
            }
        }

        public event VoidDelegate Pressed;
        public event VoidDelegate Released;
        private Vector2 sizeSave;

        public KitButton(string releasedImage, string pressedImage, Vector2 size)
        {
            Size = size;
            sizeSave = size;
            isPressed = false;
            VisualComponentPressed = new KitImage(pressedImage)
            {
                Anchor = KitAnchoring.Center,
                Origin = KitAnchoring.Center,
                ShouldDraw = false,
                Size = sizeSave,
            };
            VisualComponentReleased = new KitImage(releasedImage)
            {
                Anchor = KitAnchoring.Center,
                Origin = KitAnchoring.Center,
                ShouldDraw = false,
                Size = sizeSave,
            };
            AddChild(VisualComponentReleased);
            AddChild(VisualComponentPressed);
            Redraw = false;
            referencedVC = VisualComponentReleased;
        }
        
        public KitButton(string buttonText, string buttonFont, double ptSize, Color textColor, Color pressedColor, Color releasedColor, Vector2 borders, double edgeRound)
        {
            VisualComponentPressed = new KitText(buttonText, buttonFont, ptSize)
            {
                BackgroundEnabled = true,
                TextColor = textColor,
                ShouldDraw = false,
                Anchor = KitAnchoring.Center,
                Origin = KitAnchoring.Center
            };
            ((KitText)VisualComponentPressed).TextBackground.BoxColor = pressedColor;
            ((KitText)VisualComponentPressed).TextBackground.Anchor = KitAnchoring.Center;
            ((KitText)VisualComponentPressed).TextBackground.Origin = KitAnchoring.Center;
            ((KitText)VisualComponentPressed).TextBackground.EdgeRounding = edgeRound;
            ((KitText)VisualComponentPressed).TextBackground.Size = VisualComponentPressed.Size + borders;

            VisualComponentReleased = new KitText(buttonText, buttonFont, ptSize)
            {
                BackgroundEnabled = true,
                TextColor = textColor,
                ShouldDraw = false,
                Anchor = KitAnchoring.Center,
                Origin = KitAnchoring.Center
            };
            ((KitText)VisualComponentReleased).TextBackground.BoxColor = releasedColor;
            ((KitText)VisualComponentReleased).TextBackground.Anchor = KitAnchoring.Center;
            ((KitText)VisualComponentReleased).TextBackground.Origin = KitAnchoring.Center;
            ((KitText)VisualComponentReleased).TextBackground.EdgeRounding = edgeRound;
            ((KitText)VisualComponentReleased).TextBackground.Size = VisualComponentReleased.Size + borders;

            Size = VisualComponentPressed.Size + borders;

            AddChild(VisualComponentReleased);
            AddChild(VisualComponentPressed);
            referencedVC = VisualComponentReleased;
        }

        protected override void OnMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            if ((mouseFlags & MouseState.Left) == MouseState.Left)
            {
                if ((mouseFlags & MouseState.Down) == MouseState.Down && !isPressed && Contains(clickLocation))
                {
                    isPressed = true;
                    referencedVC = VisualComponentPressed;
                    Pressed?.Invoke();
                    Redraw = true;
                }
                else if ((mouseFlags & MouseState.Down) == 0 && isPressed)
                {
                    isPressed = false;
                    referencedVC = VisualComponentReleased;
                    if (Contains(clickLocation))
                    {
                        Released?.Invoke();
                    }
                    Redraw = true;
                }
            }
            base.OnMouseInput(clickLocation, mouseFlags);
        }

        public override void PreDrawComponent(KitBrush brush)
        {
            VisualComponentReleased.Opacity = Opacity;
            VisualComponentPressed.Opacity = Opacity;
            base.PreDrawComponent(brush);
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if (isPressed && !Contains(end) && referencedVC == VisualComponentPressed)
            {
                referencedVC = VisualComponentReleased;
                Redraw = true;
            }
            else if (isPressed && Contains(end) && referencedVC == VisualComponentReleased)
            {
                referencedVC = VisualComponentPressed;
                Redraw = true;
            }
            return base.OnMouseMove(state, start, end);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            referencedVC._DrawComponent(brush);
            base.DrawComponent(brush);
        }
    }
}
