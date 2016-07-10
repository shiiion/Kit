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

        private Vector2 lastMouseLoc;

        private Vector2 windowLoc;

        public KitWindow()
        {
            InitializeComponent();

            lastMouseLoc = new Vector2(-1, -1);

            TopLevelComponent = new TopLevelComponent(this, () =>
            {
                lock (App.WindowListLock)
                {
                    App.WindowList.Remove(this);
                    Close();
                }
            });
            componentBrush = new KitBrush();

            KitImage img2 = new KitImage(@"D:\jpeg\VqGMB7T.png")
            {
                Origin = KitAnchoring.Center,
                Anchor = KitAnchoring.Center,
                ComponentDepth = 0.5,
                Location = new Vector2(0, 22)
            };

            KitTextBox ktb = new KitTextBox(12, 200)
            {
                Origin = KitAnchoring.Center,
                Anchor = KitAnchoring.Center,
                ComponentDepth = 1.0
            };
            TopLevelComponent.AddChild(img2);
            img2.Size = TopLevelComponent.Size;
            img2.AddChild(ktb);

            SizeChanged += (o, e) =>
            {
                TopLevelComponent.Size = new Vector2(e.NewSize.Width, e.NewSize.Height);
                TopLevelComponent.Redraw = true;
                img2.Size = TopLevelComponent.Size;
            };

            CompositionTarget.Rendering += (o, e) =>
            {
                if (TopLevelComponent.Redraw)
                {
                    lock (windowLock)
                    {
                        InvalidateVisual();
                    }
                }
            };

            Loaded += (o, e) =>
            {
                Left = 200;
                Top = 200;
                windowLoc = new Vector2(200, 200);
            };

            LocationChanged += (o, e) =>
            {
                windowLoc = new Vector2(Left, Top);
            };
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

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
            lock (windowLock)
            {
                TopLevelComponent.WindowLocation = windowLoc;
                TopLevelComponent.UpdateSubcomponents(GlobalTimer.GetCurTime());
            }
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            lock (windowLock)
            {
                lastMouseLoc = (Vector2)e.GetPosition(this);
                MouseState state = 0;

                state |= MouseState.Down;

                if (e.ChangedButton == MouseButton.Left)
                {
                    state |= MouseState.Left;
                }
                else if (e.ChangedButton != MouseButton.Right)
                {
                    return;
                }

                Vector2 mouseLoc = (Vector2)e.GetPosition(this);

                TopLevelComponent.NotifyMouseInput(mouseLoc, state);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            lock (windowLock)
            {
                lastMouseLoc = new Vector2(-1, -1);
                MouseState state = 0;

                if (e.ChangedButton == MouseButton.Left)
                {
                    state |= MouseState.Left;
                }
                else if (e.ChangedButton != MouseButton.Right)
                {
                    return;
                }

                Vector2 mouseLoc = (Vector2)e.GetPosition(this);

                TopLevelComponent.NotifyMouseInput(mouseLoc, state);
            }
            base.OnMouseUp(e);
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            lock (windowLock)
            {
                TopLevelComponent.NotifyTextInput(e.Text);
            }
            base.OnTextInput(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            lock (windowLock)
            {
                KeyState state;
                if (e.IsRepeat)
                {
                    state = KeyState.Hold;
                }
                else
                {
                    state = KeyState.Press;
                }
                TopLevelComponent.NotifyKeyInput(e.Key, state);
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            lock (windowLock)
            {
                TopLevelComponent.NotifyKeyInput(e.Key, KeyState.Release);
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (lastMouseLoc.X != -1)
            {
                MouseState state;
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    state = MouseState.Left;
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    state = 0;
                }
                else
                {
                    base.OnMouseMove(e);
                    return;
                }

                Vector2 newLoc = (Vector2)e.GetPosition(this);
                if (TopLevelComponent.NotifyMouseMove(state, lastMouseLoc, newLoc) && e.LeftButton == MouseButtonState.Pressed)
                {
                    ReleaseCapture();
                    SendMessage(new WindowInteropHelper(this).Handle, 0xA1, 0x2, 0);
                }
                lastMouseLoc = newLoc;
            }
            base.OnMouseMove(e);
        }
    }
}
