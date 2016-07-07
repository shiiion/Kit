using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;


namespace Kit.Graphics.Components
{
    public class KitImage : KitComponent
    {
        public ImageSource Image { get; set; }

        public KitFont font;

        private string imagePath;
        public string ImagePath
        { 
            get { return imagePath; }
            set
            {
                imagePath = value;
                loadImage(imagePath);
            }
        }

        public KitImage(string path = "")
        {
            ImagePath = path;

            if (!string.IsNullOrWhiteSpace(path))
            {
                Size = new Vector2(Image.Width, Image.Height);
            }
        }

        public void loadImage(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Uri uri = new Uri(path);
                Image = new BitmapImage(uri);
                Size = new Vector2(Image.Width, Image.Height);
            }
        }

        public void ScaleImage(Vector2 scale)
        {
            Size = new Vector2(Image.Width * scale.X, Image.Height * scale.Y);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            pushNecessaryClips(brush);
            Vector2 loc = GetAbsoluteLocation();
            brush.DrawImage(Image, new Box(loc, Size));
            popNecessaryClips(brush);
            redraw = false;
            base.DrawComponent(brush);
        }
    }
}
