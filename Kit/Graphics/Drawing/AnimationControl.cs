using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Graphics.Drawing
{
    public class AnimationControl
    {
        public double AnimationLength { get; set; }

        public double ValueRange { get; set; }

        public string AnimationTag { get; set; }

        private bool animating;
        public bool Animating { get { return animating; } }

        private double curGradientValue;
        public double StartTime { get; set; }
        
        public AnimationControl(double animationLength, double valueRange)
        {
            AnimationLength = animationLength;
            ValueRange = valueRange;
            StartTime = -1;
            ResetAnimation();
            AnimationTag = "";
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
            return curGradientValue * ValueRange;
        }

        public bool AnimationOver()
        {
            return curGradientValue >= 1;
        }

        public void StepAnimation(double curTime)
        {
            if(animating && !AnimationOver())
            {
                curGradientValue = (curTime - StartTime) / AnimationLength;
                if(curGradientValue > 1)
                {
                    curGradientValue = 1;
                }
            }
        }

    }
}
