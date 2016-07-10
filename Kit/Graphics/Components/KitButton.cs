using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Core;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using Kit.Core.Delegates;

namespace Kit.Graphics.Components
{
    public class KitButton : KitImage
    {
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

        public string PressedImage { get; set; }
        public string ReleasedImage { get; set; }
        private Vector2 sizeSave;

        public KitButton(string releasedImage, string pressedImage, Vector2 size)
            : base(releasedImage)
        {
            PressedImage = pressedImage;
            ReleasedImage = releasedImage;
            Size = size;
            sizeSave = size;
            isPressed = false;
        }

        protected override void OnMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            if ((mouseFlags & MouseState.Left) == MouseState.Left)
            {
                if ((mouseFlags & MouseState.Down) == MouseState.Down && !isPressed && Contains(clickLocation))
                {
                    isPressed = true;
                    ImagePath = PressedImage;
                    Size = sizeSave;
                    Pressed?.Invoke();
                }
                else if ((mouseFlags & MouseState.Down) == 0 && isPressed)
                {
                    isPressed = false;
                    ImagePath = ReleasedImage;
                    Size = sizeSave;
                    if (Contains(clickLocation))
                    {
                        Released?.Invoke();
                    }
                }
            }
            base.OnMouseInput(clickLocation, mouseFlags);
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if (isPressed && !Contains(end))
            {
                ImagePath = ReleasedImage;
                Size = sizeSave;
            }
            else if (isPressed && Contains(end) && ImagePath.Equals(ReleasedImage))
            {
                ImagePath = PressedImage;
                Size = sizeSave;
            }
            return base.OnMouseMove(state, start, end);
        }
    }
}
