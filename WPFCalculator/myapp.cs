using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
/* Error	1	The type 'System.Windows.Markup.IQueryAmbient' is defined in an
 * assembly that is not referenced. You must add a reference to assembly
 * 'System.Xaml, Version=4.0.0.0, Culture=neutral, 
 * PublicKeyToken=b77a5c561934e089'.*/
//using System.Xaml;

namespace Piannaf.Ports.Microsoft.Samples.WPF.Calculator
{
    public class app : Application
    {
        [STAThread]
        public static void Main()
        {
            app wpfcalculator = new app();
            wpfcalculator.Run();
        }

        app()
        {
            this.Startup += new StartupEventHandler(AppStartingUp);
        }

        void AppStartingUp(object sender, StartupEventArgs e)
        {
            Window1 mainWindow = new Window1();
            mainWindow.Show();
        }
    }
}