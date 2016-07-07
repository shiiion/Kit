using System.Windows.Input;
using System.Collections.Generic;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;
using Kit.Core;

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

        private void orderByDepth(List<KitComponent> list)
        {
            list.Clear();
            foreach (KitComponent child in Children)
            {
                child.OrderByDepth(list);
            }
        }

        //POSSIBLE RE-IMPLEMENTATION: use IComparable to sort list
        public void PreDrawComponentTree(KitBrush brush)
        {
            foreach (KitComponent child in Children)
            {
                child.PreDrawComponent(brush);
            }
            orderByDepth(drawOrder);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        public void NotifyMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyMouseInput(clickLocation, mouseFlags);
            }

            List<KitComponent> depthOrder = new List<KitComponent>();
            orderByDepth(depthOrder);

            for (int i = 0; i < depthOrder.Count; i++)
            {
                if (!depthOrder[i].Focused)
                {
                    depthOrder.Remove(depthOrder[i]);
                    i--;
                }
            }

            for (int i = 0; i < depthOrder.Count - 1; i++)
            {
                depthOrder[i].Focused = false;
            }
        }

        public void DrawComponentTree(KitBrush brush)
        {

            foreach (KitComponent component in drawOrder)
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
