using System.Windows.Media;
using Kit.Core.Delegates;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using System.Collections.Generic;
using System.Timers;
using Kit.Core;

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

        public AnimationControl FadeControl { get; set; }
        public AnimationControl MovementControl { get; set; }

        private Vector2 startPos;
        private Vector2 endPos;

        private Timer fadeTimer;
        private Timer moveTimer;
        private string fadeTag;
        private string moveTag;

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
            if(!FadeControl.AnimationOver() && FadeControl.Animating)
            {
                Opacity = FadeControl.GetGradient();
            }
            if(!MovementControl.AnimationOver() && MovementControl.Animating)
            {
                Location = startPos + ((endPos - startPos) * MovementControl.GetGradient());
            }
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

        private void onFadeElapsed()
        {
            lock (ComponentLock)
            {
                if (FadeControl.AnimationLength != -1)
                {
                    FadeControl.BeginAnimation(time, fadeTag);
                }
            }
        }

        private void onMoveElapsed()
        {
            lock (ComponentLock)
            {
                if (MovementControl.AnimationLength != -1)
                {
                    MovementControl.BeginAnimation(time, moveTag);
                }
            }
        }

        public void SetFade(double startOffset, double time, string tag, bool startInvisible = true, KitEasingMode easeMode = KitEasingMode.EaseIn, KitEasingType easeType = KitEasingType.Line)
        {
            fadeTag = tag;
            FadeControl.Easing.EasingMode = easeMode;
            FadeControl.Easing.EasingType = easeType;
            FadeControl.Inverted = !startInvisible;
            if(startInvisible)
            {
                Opacity = 0;
            }
            else
            {
                Opacity = 1;
            }
            if (startOffset <= 0)
            {
                startOffset = 0.0000000000000000000000001;
            }
            fadeTimer.Interval = startOffset;
            FadeControl.AnimationLength = time;
        }

        public void SetMovement(double startOffset, double time, Vector2 start, Vector2 end, string tag, KitEasingMode easeMode = KitEasingMode.EaseIn, KitEasingType easeType = KitEasingType.Line)
        {
            moveTag = tag;
            MovementControl.Easing.EasingMode = easeMode;
            MovementControl.Easing.EasingType = easeType;
            if (startOffset <= 0)
            {
                startOffset = 0.0000000000000000000000001;
            }
            moveTimer.Interval = startOffset;
            MovementControl.AnimationLength = time;
            startPos = start;
            endPos = end;
            Location = start;
        }

        public void StartAnimation(bool move = true, bool fade = true)
        {
            if(fade)
            {
                fadeTimer.AutoReset = false;
                if (FadeControl.Inverted)
                {
                    Opacity = 1;
                }
                else
                {
                    Opacity = 0;
                }
                fadeTimer.Start();
            }
            if (move)
            {
                moveTimer.AutoReset = false;
                Location = startPos;
                Redraw = true;
                moveTimer.Start();
            }
        }

        public void StopAnimation()
        {
            fadeTimer.Stop();
            moveTimer.Stop();
            double t1 = fadeTimer.Interval;
            double t2 = moveTimer.Interval;
            fadeTimer.Dispose();
            moveTimer.Dispose();
            fadeTimer = new Timer(t1);
            moveTimer = new Timer(t2);
        }
    }
}