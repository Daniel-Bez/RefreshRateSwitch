using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RefreshRateSwitch
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            Application.Run(form);
        }
    }
}