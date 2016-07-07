using System.Collections.Generic;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    class KitTextBox : KitComponent
    {
        public KitText TextField { get; set; } 

        public KitTextBox(double fontSize, Vector2 location = default(Vector2), Vector2 size = default(Vector2))
            : base(location, size)
        {
            Anchor = KitAnchoring.TopLeft;
            TextField = new KitText("", "Veranda", fontSize, Vector2.Zero, Size);

            AddChild(TextField);
        }
        

        public override void PreDrawComponent(List<KitComponent> drawList, KitBrush brush)
        {
            foreach(KitComponent child in Children)
            {
                child.ComponentDepth = ComponentDepth + 0.01;
            }
            base.PreDrawComponent(drawList, brush);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            base.DrawComponent(brush);
        }
    }
}