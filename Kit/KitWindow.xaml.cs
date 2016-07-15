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

            TopLevelComponent = new TopLevelComponent(this, "Kit", () =>
            {
                lock (App.WindowListLock)
                {
                    App.WindowList.Remove(this);
                    Close();
                }
            });
            componentBrush = new KitBrush();
            SizeChanged += (o, e) =>
            {
                TopLevelComponent.Size = new Vector2(e.NewSize.Width, e.NewSize.Height);
                TopLevelComponent.Redraw = true;
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

            KitTextArea area = new KitTextArea("Consolas", 12, new Vector2(340, 300))
            {
                Location = new Vector2(5, 22)
            };

            KitButton button = new KitButton(@"D:\jpeg\compile.png", @"D:\jpeg\compiledown.png", new Vector2(50, 50))
            {
                Anchor = KitAnchoring.RightCenter,
                Origin = KitAnchoring.LeftCenter,
                Location = new Vector2(2)
            };

            KitText compileWindow = new KitText("", "Comic Sans MS Bold")
            {
                Anchor = KitAnchoring.TopRight,
                Origin = KitAnchoring.TopLeft,
                Location = new Vector2(50 + 2 + 10),
                TextColor = Colors.Red
            };

            button.Released += () =>
            {
                string ret = "";
                try
                {
                    object rv = CSCodeCompiler.ExecuteCode(area.TextField.Text, "mom", "dad", "start", true, null);
                    if(rv is string)
                    {
                        ret = rv as string;
                    }
                }
                catch(Core.Exceptions.CompilerErrorException e)
                {
                    compileWindow.Text = e.Message;
                    return;
                }
                compileWindow.Text = ret;
            };

            TopLevelComponent.AddChild(area);
            area.AddChild(button);
            area.AddChild(compileWindow);
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
