using System;
using System.Windows.Media;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    class KitBox : KitComponent
    {
        public Color BoxColor { get; set; }

        public double EdgeRounding { get; set; }

        public bool Filled { get; set; }

        public KitBox(Color boxColor, Vector2 size)
            : base(new Vector2(), size)
        {
            BoxColor = boxColor;
            Filled = true;
            EdgeRounding = 0;
        }

        protected override void DrawComponent(KitBrush brush)
        {
            if(EdgeRounding > 0)
            {
                brush.DrawRoundedRectangle(new Box(GetAbsoluteLocation(), Size), Filled, BoxColor, EdgeRounding, EdgeRounding);
            }
            else
            {
                brush.DrawRectangle(new Box(GetAbsoluteLocation(), Size), Filled, BoxColor);
            }
            base.DrawComponent(brush);
        }
    }
}
