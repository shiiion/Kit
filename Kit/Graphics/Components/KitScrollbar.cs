using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Types;
using System.Windows.Media;
using Kit.Graphics.Drawing;
using Kit.Core;

namespace Kit.Graphics.Components
{
    class KitScrollbar : KitComponent
    {
        private KitBox scrollBar;

        public double ScrollLocation { get; set; }
        
        public bool Enabled { get; set; }

        public double ScrollAmount { get; set; }
        public double ScrollStep { get; set; }

        private IScrollable scrollComponent;

        private bool isClicked;
        private Vector2 clickLocation;
        private Vector2 startLocation;

        public KitScrollbar(KitComponent parent)
        {
            ScrollAmount = 1;
            ScrollStep = 1;
            Anchor = KitAnchoring.TopRight;
            Origin = KitAnchoring.TopRight;

            scrollBar = new KitBox(Color.FromArgb(0xE0, 0xD3, 0xD3, 0xD3), new Vector2(16, parent.Size.Y))
            {
                Anchor = KitAnchoring.TopCenter,
                Origin = KitAnchoring.TopCenter,
                ComponentDepth = parent.ComponentDepth,
                EdgeRounding = 2
            };
            isClicked = false;
            AddChild(scrollBar);
            ScrollLocation = 0;
            Size = new Vector2(16, parent.Size.Y);
            parent.AddChild(this);
            ComponentDepth = parent.ComponentDepth;
            parent.Resize += () =>
            {
                Size = new Vector2(16, parent.Size.Y);
            };

            DepthChanged += () =>
            {
                scrollBar.ComponentDepth = ComponentDepth;
            };
            clickLocation = new Vector2();
            Enabled = false;
        }

        public void RegisterScrollbar(IScrollable registrar)
        {
            scrollComponent = registrar;
        }

        public void OnContentSizeChange()
        {
            if (Enabled = scrollComponent.ContentLargerThanArea())
            {
                double heightScale = Size.Y / scrollComponent.ContentDimensions().Y;
                scrollBar.Size = new Vector2(16, Math.Max(8, Size.Y * heightScale));
            }
            else
            {
                scrollBar.Size = new Vector2(16, Size.Y);
                scrollBar.Location = new Vector2();
                ScrollLocation = 0;
            }
        }

        protected override void OnMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            if (Enabled)
            {
                isClicked = ((mouseFlags & MouseState.Down) == MouseState.Down) && Focused;
                this.clickLocation = clickLocation;
                startLocation = scrollBar.Location;
            }
            base.OnMouseInput(clickLocation, mouseFlags);
        }

        private bool OOB(double loc)
        {
            return (loc < 0) || (loc > (scrollComponent.ContentDimensions().Y / ScrollStep));
        }
        
        protected override void OnScroll(Vector2 pos, int direction)
        {
            if((Contains(pos) || scrollComponent.ContainsCursor(pos)) && Enabled)
            {
                if (direction > 0)
                {
                    SetScrollLocation(ScrollLocation - ScrollAmount);
                    if(OOB(ScrollLocation))
                    {
                        SetScrollLocation(0);
                    }
                }
                else
                {
                    SetScrollLocation(ScrollLocation + ScrollAmount);
                    if (OOB(ScrollLocation))
                    {
                        SetScrollLocation(scrollComponent.ContentDimensions().Y / ScrollStep);
                    }
                }
                Redraw = true;
            }
            base.OnScroll(pos, direction);
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if (Enabled)
            {
                if (isClicked)
                {
                    double relY = end.Y - clickLocation.Y;
                    double yLoc = Math.Max(0, Math.Min(startLocation.Y + relY + scrollBar.Size.Y, Size.Y) - scrollBar.Size.Y);
                    scrollBar.Location = new Vector2(scrollBar.Location.X, yLoc);
                    Redraw = true;
                    double scrollFract = scrollBar.Location.Y / (Size.Y - scrollBar.Size.Y);

                    ScrollLocation = scrollFract * (scrollComponent.ContentDimensions().Y / ScrollStep);
                }
            }
            return base.OnMouseMove(state, start, end);
        }

        public void SetScrollLocation(double loc)
        {
            ScrollLocation = loc;
            double fract = ScrollLocation / (scrollComponent.ContentDimensions().Y / ScrollStep);
            scrollBar.Location = new Vector2(0, fract * (Size.Y - scrollBar.Size.Y));
        }

        public override void PreDrawComponent(KitBrush brush)
        {
            if (Enabled)
            {
                scrollBar.BoxColor = Color.FromArgb(0xE0, 0xD3, 0xD3, 0xD3);
            }
            else
            {
                scrollBar.BoxColor = Color.FromArgb(0xE0, 0x43, 0x43, 0x43);
            }
            base.PreDrawComponent(brush);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            Vector2 lineStart = scrollBar.GetAbsoluteLocation();
            lineStart.AddThis(scrollBar.Size * 0.5);
            Color splitColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            brush.DrawLine(new Vector2(lineStart.X - 4, lineStart.Y), new Vector2(lineStart.X + 4, lineStart.Y), splitColor, 1, true);
            brush.DrawLine(new Vector2(lineStart.X - 2, lineStart.Y + 2), new Vector2(lineStart.X + 2, lineStart.Y + 2), splitColor, 1, true);
            brush.DrawLine(new Vector2(lineStart.X - 2, lineStart.Y - 2), new Vector2(lineStart.X + 2, lineStart.Y - 2), splitColor, 1, true);
            brush.DrawRectangle(new Box(GetAbsoluteLocation(), Size), true, Color.FromArgb(0x01, 0x40, 0x40, 0x40));
            base.DrawComponent(brush);
        }
    }
}
