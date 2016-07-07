using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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

        private static void updateThread()
        {
            while(WindowList.Count > 0)
            {
                foreach(KitWindow window in WindowList)
                {
                    window.UpdateComponents();
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

            WindowList.Add(startWindow);

            startWindow.Show();

            startWindow.Width = 600;

            Thread kitUpdateThread = new Thread(updateThread);
            kitUpdateThread.Start();
            
            kitApp.Run();
        }
    } 
}
