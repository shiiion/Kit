using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using System.Windows.Media;

namespace Kit.Graphics.Components
{
    class KitTextAnimation : KitText
    {

        private AnimationControl FadeControl { get; set; }
        private AnimationControl MovementControl { get; set; }

        private Vector2 startPos;
        private Vector2 endPos;

        private Timer fadeTimer;
        private Timer moveTimer;

        private Color colorSave;

        public KitTextAnimation(string text = "", string font = "Consolas", double ptSize = 12)
            : base(text, font, ptSize)
        {
            FadeControl = new AnimationControl(-1, 1);
            MovementControl = new AnimationControl(-1, 1);
            fadeTimer = new Timer();
            moveTimer = new Timer();

            fadeTimer.Elapsed += (sender, e) => onFadeElapsed();
            moveTimer.Elapsed += (sender, e) => onMoveElapsed();
        }

        private void onFadeElapsed()
        {
            lock(ComponentLock)
            {
                if (FadeControl.AnimationLength != -1)
                {
                    FadeControl.BeginAnimation(time, "fade");
                }
            }
        }

        private void onMoveElapsed()
        {
            lock (ComponentLock)
            {
                if (MovementControl.AnimationLength != -1)
                {
                    MovementControl.BeginAnimation(time, "move");
                }
            }
        }

        protected override void OnUpdate()
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
            Redraw = draw;
            base.OnUpdate();
        }

        public void SetFade(double startOffset, double time)
        {
            colorSave = TextColor;
            TextColor = Color.FromArgb(0, colorSave.R, colorSave.G, colorSave.B);
            fadeTimer.Interval = startOffset;
            FadeControl.AnimationLength = time;
        }

        public void SetMovement(double startOffset, double time, Vector2 start, Vector2 end)
        {
            moveTimer.Interval = startOffset;
            MovementControl.AnimationLength = time;
            startPos = start;
            endPos = end;
            Location = start;
        }

        public void StartAnimation()
        {
            fadeTimer.AutoReset = false;
            moveTimer.AutoReset = false;
            fadeTimer.Start();
            moveTimer.Start();
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

        protected override void DrawComponent(KitBrush brush)
        {

            if(FadeControl.Animating)
            {
                TextColor = Color.FromArgb((byte)(colorSave.A * FadeControl.GetGradient()), colorSave.R, colorSave.G, colorSave.B);
            }
            if(MovementControl.Animating)
            {
                Location = startPos + ((endPos - startPos) * MovementControl.GetGradient());
            }
            base.DrawComponent(brush);
        }
    }
}
