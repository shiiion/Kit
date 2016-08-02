using System.Windows.Input;
using System.Collections.Generic;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;
using Kit.Core;
using Kit.Core.Delegates;
using System;

namespace Kit.Graphics.Components
{
    public class TopLevelComponent : KitComponent
    {
        private List<KitComponent> drawOrder;

        private KitTitleBar titleBar;

        public Vector2 WindowLocation { get; set; }

        public TopLevelComponent(KitWindow owner, string title, VoidDelegate onClose)
            : base(Vector2.Zero, new Vector2(owner.Width, owner.Height))
        {
            drawOrder = new List<KitComponent>();
            titleBar = new KitTitleBar(System.Windows.Media.Color.FromArgb(220, 127, 127, 127), this, onClose, title);
            AddChild(titleBar);
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
            debugKey_Pressed = false;
            foreach (KitComponent child in Children)
            {
                child.PreDrawComponent(brush);
                debugKey_Pressed |= child.debugKey_Pressed;
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
            if ((mouseFlags & MouseState.Down) == MouseState.Down)
            {
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
        }

        public void NotifyKeyInput(Key key, KeyState state)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyKeyInput(key, state);
            }
        }

        public void NotifyTextInput(string text)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyTextInput(text);
            }
        }

        public bool NotifyMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            bool ret = false;
            foreach (KitComponent child in Children)
            {
                ret |= child._NotifyMouseMove(state, start, end);
            }
            return ret;
        }

        public void NotifyScroll(Vector2 mouseLoc, int direction)
        {
            foreach(KitComponent child in Children)
            {
                child._NotifyScroll(mouseLoc, direction);
            }
        }

        public void DrawComponentTree(KitBrush brush, bool drawBG)
        {
            if (drawBG)
            {
                brush.DrawRectangle(new Box(0, 0, Size), true, System.Windows.Media.Color.FromArgb(0x01, 0x7f, 0x7f, 0x7f));
            }
            foreach (KitComponent component in drawOrder)
            {
                if (component.ShouldDraw)
                {
                    brush.PushOpacity(component.Opacity);
                    component._DrawComponent(brush);
                    brush.Pop();
                }
            }
            redraw = false;

            brush.PushOpacity(titleBar.Opacity);
            titleBar._DrawComponent(brush);
            brush.Pop();

            foreach (KitComponent component in titleBar.Children)
            {
                brush.PushOpacity(component.Opacity);
                component._DrawComponent(brush);
                brush.Pop();
            }

#if DEBUG
            if (debugKey_Pressed)
            {
                DrawComponentDebugInfo(brush);
            }
#endif
        }
    }
}
