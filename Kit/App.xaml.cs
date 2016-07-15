using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Kit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<KitWindow> WindowList { get; set; }

        public static object WindowListLock = new object();

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
                Thread.Sleep(16);
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Core.GlobalTimer.StartTimer();
            WindowList = new List<KitWindow>();
            App kitApp = new App();
            KitWindow startWindow = new KitWindow();
            startWindow.Width = 700;
            startWindow.Height = 324;
            WindowList.Add(startWindow);
            startWindow.Show();

            Thread kitUpdateThread = new Thread(updateThread);
            kitUpdateThread.Start();

            kitApp.Run();
        }
    }
}
