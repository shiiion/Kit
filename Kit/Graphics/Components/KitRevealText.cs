using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    class KitRevealText : KitText
    {
        private bool running;
        public bool Running { get { return running; } }

        /// <summary>
        /// wait time between each character being shown
        /// </summary>
        public double RevealSpeed { get; set; }
        private double lastRevealTime;

        public bool PauseOnPunctuation { get; set; }

        public double PauseSpeed { get; set; }

        private int curIndex;

        public KitRevealText(double revealSpeed, string text, string font, double ptSize)
            : base(text, font, ptSize)
        {
            running = false;
            RevealSpeed = revealSpeed;
            PauseSpeed = 500;
            PauseOnPunctuation = true;
            curIndex = 0;
        }
        
        public void StartAnimation()
        {
            curIndex = 0;
            running = true;
            lastRevealTime = time;
        }

        public void StopAnimation()
        {
            running = false;
        }

        private void skipWhitespace()
        {
            while(curIndex < Text.Length && char.IsWhiteSpace(Text[curIndex]))
            {
                curIndex++;
            }
        }

        private bool isPunctuation(int loc)
        {
            loc--;
            while(loc > 0 && char.IsWhiteSpace(Text[loc]))
            {
                loc--;
            }
            if(loc < 0)
            {
                return false;
            }
            char character = Text[loc];
            return character == '.' || character == '!' || character == '?';
        }

        protected override void OnUpdate()
        {
            if(Running)
            {
                if(curIndex < Text.Length)
                {
                    if (isPunctuation(curIndex))
                    {
                        if (time - lastRevealTime > PauseSpeed)
                        {
                            lastRevealTime = time;
                            curIndex++;
                            Redraw = true;
                        }
                    }
                    else if (time - lastRevealTime > RevealSpeed)
                    {
                        lastRevealTime = time;
                        curIndex++;
                        Redraw = true;
                    }
                    skipWhitespace();
                }
            }
            base.OnUpdate();
        }

        protected override void DrawComponent(KitBrush brush)
        {
            if(Running)
            {
                Vector2 pos = GetAbsoluteLocation();
                pushNecessaryClips(brush);
                brush.DrawString(Text.Substring(0, curIndex), Font, pos, TextColor);
                popNecessaryClips(brush);
            }
            Redraw = false;
        }
    }
}
