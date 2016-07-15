using System;
using System.Windows.Media;
using System.Windows;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    public class KitText : KitComponent
    {
        private string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                Size = getTextSize(text);
                Redraw = true;
            }
        }
        public KitFont Font { get; set; }

        public Color TextColor { get; set; }

        public KitText(string text = "", string font = "Consolas", double ptSize = 12)
            : base()
        {
            Font = new KitFont()
            {
                FontSize = ptSize
            };
            LoadFont(font, FontStyles.Normal, FontWeights.Normal);
            Text = text;
            TextColor = Colors.Black;
        }

        public void LoadFont(string font, FontStyle style, FontWeight weight)
        {
            Font.IsCustom = false;
            Font.NormalFont = new Typeface(new FontFamily(font), style, weight, FontStretches.Normal);
        }

        private Vector2 getTextSize(string text)
        {
            FormattedText formattedText = new FormattedText(
                text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                Font.NormalFont, Font.FontSize, Brushes.Red);
            return new Vector2(formattedText.Width, formattedText.Height);
        }

        public override void PreDrawComponent(KitBrush brush)
        {
            Size = getTextSize(Text);
            base.PreDrawComponent(brush);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            Vector2 pos = GetAbsoluteLocation();
            pushNecessaryClips(brush);
            brush.DrawString(Text, Font, pos, TextColor);
            popNecessaryClips(brush);
            base.DrawComponent(brush);
        }
    }
}
