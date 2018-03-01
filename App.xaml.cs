using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace DirectoryPositioner {
    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);  
        }
    }



    public class SingleInstanceManager : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    {
        App app;
        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            // First time app is launched
            app = new App();
            app.Run();
            return false;
        }


        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {

            base.OnStartupNextInstance(eventArgs);
            app.Activate();
        }
    }



    /// <summary>

    /// App.xaml 的交互逻辑

    /// </summary>

    public partial class App : Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow mw = new MainWindow();
            mw.Show();
        }



        public void Activate()
        {
            var mainWindow = this.MainWindow as MainWindow;
            mainWindow.ForceShow();
        }
    }
}
