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
        public bool ClickableBackground { get; set; }
        private KitBrush componentBrush;
        private object windowLock = new object();

        private Vector2 lastMouseLoc;
        private Vector2 lastMouseMove;

        private Vector2 windowLoc;

        public KitWindow(string title)
        {
            InitializeComponent();

            lastMouseLoc = new Vector2(-1, -1);

            TopLevelComponent = new TopLevelComponent(new Vector2(Width, Height), title, () =>
            {
                lock(App.RemoveLock)
                {
                    App.RemoveList.Add(this);
                    Close();
                }
            }, @"pack://application:,,,/Resources/XButton.png", @"pack://application:,,,/Resources/XButtonDown.png");
            componentBrush = new KitBrush();
            SizeChanged += (o, e) =>
            {
                TopLevelComponent.Size = new Vector2(e.NewSize.Width, e.NewSize.Height);
                TopLevelComponent.Redraw = true;
            };

            CompositionTarget.Rendering += (o, e) =>
            {
                lock(windowLock)
                {
                    if (TopLevelComponent.Redraw)
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            lock (windowLock)
            {
                BeginPaint(drawingContext);
            }
            base.OnRender(drawingContext);
        }

        public void BeginPaint(DrawingContext context)
        {
            componentBrush.InitRender(context);
            TopLevelComponent.PreDrawComponentTree(componentBrush);

            TopLevelComponent.DrawComponentTree(componentBrush, ClickableBackground);
        }

        public void UpdateComponents()
        {
            lock (windowLock)
            {
                TopLevelComponent.WindowLocation = windowLoc;
                TopLevelComponent.UpdateSubcomponents(GlobalTimer.GetCurTime());
            }
        }

        

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

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            lock(windowLock)
            {
                TopLevelComponent.NotifyScroll((Vector2)e.GetPosition(this), Math.Sign(e.Delta));
            }
            base.OnMouseWheel(e);
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
            if(e.LeftButton == MouseButtonState.Released)
            {
                MouseState state = 0;
                Vector2 newLoc = (Vector2)e.GetPosition(this);
                TopLevelComponent.NotifyMouseMove(state, lastMouseMove, newLoc);
                lastMouseMove = newLoc;
                base.OnMouseMove(e);
                return;
            }
            if (lastMouseLoc.X != -1)
            {
                MouseState state = 0;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    state |= MouseState.Left | MouseState.Down;
                }

                Vector2 newLoc = (Vector2)e.GetPosition(this);
                if (TopLevelComponent.NotifyMouseMove(state, lastMouseLoc, newLoc))
                {
                    InteropFunctions.ReleaseCapture();
                    InteropFunctions.SendMessage(new WindowInteropHelper(this).Handle, 0xA1, 0x2, 0);
                }
                lastMouseMove = lastMouseLoc = newLoc;
            }
            base.OnMouseMove(e);
        }
    }
}
