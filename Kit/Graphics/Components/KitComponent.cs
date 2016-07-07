using System.Collections.Generic;
using Kit.Graphics.Types;
using Kit.Core.Delegates;
using System.Runtime.InteropServices;

namespace Kit.Graphics.Components
{
    /// <summary>
    /// General representation of a component
    /// </summary>
    public partial class KitComponent : IContainer<KitComponent>
    {
        public List<KitComponent> Children { get; set; }

        private KitComponent parent;

        /// <summary>
        /// Offset from parent's anchor from this origin
        /// </summary>
        private Vector2 location;

        private Vector2 size;
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                Resize?.Invoke();
            }
        }

        public KitAnchoring Anchor;
        public KitAnchoring Origin;

        public Vector2 CustomOrigin { get; set; }
        public Vector2 CustomAnchor { get; set; }

        public event VoidDelegate Update;

        public event VoidDelegate Resize;

        protected double time;

        protected bool debugKey_Pressed = false;

        public KitComponent(Vector2 location = default(Vector2), Vector2 Size = default(Vector2))
        {
            redraw = true;
            Children = new List<KitComponent>();
            parent = null;
            this.location = location;
            this.Size = Size;
            ComponentDepth = 0;
        }

        public void SetParent(KitComponent parent)
        {
            if(parent.parent == this)
            {
                //THROW EXC
                return;
            }
            this.parent = parent;
        }

        public void AddChild(KitComponent child)
        {
            if(!isValidChild(child))
            {
                //THROW EXC
                return;
            }
            Children.Add(child);
            child.SetParent(this);
        }

        private bool isValidChild(KitComponent child)
        {
            if(parent == child)
            {
                return false;
            }
            KitComponent cParent = child.parent;
            while (cParent != null)
            {
                cParent = cParent.parent;
                if(parent == cParent)
                {
                    return false;
                }
            }
            return true;
        }

        public Vector2 GetLocation()
        {
            return location + GetOffset();
        }

        public Vector2 GetAbsoluteLocation()
        {
            if (parent == null)
            {
                return GetLocation();
            }
            else
            {
                return GetLocation() + parent.GetAbsoluteLocation();
            }
        }

#if DEBUG
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int key);
#endif

        protected virtual void OnUpdate()
        {
#if DEBUG
            if (GetAsyncKeyState(0x10) != 0)
            {
                if (!debugKey_Pressed)
                {
                    debugKey_Pressed = true;
                    Redraw = true;
                }
            }
            else
            {
                if (debugKey_Pressed)
                {
                    debugKey_Pressed = false;
                    Redraw = true;
                }
            }
#endif
        }

        public void UpdateSubcomponents(double CurTime)
        {
            time = CurTime;

            foreach (KitComponent child in Children)
            {
                child.UpdateSubcomponents(CurTime);
            }

            Update?.Invoke();
            OnUpdate();
        }

        protected Vector2 GetOffset()
        {
            if(parent == null)
            {
                return Vector2.Zero;
            }
            switch(parent.Anchor)
            {
                default:
                case KitAnchoring.TopLeft:
                    return GetOffset(Vector2.Zero);
                case KitAnchoring.TopCenter:
                    return GetOffset(new Vector2(parent.Size.X / 2.0));
                case KitAnchoring.TopRight:
                    return GetOffset(new Vector2(parent.Size.X));
                case KitAnchoring.LeftCenter:
                    return GetOffset(new Vector2(0, parent.Size.Y / 2.0));
                case KitAnchoring.Center:
                    return GetOffset(new Vector2(parent.Size.X / 2.0, parent.Size.Y / 2.0));
                case KitAnchoring.RightCenter:
                    return GetOffset(new Vector2(parent.Size.X, parent.Size.Y / 2.0));
                case KitAnchoring.BottomLeft:
                    return GetOffset(new Vector2(0, parent.Size.Y));
                case KitAnchoring.BottomCenter:
                    return GetOffset(new Vector2(parent.Size.X / 2.0, parent.Size.Y));
                case KitAnchoring.BottomRight:
                    return GetOffset(new Vector2(parent.Size.X, parent.Size.Y));
                case KitAnchoring.CustomAnchor:
                    return GetOffset(parent.CustomAnchor);
            }
        }

        protected Vector2 GetOffset(Vector2 anchorOffset)
        {
            switch(Origin)
            {
                default:
                case KitAnchoring.TopLeft:
                    return anchorOffset;
                case KitAnchoring.TopCenter:
                    return anchorOffset - new Vector2(Size.X / 2.0);
                case KitAnchoring.TopRight:
                    return anchorOffset - new Vector2(Size.X);
                case KitAnchoring.LeftCenter:
                    return anchorOffset - new Vector2(0, Size.Y / 2.0);
                case KitAnchoring.Center:
                    return anchorOffset - new Vector2(Size.X / 2.0, Size.Y / 2.0);
                case KitAnchoring.RightCenter:
                    return anchorOffset - new Vector2(Size.X, Size.Y / 2.0);
                case KitAnchoring.BottomLeft:
                    return anchorOffset - new Vector2(0, Size.Y);
                case KitAnchoring.BottomCenter:
                    return anchorOffset - new Vector2(Size.X / 2.0, Size.Y);
                case KitAnchoring.BottomRight:
                    return anchorOffset - new Vector2(Size.X, Size.Y);
                case KitAnchoring.CustomAnchor:
                    return anchorOffset - CustomOrigin;
            }
        }
    }
}
