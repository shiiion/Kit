using System.Windows.Media;
using Kit.Core.Delegates;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using System.Collections.Generic;

namespace Kit.Graphics.Components
{

    public partial class KitComponent : IContainer<KitComponent>
    {
        public event VoidDelegate Draw;

        private double componentDepth;
        public double ComponentDepth
        {
            get
            {
                return componentDepth;
            }

            set
            {
                componentDepth = value;
                DepthChanged?.Invoke();
            }
        }

        private bool shouldDraw;
        public bool ShouldDraw
        {
            get
            {
                return shouldDraw;
            }

            set
            {
                if(!value)
                {
                    Redraw = false;
                }
                shouldDraw = value;
            }
        }

        public double Opacity { get; set; }

        private bool masked;
        public bool Masked
        {
            get { return masked; }
            set
            {
                masked = value;
                Redraw = true;
            }
        }

        protected bool redraw;
        public bool Redraw
        {
            get
            {
                bool ret;
                ret = redraw;
                foreach (KitComponent child in Children)
                {
                    ret |= (child.Redraw);
                    if (ret)
                    {
                        break;
                    }
                }
                return ret;
            }

            set
            {
                redraw = value;
            }
        }

        public event VoidDelegate DepthChanged;

        public bool UseCustomMask { get; set; }
        public Vector2 CustomMask { get; set; }

        public bool RoundedMask { get; set; }
        public double RoundingRadius { get; set; }

        protected virtual void OnDraw()
        {
        }

        public void OrderByDepth(List<KitComponent> outList)
        {
            if (outList.Count == 0)
            {
                outList.Add(this);
            }
            else
            {
                bool beforeEnd = false;
                for (int i = 0; i < outList.Count; i++)
                {
                    if (outList[i].ComponentDepth > ComponentDepth)
                    {
                        outList.Insert(i, this);
                        beforeEnd = true;
                        break;
                    }
                }
                if (!beforeEnd)
                {
                    outList.Add(this);
                }
            }

            foreach (KitComponent child in Children)
            {
                child.OrderByDepth(outList);
            }
        }

        public virtual void PreDrawComponent(KitBrush brush)
        {
            foreach (KitComponent child in Children)
            {
                child.PreDrawComponent(brush);
            }
        }

        public void _DrawComponent(KitBrush brush)
        {
            lock (ComponentLock)
            {
                DrawComponent(brush);
                Draw?.Invoke();
                OnDraw();
            }
        }

        private KitComponent getFrontComponentFromBranch(Vector2 location)
        {
            if (Children.Count == 0)
            {
                if (Contains(location))
                {
                    return this;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                KitComponent front = null;
                foreach (KitComponent child in Children)
                {
                    front = child.getFrontComponentFromBranch(location);
                }
                if (front == null)
                {
                    if (Contains(location))
                    {
                        return this;
                    }
                    else
                    {
                        return null;
                    }
                }
                if (!Contains(location))
                {
                    return front;
                }
                //if child depth is equal to this depth, return child component
                if (front.ComponentDepth >= ComponentDepth)
                {
                    return front;
                }
                return this;
            }
        }

        public KitComponent FrontmostComponentAt(Vector2 location)
        {
            if (parent == null)
            {
                KitComponent frontmostChild = null;
                foreach (KitComponent child in Children)
                {
                    frontmostChild = child.getFrontComponentFromBranch(location);
                }
                return frontmostChild;
            }
            else
            {
                return parent.FrontmostComponentAt(location);
            }
        }

#if DEBUG

        KitFont debugFont = new KitFont()
        {
            NormalFont = new Typeface("Consolas"),
            FontSize = 8
        };

        public void DrawComponentDebugInfo(KitBrush brush)
        {
            Vector2 pos = GetAbsoluteLocation();
            brush.SetLineThickness(1);
            brush.DrawRectangle(new Box(pos, Size.X, Size.Y), false, Colors.Red);
            brush.DrawLine(pos + new Vector2(Size.X / 2.0, 0), pos + new Vector2(Size.X / 2.0, Size.Y), Colors.Red, 1);
            brush.DrawLine(pos + new Vector2(0, Size.Y / 2.0), pos + new Vector2(Size.X, Size.Y / 2.0), Colors.Red, 1);
            if (Focused)
            {
                brush.DrawString(GetType().Name, debugFont, pos, Colors.Red);
            }
            foreach (KitComponent child in Children)
            {
                child.DrawComponentDebugInfo(brush);
            }
        }
#endif
        protected virtual void DrawComponent(KitBrush brush)
        {
            redraw = false;
        }


        protected void pushNecessaryClips(KitBrush brush)
        {
            if (Masked)
            {
                Vector2 loc = GetAbsoluteLocation();
                if (UseCustomMask)
                {
                    brush.PushClip(loc, CustomMask, RoundedMask, RoundingRadius);
                }
                else
                {
                    brush.PushClip(loc, Size, RoundedMask, RoundingRadius);
                }
                if (parent != null)
                {
                    parent.pushNecessaryClips(brush);
                }
            }
        }

        protected void popNecessaryClips(KitBrush brush)
        {
            if (Masked)
            {
                brush.Pop();
                if (parent != null)
                {
                    parent.popNecessaryClips(brush);
                }
            }
        }
    }
}