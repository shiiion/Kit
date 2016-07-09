using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using Kit.Graphics.Types;
using System.Windows.Media;


namespace Kit.Graphics.Drawing
{
    public class KitBrush
    {
        private DrawingContext renderer;

        public SolidColorBrush colorBrush;
        public Pen lineBrush;

        public void InitRender(DrawingContext renderer)
        {
            this.renderer = renderer;
        }

        public KitBrush()
        {
            colorBrush = new SolidColorBrush();
            lineBrush = new Pen(colorBrush, 5);
        }

        public void SetLineThickness(double thickness)
        {
            lineBrush = new Pen(colorBrush, thickness);
        }
        
        public void SetColor(Color color)
        {
            colorBrush = new SolidColorBrush(color);
            lineBrush = new Pen(colorBrush, lineBrush.Thickness);
        }

        public void DrawLine(Line line, Color color)
        {
            if (renderer == null)
            {
                //THROW EXC
            }

            SetColor(color);

            SetLineThickness((float)line.Width);

            renderer.DrawLine(lineBrush, (Point)line.Point, (Point)line.Endpoint);
        }

        public void DrawLine(Vector2 p1, Vector2 p2, Color color, float thickness)
        {
            if (renderer == null)
            {
                //THROW EXC
            }

            if (thickness > 0)
            {
                SetLineThickness(thickness);
            }

            SetColor(color);

            renderer.DrawLine(lineBrush, (Point)p1, (Point)p2);
        }

        public static Vector2 GetTextBounds(string text, KitFont font)
        {
            FormattedText ft = new FormattedText(text, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, font.NormalFont, font.FontSize, Brushes.Red);
            
            return new Vector2(ft.WidthIncludingTrailingWhitespace, ft.Height);
        }

        public void DrawString(string text, KitFont font, Vector2 location, Color color)
        {
            if (renderer == null)
            {
                //THROW EXC
            }

            SetColor(color);

            if (!font.IsCustom)
            {
                FormattedText ft = new FormattedText(text, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, font.NormalFont, font.FontSize, colorBrush);
                renderer.DrawText(ft, (Point)location);
                
            }
            else
            {
                //DrawStringCustom(text, font, tool, location);
            }
        }

        private void DrawStringCustom(string text, KitFont customFont, Vector2 location)
        {
            //IMPLEMENT ME
        }

        public void DrawImage(ImageSource image, Box outRect = default(Box))
        {
            if (renderer == null)
            {
                //THROW EXC
            }
            
            if (outRect.Size.X == 0 || outRect.Size.Y == 0)
            {
                renderer.DrawImage(image, new Rect(outRect.Pos.X, outRect.Pos.Y, image.Width, image.Height));
            }
            else
            {
                renderer.DrawImage(image, new Rect(outRect.Pos.X, outRect.Pos.Y, outRect.Size.X, outRect.Size.Y));
            }
        }



        public void DrawRectangle(Box rectangle, bool filled, Color color)
        {
            if (renderer == null)
            {
                //THROW EXC
            }

            Rect drawArea = new Rect(rectangle.Pos.X, rectangle.Pos.Y, rectangle.Size.X, rectangle.Size.Y);

            SetColor(color);
            
            if (filled)
            {
                renderer.DrawRectangle(colorBrush, null, drawArea);
            }
            else
            {
                renderer.DrawRectangle(null, lineBrush, drawArea);
            }
        }

        public void DrawRoundedRectangle(Box rectangle, bool filled, Color color, double radX, double radY)
        {
            if (renderer == null)
            {
                //THROW EXC
            }

            Rect drawArea = new Rect(rectangle.Pos.X, rectangle.Pos.Y, rectangle.Size.X, rectangle.Size.Y);

            SetColor(color);

            if (filled)
            {
                renderer.DrawRoundedRectangle(colorBrush, null, drawArea, radX, radY);
            }
            else
            {
                renderer.DrawRoundedRectangle(null, lineBrush, drawArea, radX, radY);
            }
        }

        public void PushClip(Vector2 pos, Vector2 size)
        {
            if(renderer == null)
            {
                //THROW EXC
            }
            renderer.PushClip(new RectangleGeometry(new Rect(pos.X, pos.Y, size.X, size.Y)));
        }

        public void Pop()
        {
            if (renderer == null)
            {
                //THROW EXC
            }
            renderer.Pop();
        }
    }
}
