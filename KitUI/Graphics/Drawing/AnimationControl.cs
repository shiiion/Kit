using Kit.Core;

namespace Kit.Graphics.Drawing
{
    public class AnimationControl
    {
        public KitEasing Easing { get; set; }

        public double AnimationLength { get; set; }

        public double ValueRange { get; set; }

        public string AnimationTag { get; set; }

        private bool animating;
        public bool Animating { get { return animating; } }

        private double curGradientValue;
        public double StartTime { get; set; }

        public bool Inverted { get; set; }

        public AnimationControl(double animationLength, double valueRange)
        {
            AnimationLength = animationLength;
            ValueRange = valueRange;
            StartTime = -1;
            ResetAnimation();
            AnimationTag = "";
            Easing = new KitEasing(KitEasingMode.EaseIn, KitEasingType.Line);
        }

        public void ResetAnimation()
        {
            animating = false;
            curGradientValue = 0;
            StartTime = -1;
        }

        public void BeginAnimation(double startTime, string tag)
        {
            AnimationTag = tag;
            animating = true;
            StartTime = startTime;
            curGradientValue = 0;
        }

        public double GetGradient()
        {
            if (Inverted)
            {
                return (1 - curGradientValue) * ValueRange;
            }
            else
            {
                return curGradientValue * ValueRange;
            }
        }

        public bool AnimationOver()
        {
            return curGradientValue >= 1;
        }

        private void ease(double curTime)
        {
            double normTime = (curTime - StartTime) / AnimationLength;
            if (normTime > 1)
            {
                curGradientValue = 1;
            }
            else
            {
                curGradientValue = Easing.Ease(normTime);
            }
        }

        public void StepAnimation(double curTime)
        {
            if (animating && !AnimationOver())
            {
                ease(curTime);
            }
        }

    }
}
