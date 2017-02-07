using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace UnwindTicket
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new OpenIdeas());
          
            string[] args = Environment.GetCommandLineArgs();
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);

        }
    }

      public class SingleInstanceController : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            // Set whether the application is single instance
            this.IsSingleInstance = true;

            this.StartupNextInstance += new
              StartupNextInstanceEventHandler(this_StartupNextInstance);
        }

        void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            // Here you get the control when any other instance is
            // invoked apart from the first one.
            // You have args here in e.CommandLine.
          
            
            // You custom code which should be run on other instances
            OpenIdeas frm = MainForm as OpenIdeas;
            frm.Show();
        }

        protected override void OnCreateMainForm()
        {
          
            // Instantiate your main application form
            this.MainForm = new OpenIdeas();
        }
    }


}
