using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Kit.Graphics.Components;
using Kit.Graphics.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Kit
{

    /// <summary>
    /// Interaction logic for KitWindow.xaml
    /// </summary>
    public partial class KitWindow : Window
    {
        public TopLevelComponent TopLevelComponent { get; set; }
        private KitBrush componentBrush;
        private object windowLock = new object();

        public KitWindow()
        {
            InitializeComponent();

            TopLevelComponent = new TopLevelComponent(this)
            {
                Anchor = KitAnchoring.Center
            };
            componentBrush = new KitBrush();

            KitImage img = new KitImage(@"D:\jpeg\tst.png")
            {
                Origin = KitAnchoring.Center,
                Anchor = KitAnchoring.RightCenter,
                ComponentDepth = 0.8
            };

            KitImage img2 = new KitImage(@"D:\jpeg\dog.jpg")
            {
                Origin = KitAnchoring.Center,
                Anchor = KitAnchoring.Center,
                ComponentDepth = 0.5
            };

            KitText text = new KitText("text string")
            {
                Origin = KitAnchoring.Center,
                Anchor = KitAnchoring.RightCenter,
                ComponentDepth = 0.9,
                TextColor = Colors.Red
            };

            KitText text2 = new KitText("text string two")
            {
                Origin = KitAnchoring.LeftCenter,
                TextColor = Colors.Blue,
                ComponentDepth = 0.9
            };

            img.ScaleImage(new Graphics.Types.Vector2(0.1, 0.1));
            TopLevelComponent.AddChild(img);
            TopLevelComponent.AddChild(img2);
            img2.AddChild(text);
            text.AddChild(text2);

            SizeChanged += delegate (object sender, SizeChangedEventArgs args)
            {
                TopLevelComponent.Size = new Graphics.Types.Vector2(args.NewSize.Width, args.NewSize.Height);
                TopLevelComponent.Redraw = true;
            };

            CompositionTarget.Rendering += (object sender, EventArgs args) =>
            {
                if (TopLevelComponent.Redraw)
                {
                    lock (windowLock)
                    {
                        InvalidateVisual();
                    }
                }
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            BeginPaint(drawingContext);
            base.OnRender(drawingContext);
        }

        public void BeginPaint(DrawingContext context)
        {
            componentBrush.InitRender(context);
            TopLevelComponent.PreDrawComponentTree(componentBrush);

            TopLevelComponent.DrawComponentTree(componentBrush);
        }

        public void UpdateComponents()
        {
            lock(windowLock)
            {
                TopLevelComponent.UpdateSubcomponents(Core.GlobalTimer.GetCurTime());
            }
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ReleaseCapture();
                SendMessage(new WindowInteropHelper(this).Handle, 0xA1, 0x2, 0);
            }
            base.OnMouseMove(e);
        }
    }
}
