﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using Eu.EDelivery.AS4.WindowsService.SystemTray.Properties;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.WindowsService.SystemTray
{
    public partial class SystemTrayForm : Form
    {
        private readonly NotifyIcon _icon;

        public SystemTrayForm()
        {
            InitializeComponent();

            // TODO: maybe we should determine the current status of the windows service before assuming we haven't started it manually.
            _icon = new NotifyIcon
            {
                Visible = true,
                Icon = Resources.favicon,
                ContextMenu = new ContextMenu(
                    new []
                    {
                        new MenuItem("Start", OnStart),
                        new MenuItem("Open Portal", OnOpenPortal)
                    })
            };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnStart(object sender, EventArgs e)
        {
            using (var controller = new ServiceController("AS4Service"))
            {
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                }
            }

            _icon.ContextMenu
                 .MenuItems
                 .Remove(
                     _icon.ContextMenu
                          .MenuItems
                          .OfType<MenuItem>()
                          .First(m => m.Text == "Start"));

            _icon.ContextMenu
                 .MenuItems
                 .Add(index: 0, item: new MenuItem("Stop", OnStop));
        }

        private void OnStop(object sender, EventArgs e)
        {
            using (var controller = new ServiceController("AS4Service"))
            {
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }

            _icon.ContextMenu
                 .MenuItems
                 .Remove(
                     _icon.ContextMenu
                          .MenuItems
                          .OfType<MenuItem>()
                          .First(m => m.Text == "Stop"));

            _icon.ContextMenu
                 .MenuItems
                 .Add(index: 0, item: new MenuItem("Start", OnStart));
        }

        private void OnOpenPortal(object sender, EventArgs e)
        {
            // TODO: find out where the url is stored for the portal.

            string appsettingsPath = 
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
#if DEBUG
                    "appsettings.Development.json"
#else
                    "appsettings.inprocess.json"
#endif
                    );

            if (File.Exists(appsettingsPath))
            {
                string json = File.ReadAllText(appsettingsPath);
                JObject o = JObject.Parse(json);
                JToken token = o?.SelectToken("$.Port");
                var port = token?.Value<string>();
                if (port != null)
                {
                    Process.Start(port);
                }
            }
        }
    }
}
