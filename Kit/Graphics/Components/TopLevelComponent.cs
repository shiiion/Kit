using System;
using System.Collections.Generic;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;

namespace Kit.Graphics.Components
{
    public class TopLevelComponent : KitComponent
    {
        private List<KitComponent> drawOrder;

        public TopLevelComponent(KitWindow owner)
            : base(Vector2.Zero, new Vector2(owner.Width, owner.Height))
        {
            drawOrder = new List<KitComponent>();
        }

        //POSSIBLE RE-IMPLEMENTATION: use IComparable to sort list
        public void PreDrawComponentTree(KitBrush brush)
        {
            drawOrder.Clear();
            foreach(KitComponent child in Children)
            {
                child.PreDrawComponent(drawOrder, brush);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        public void DrawComponentTree(KitBrush brush)
        {

            foreach(KitComponent component in drawOrder)
            {
                component._DrawComponent(brush);
            }
            redraw = false;
#if DEBUG
            if (debugKey_Pressed)
            {
                DrawComponentDebugInfo(brush);
            }
#endif
        }
    }
}
