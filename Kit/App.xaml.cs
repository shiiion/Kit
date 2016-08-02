using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Kit.Graphics.Components;
using Kit.Graphics.Drawing;
using Kit.Graphics.Types;
using Kit.Core;
using System.Windows.Media;

//  QUADRATIC INTERPOLATION FOR ANIMATION
//  ANIMATIONS CAN BE APPLIED TO ANY KITCOMPONENT

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

            KitTextAnimation kta1 = new KitTextAnimation("option 1", "Consolas", 12)
            {
                TextColor = Colors.Red,
                Location = new Vector2(100, 100)
            };
            KitTextAnimation kta2 = new KitTextAnimation("option 2", "Consolas", 12)
            {
                TextColor = Colors.Red,
                Location = new Vector2(100, 120)
            };
            KitTextAnimation kta3 = new KitTextAnimation("option 3", "Consolas", 12)
            {
                TextColor = Colors.Red,
                Location = new Vector2(100, 140)
            };
            KitTextAnimation kta4 = new KitTextAnimation("option 4", "Consolas", 12)
            {
                TextColor = Colors.Red,
                Location = new Vector2(100, 160)
            };

            kta1.SetFade(200, 180);
            kta1.SetMovement(200, 150, new Vector2(100, 80), new Vector2(100, 100));
            kta2.SetFade(320, 180);
            kta2.SetMovement(320, 150, new Vector2(100, 100), new Vector2(100, 120));
            kta3.SetFade(440, 180);
            kta3.SetMovement(440, 150, new Vector2(100, 120), new Vector2(100, 140));
            kta4.SetFade(560, 180);
            kta4.SetMovement(560, 150, new Vector2(100, 140), new Vector2(100, 160));


            startWindow.TopLevelComponent.AddChild(kta1);
            startWindow.TopLevelComponent.AddChild(kta2);
            startWindow.TopLevelComponent.AddChild(kta3);
            startWindow.TopLevelComponent.AddChild(kta4);
            startWindow.Width = 700;
            startWindow.Height = 324;
            WindowList.Add(startWindow);
            startWindow.ClickableBackground = true;
            startWindow.Show();
            kta1.StartAnimation();
            kta2.StartAnimation();
            kta3.StartAnimation();
            kta4.StartAnimation();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Core.GlobalTimer.StartTimer();
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
