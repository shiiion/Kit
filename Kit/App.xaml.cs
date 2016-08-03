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
        public static List<KitWindow> WindowList { get; set; }
        public static List<KitWindow> RemoveList { get; set; }

        public static object WindowListLock = new object();
        public static object RemoveLock = new object();

        private static void updateThread()
        {
            while (WindowList.Count > 0)
            {
                lock (WindowListLock)
                {
                    foreach (KitWindow window in WindowList)
                    {
                        window.UpdateComponents();
                    }
                }

                lock(RemoveLock)
                {
                    foreach (KitWindow window in RemoveList)
                    {
                        WindowList.Remove(window);
                    }
                    RemoveList.Clear();
                }
                Thread.Sleep(16);
            }
        }

        private static void createStartupWindow()
        {
            KitWindow startWindow = new KitWindow("Test Window");

            KitButton kb1 = new KitButton("do something", "Consolas", 12, Colors.Black, Colors.DarkGray, Colors.White, new Vector2(2, 2), 3);
            KitButton kb2 = new KitButton("button 2", "Consolas", 12, Colors.Black, Colors.DarkGray, Colors.White, new Vector2(2, 2), 3);
            KitButton kb3 = new KitButton("button 3", "Consolas", 12, Colors.Black, Colors.DarkGray, Colors.White, new Vector2(2, 2), 3);
            KitButton kb4 = new KitButton("animate again", "Consolas", 12, Colors.Black, Colors.DarkGray, Colors.White, new Vector2(2, 2), 3);

            kb1.SetFade(0, 800, true, KitEasingMode.EaseOut, KitEasingType.Cubic);
            kb1.SetMovement(0, 800, new Vector2(80, 80), new Vector2(100, 100), KitEasingMode.EaseOut, KitEasingType.Expo);
            kb2.SetFade(60, 800, true, KitEasingMode.EaseOut, KitEasingType.Cubic);
            kb2.SetMovement(60, 800, new Vector2(100, 100), new Vector2(120, 120), KitEasingMode.EaseOut, KitEasingType.Expo);
            kb3.SetFade(120, 800, true, KitEasingMode.EaseOut, KitEasingType.Cubic);
            kb3.SetMovement(120, 800, new Vector2(120, 120), new Vector2(120, 140), KitEasingMode.EaseOut, KitEasingType.Expo);
            kb4.SetFade(180, 800, true, KitEasingMode.EaseOut, KitEasingType.Cubic);
            kb4.SetMovement(180, 800, new Vector2(120, 140), new Vector2(100, 160), KitEasingMode.EaseOut, KitEasingType.Expo);


            startWindow.TopLevelComponent.AddChild(kb1);
            startWindow.TopLevelComponent.AddChild(kb2);
            startWindow.TopLevelComponent.AddChild(kb3);
            startWindow.TopLevelComponent.AddChild(kb4);
            startWindow.Width = 700;
            startWindow.Height = 324;
            WindowList.Add(startWindow);
            startWindow.ClickableBackground = true;
            startWindow.Show();
            kb1.StartAnimation();
            kb2.StartAnimation();
            kb3.StartAnimation();
            kb4.StartAnimation();

            kb4.Released += () =>
            {
                kb1.StartAnimation();
                kb2.StartAnimation();
                kb3.StartAnimation();
                kb4.StartAnimation();
            };

            kb1.Released += () =>
            {
                RunCmdLine("start notepad");
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
            App kitApp = new App();
            createStartupWindow();
            Thread kitUpdateThread = new Thread(updateThread);
            kitUpdateThread.Start();

            kitApp.Run();
        }
    }
}
