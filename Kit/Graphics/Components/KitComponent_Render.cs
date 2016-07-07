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

        public double ComponentDepth { get; set; }

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

        //private bool masked;
        //public bool Masked
        //{
        //    get
        //    {
        //        if (parent != null)
        //        {
        //            return masked | parent.Masked;
        //        }
        //        else
        //        {
        //            return masked;
        //        }
        //    }
        //    set
        //    {
        //        masked = value;
        //    }
        //}

        private object redrawLock = new object();
        protected bool redraw;
        public bool Redraw
        {
            get
            {
                bool ret;
                lock(redrawLock)
                {
                    ret = redraw;
                    foreach (KitComponent child in Children)
                    {
                        ret |= child.Redraw;
                        if (ret)
                        {
                            break;
                        }
                    }
                }
                return ret;
            }
            set
            {
                lock(redrawLock)
                {
                    redraw = value;
                }
            }
        }

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
                    if (outList[i].ComponentDepth >= ComponentDepth)
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
            foreach(KitComponent child in Children)
            {
                child.PreDrawComponent(brush);
            }
        }

        public void _DrawComponent(KitBrush brush)
        {
            DrawComponent(brush);
            Draw?.Invoke();
            OnDraw();
        }
        
#if DEBUG

        KitFont debugFont = new KitFont()
        {
            NormalFont = new Typeface("Veranda"),
            FontSize = 10
        };

        public void DrawComponentDebugInfo(KitBrush brush)
        {
            Vector2 pos = GetAbsoluteLocation();
            brush.SetLineThickness(1);
            brush.DrawRectangle(new Box(pos, Size.X, Size.Y), false, Colors.Red);
            brush.DrawLine(pos + new Vector2(Size.X / 2.0, 0), pos + new Vector2(Size.X / 2.0, Size.Y), Colors.Red, 1);
            brush.DrawLine(pos + new Vector2(0, Size.Y / 2.0), pos + new Vector2(Size.X, Size.Y / 2.0), Colors.Red, 1);
            if(Focused)
                brush.DrawString(GetType().Name, debugFont, pos, Colors.Red);
            foreach(KitComponent child in Children)
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
            if(Masked)
            {
                Vector2 loc = GetAbsoluteLocation();
                brush.PushClip(loc, Size);
            }

            if(parent != null)
            {
                parent.pushNecessaryClips(brush);
            }
        }

        protected void popNecessaryClips(KitBrush brush)
        {
            if(Masked)
            {
                brush.Pop();
            }
            if(parent != null)
            {
                parent.popNecessaryClips(brush);
            }
        }
    }
}