﻿using System.Collections.Generic;
using Kit.Graphics.Types;
using Kit.Core.Delegates;
using Kit.Graphics.Drawing;
using System.Timers;
using Kit.Core;
using System.Windows.Input;

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
        public Vector2 Location { get; set; }

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

        public bool Draggable { get; set; }

        private object componentLock;
        public object ComponentLock { get { return componentLock; } }


        public KitAnchoring Anchor;
        public KitAnchoring Origin;

        public Vector2 CustomOrigin { get; set; }
        public Vector2 CustomAnchor { get; set; }

        public event ComponentDelegate Update;
        public event VoidDelegate Resize;
        public event MouseStateDelegate MouseInput;
        public event KeyStateDelegate KeyInput;
        public event StringDelegate TextInput;
        public event MouseMoveDelegate MouseMove;
        public event MouseScrollDelegate Scroll;

        public bool Focused { get; set; }

        protected double time;

        public bool debugKey_Pressed { get; set; }

        public KitComponent(Vector2 location = default(Vector2), Vector2 Size = default(Vector2))
        {
            componentLock = new object();
            Focused = false;
            redraw = true;
            Masked = false;
            ShouldDraw = true;

            Children = new List<KitComponent>();

            parent = null;

            Location = location;
            this.Size = Size;
            ComponentDepth = 0;
            Opacity = 1;
            CustomMask = Size;

            RoundedMask = false;
            RoundingRadius = 0;

            FadeControl = new AnimationControl(-1, 1);
            MovementControl = new AnimationControl(-1, 1);
            fadeTimer = new Timer();
            moveTimer = new Timer();

            fadeTimer.Elapsed += (sender, e) => onFadeElapsed();
            moveTimer.Elapsed += (sender, e) => onMoveElapsed();
        }

        public void SetParent(KitComponent parent)
        {
            if (parent.parent == this)
            {
                //THROW EXC
                return;
            }
            this.parent = parent;
        }

        public virtual Vector2 GetWindowLocation()
        {
            if (parent != null)
            {
                return parent.GetWindowLocation();
            }
            else
            {
                return default(Vector2);
            }
        }

        public void AddChild(KitComponent child)
        {
            if (!isValidChild(child))
            {
                //THROW EXC
                return;
            }
            Children.Add(child);
            child.SetParent(this);
        }

        private bool isValidChild(KitComponent child)
        {
            if (parent == child)
            {
                return false;
            }
            KitComponent cParent = child.parent;
            while (cParent != null)
            {
                cParent = cParent.parent;
                if (parent == cParent)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Contains(Vector2 point)
        {
            Box thisBox = new Box(GetAbsoluteLocation(), Size);
            return thisBox.Contains(point);
        }

        public bool ContainsCursor()
        {
            Box thisBox = new Box(GetAbsoluteLocation(), Size);
            return thisBox.Contains(InteropFunctions.GetCursorPosition() - GetWindowLocation());
        }

        public Vector2 GetLocation()
        {
            return Location + GetOffset();
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

        protected virtual void OnUpdate()
        {
            bool draw = false;
            if (!FadeControl.AnimationOver() && FadeControl.Animating)
            {
                FadeControl.StepAnimation(time);
                draw = true;
            }
            if (!MovementControl.AnimationOver() && MovementControl.Animating)
            {
                MovementControl.StepAnimation(time);
                draw = true;
            }
            Redraw |= draw;
        }

        public void UpdateSubcomponents(double CurTime)
        {

            foreach (KitComponent child in Children)
            {
                child.UpdateSubcomponents(CurTime);
            }

            lock (componentLock)
            {
                time = CurTime;
                Update?.Invoke(this);
                OnUpdate();
            }
        }

        protected virtual void OnMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        { }

        protected virtual void OnKeyInput(Key key, KeyState state)
        {
#if DEBUG
            if (key == Key.RightCtrl)
            {
                if (state == KeyState.Press)
                {
                    if (!debugKey_Pressed)
                    {
                        debugKey_Pressed = true;
                        Redraw = true;
                    }
                }
                if (state == KeyState.Release)
                {
                    if (debugKey_Pressed)
                    {
                        debugKey_Pressed = false;
                        Redraw = true;
                    }
                }
            }
#endif
        }

        protected virtual void OnTextInput(string text)
        { }

        protected virtual void OnScroll(Vector2 pos, int direction)
        { }

        /// <returns>If the window is enabled to move</returns>
        protected virtual bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            return false;
        }

        public void _NotifyMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyMouseInput(clickLocation, mouseFlags);
            }
            lock (ComponentLock)
            {
                if (Contains(clickLocation) && mouseFlags == MouseState.LeftDown)
                {
                    Focused = true;
                }
                else if (mouseFlags == MouseState.LeftDown)
                {
                    Focused = false;
                }
                OnMouseInput(clickLocation, mouseFlags);
                MouseInput?.Invoke(clickLocation, mouseFlags);
            }
        }

        public void _NotifyTextInput(string text)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyTextInput(text);
            }
            lock (ComponentLock)
            {
                OnTextInput(text);
                TextInput?.Invoke(text);
            }
        }

        public void _NotifyKeyInput(Key key, KeyState state)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyKeyInput(key, state);
            }
            lock (ComponentLock)
            {
                OnKeyInput(key, state);
                KeyInput?.Invoke(key, state);
            }
        }

        public void _NotifyScroll(Vector2 mouseLoc, int direction)
        {
            foreach (KitComponent child in Children)
            {
                child._NotifyScroll(mouseLoc, direction);
            }
            lock (ComponentLock)
            {
                OnScroll(mouseLoc, direction);
                Scroll?.Invoke(mouseLoc, direction);
            }
        }

        public bool _NotifyMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            bool ret = false;
            foreach (KitComponent child in Children)
            {
                ret |= child._NotifyMouseMove(state, start, end);
            }
            lock (ComponentLock)
            {
                ret |= OnMouseMove(state, start, end);
                MouseMove?.Invoke(state, start, end);
            }
            return ret;
        }

        protected Vector2 GetOffset()
        {
            if (parent == null)
            {
                return Vector2.Zero;
            }
            switch (Anchor)
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
            switch (Origin)
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
