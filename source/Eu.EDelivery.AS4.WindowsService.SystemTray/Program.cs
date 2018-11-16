﻿using System;
using System.Windows.Forms;

namespace Eu.EDelivery.AS4.WindowsService.SystemTray
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SystemTrayForm());
        }
    }
}
