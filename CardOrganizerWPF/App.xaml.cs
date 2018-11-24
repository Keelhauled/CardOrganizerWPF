using System;
using System.Windows;
using CardOrganizerWPF.Utils;

namespace CardOrganizerWPF
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            using(var mutex = new SingleGlobalInstance(1000))
            {
                if(mutex._hasHandle)
                {
                    App app = new App();
                    app.InitializeComponent();
                    app.Run();
                }
                else
                {
                    MessageBox.Show("Instance already running", "CardOrganizer");
                }
            }
        }
    }
}
