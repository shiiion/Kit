using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Kit.Graphics.Components;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using Kit.Core;
using System.Windows.Media;

namespace Kit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static List<KitWindow> WindowList { get; set; }
        public static List<KitWindow> RemoveList { get; set; }
        public static List<KitWindow> AddList { get; set; }

        public static object WindowListLock = new object();
        public static object RemoveLock = new object();
        public static object AddObject = new object();

        private static void updateThread()
        {
            while (WindowList.Count > 0 || AddList.Count > 0)
            {
                lock (WindowListLock)
                {
                    foreach (KitWindow window in WindowList)
                    {
                        window.UpdateComponents();
                    }
                }

                lock (RemoveLock)
                {
                    foreach (KitWindow window in RemoveList)
                    {
                        WindowList.Remove(window);
                    }
                    RemoveList.Clear();
                }

                lock (AddObject)
                {
                    foreach (KitWindow window in AddList)
                    {
                        WindowList.Add(window);
                    }
                    AddList.Clear();
                }
                Thread.Sleep(16);
            }
        }

        private static void createStartupWindow()
        {
            KitWindow startWindow = new KitWindow("Test Window");

            KitButton kb1 = new KitButton("zz~~", "Consolas", 20, Colors.Black, Color.FromArgb(0xff, 0xb5, 0x5d, 0x7a), Colors.PaleVioletRed, new Vector2(2, 2), 3)
            {
                Anchor = KitAnchoring.TopRight,
                Origin = KitAnchoring.TopRight
            };
            KitButton kb2 = new KitButton("zzg 2~~", "Consolas", 20, Colors.Black, Color.FromArgb(0xff, 0x7a, 0x5d, 0xb5), Colors.MediumPurple, new Vector2(2, 2), 3)
            {
                Anchor = KitAnchoring.TopRight,
                Origin = KitAnchoring.TopRight
            };
            KitButton kb3 = new KitButton("fghdfg 3~~", "Consolas", 20, Colors.Black, Color.FromArgb(0xff, 0x8c, 0xd4, 0x8c), Color.FromArgb(0xff, 0xa6, 0xfb, 0xa6), new Vector2(2, 2), 3)
            {
                Anchor = KitAnchoring.TopRight,
                Origin = KitAnchoring.TopRight
            };
            KitButton kb4 = new KitButton("r67im~~", "Consolas", 20, Colors.Black, Color.FromArgb(0xff, 0xd9, 0xd9, 0x91), Color.FromArgb(0xff, 0xff, 0xff, 0xaa), new Vector2(2, 2), 3)
            {
                Anchor = KitAnchoring.TopRight,
                Origin = KitAnchoring.TopRight
            };
            KitButton kb5 = new KitButton("ofgh~~", "Consolas", 20, Colors.Black, Color.FromArgb(0xff, 0xd9, 0xd9, 0xd9), Colors.White, new Vector2(2, 2), 3)
            {
                Anchor = KitAnchoring.TopRight,
                Origin = KitAnchoring.TopRight
            };

            kb1.SetFade(500, 800, "beginFade", true, KitEasingMode.EaseOut, KitEasingType.Quad);
            kb1.SetMovement(500, 800, new Vector2(kb1.Size.X, 22), new Vector2(20, 22), "beginMove", KitEasingMode.EaseOut, KitEasingType.Expo);
            kb2.SetFade(540, 800, "beginFade", true, KitEasingMode.EaseOut, KitEasingType.Quad);
            kb2.SetMovement(540, 800, new Vector2(kb2.Size.X, 22 + kb1.Size.Y), new Vector2(20, 22 + kb1.Size.Y), "beginMove", KitEasingMode.EaseOut, KitEasingType.Expo);
            kb3.SetFade(580, 800, "beginFade", true, KitEasingMode.EaseOut, KitEasingType.Quad);
            kb3.SetMovement(580, 800, new Vector2(kb3.Size.X, 22 + 2 * kb1.Size.Y), new Vector2(20, 22 + 2 * kb1.Size.Y), "beginMove", KitEasingMode.EaseOut, KitEasingType.Expo);
            kb4.SetFade(620, 800, "beginFade", true, KitEasingMode.EaseOut, KitEasingType.Quad);
            kb4.SetMovement(620, 800, new Vector2(kb4.Size.X, 22 + 3 * kb1.Size.Y), new Vector2(20, 22 + 3 * kb1.Size.Y), "beginMove", KitEasingMode.EaseOut, KitEasingType.Expo);
            kb5.SetFade(660, 800, "beginFade", true, KitEasingMode.EaseOut, KitEasingType.Quad);
            kb5.SetMovement(660, 800, new Vector2(kb5.Size.X, 22 + 4 * kb1.Size.Y), new Vector2(20, 22 + 4 * kb1.Size.Y), "beginMove", KitEasingMode.EaseOut, KitEasingType.Expo);


            startWindow.TopLevelComponent.AddChild(kb1);
            startWindow.TopLevelComponent.AddChild(kb2);
            startWindow.TopLevelComponent.AddChild(kb3);
            startWindow.TopLevelComponent.AddChild(kb4);
            startWindow.TopLevelComponent.AddChild(kb5);

            startWindow.Width = 700;
            startWindow.Height = 322;

            lock (AddObject)
            {
                AddList.Add(startWindow);
            }

            startWindow.ClickableBackground = true;
            startWindow.Show();

            kb1.StartAnimation();
            kb2.StartAnimation();
            kb3.StartAnimation();
            kb4.StartAnimation();
            kb5.StartAnimation();

            kb4.Released += () =>
            {
                kb1.StartAnimation();
                kb2.StartAnimation();
                kb3.StartAnimation();
                kb4.StartAnimation();
                kb5.StartAnimation();
            };

            kb1.Released += () =>
            {
                createStartupWindow();
            };

            kb1.Update += (c) =>
            {
                if (!c.MovementControl.AnimationOver())
                    return;
                if (kb1.ContainsCursor() && (kb1.MovementControl.AnimationTag.Equals("moveBack") || kb1.MovementControl.AnimationTag.Equals("beginMove")))
                {
                    kb1.SetMovement(0, 100, new Vector2(20, 22), new Vector2(10, 22), "moveHover", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb1.StartAnimation(true, false);
                }
                else if (!kb1.ContainsCursor() && kb1.MovementControl.AnimationTag.Equals("moveHover"))
                {
                    kb1.SetMovement(0, 100, new Vector2(10, 22), new Vector2(20, 22), "moveBack", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb1.StartAnimation(true, false);
                }
            };
            kb2.Update += (c) =>
            {
                if (!c.MovementControl.AnimationOver())
                    return;
                if (kb2.ContainsCursor() && (kb2.MovementControl.AnimationTag.Equals("moveBack") || kb2.MovementControl.AnimationTag.Equals("beginMove")))
                {
                    kb2.SetMovement(0, 100, new Vector2(20, kb2.Location.Y), new Vector2(10, kb2.Location.Y), "moveHover", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb2.StartAnimation(true, false);
                }
                else if (!kb2.ContainsCursor() && kb2.MovementControl.AnimationTag.Equals("moveHover"))
                {
                    kb2.SetMovement(0, 100, new Vector2(10, kb2.Location.Y), new Vector2(20, kb2.Location.Y), "moveBack", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb2.StartAnimation(true, false);
                }
            };
            kb3.Update += (c) =>
            {
                if (!c.MovementControl.AnimationOver())
                    return;
                if (kb3.ContainsCursor() && (kb3.MovementControl.AnimationTag.Equals("moveBack") || kb3.MovementControl.AnimationTag.Equals("beginMove")))
                {
                    kb3.SetMovement(0, 100, new Vector2(20, kb3.Location.Y), new Vector2(10, kb3.Location.Y), "moveHover", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb3.StartAnimation(true, false);
                }
                else if (!kb3.ContainsCursor() && kb3.MovementControl.AnimationTag.Equals("moveHover"))
                {
                    kb3.SetMovement(0, 100, new Vector2(10, kb3.Location.Y), new Vector2(20, kb3.Location.Y), "moveBack", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb3.StartAnimation(true, false);
                }
            };
            kb4.Update += (c) =>
            {
                if (!c.MovementControl.AnimationOver())
                    return;
                if (kb4.ContainsCursor() && (kb4.MovementControl.AnimationTag.Equals("moveBack") || kb4.MovementControl.AnimationTag.Equals("beginMove")))
                {
                    kb4.SetMovement(0, 100, new Vector2(20, kb4.Location.Y), new Vector2(10, kb4.Location.Y), "moveHover", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb4.StartAnimation(true, false);
                }
                else if (!kb4.ContainsCursor() && kb4.MovementControl.AnimationTag.Equals("moveHover"))
                {
                    kb4.SetMovement(0, 100, new Vector2(10, kb4.Location.Y), new Vector2(20, kb4.Location.Y), "moveBack", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb4.StartAnimation(true, false);
                }
            };
            kb5.Update += (c) =>
            {
                if (!c.MovementControl.AnimationOver())
                    return;
                if (kb5.ContainsCursor() && (kb5.MovementControl.AnimationTag.Equals("moveBack") || kb5.MovementControl.AnimationTag.Equals("beginMove")))
                {
                    kb5.SetMovement(0, 100, new Vector2(20, kb5.Location.Y), new Vector2(10, kb5.Location.Y), "moveHover", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb5.StartAnimation(true, false);
                }
                else if (!kb5.ContainsCursor() && kb5.MovementControl.AnimationTag.Equals("moveHover"))
                {
                    kb5.SetMovement(0, 100, new Vector2(10, kb5.Location.Y), new Vector2(20, kb5.Location.Y), "moveBack", KitEasingMode.EaseOut, KitEasingType.Quad);
                    kb5.StartAnimation(true, false);
                }
            };
        }

        public static void RunCmdLine(string cmd)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.FileName = "cmd.exe";
            psi.Arguments = "/C " + cmd;
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            GlobalTimer.StartTimer();
            WindowList = new List<KitWindow>();
            RemoveList = new List<KitWindow>();
            AddList = new List<KitWindow>();
            App kitApp = new App();
            createStartupWindow();
            Thread kitUpdateThread = new Thread(updateThread);
            kitUpdateThread.Start();

            kitApp.Run();
        }
    }
}
