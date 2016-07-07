using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    class KitText : KitComponent
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

        public KitText(string text = "", string font = "Veranda", double ptSize = 12, Vector2 location = default(Vector2), Vector2 size = default(Vector2))
            : base(location, size)
        {
            Font = new KitFont()
            {
                FontSize = ptSize
            };
            LoadFont(text);
            Text = text;
            TextColor = Colors.Black;
        }

        public void LoadFont(string font)
        {
            Font.IsCustom = false;
            Font.NormalFont = new Typeface(font);
        }

        private Vector2 getTextSize(string text)
        {
            FormattedText formattedText = new FormattedText(
                text, System.Globalization.CultureInfo.GetCultureInfo("en-us"), System.Windows.FlowDirection.LeftToRight,
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
