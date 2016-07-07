using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Kit.Graphics.Components;
using Kit.Graphics.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Kit.Graphics.Types;
using Kit.Core;

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
            
            KitImage img2 = new KitImage(@"D:\jpeg\dog.jpg")
            {
                Origin = KitAnchoring.Center,
                Anchor = KitAnchoring.Center,
                ComponentDepth = 0.5
            };

            KitTextBox ktb = new KitTextBox(12, 100)
            {
                ComponentDepth = 1.0
            };
            
            TopLevelComponent.AddChild(img2);
            img2.AddChild(ktb);

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
                TopLevelComponent.UpdateSubcomponents(GlobalTimer.GetCurTime());
                //Console.WriteLine(GlobalTimer.GetCurTime());
            }
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            lock(windowLock)
            {
                MouseState state = 0;
                
                if(e.ButtonState == MouseButtonState.Pressed)
                {
                    state |= MouseState.Down;
                }
                if(e.ChangedButton == MouseButton.Left)
                {
                    state |= MouseState.Left;
                }
                else if(e.ChangedButton != MouseButton.Right)
                {
                    return;
                }

                Vector2 mouseLoc = (Vector2)e.GetPosition(this);

                TopLevelComponent.NotifyMouseInput(mouseLoc, state);
            }
            base.OnMouseDown(e);
        }

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
