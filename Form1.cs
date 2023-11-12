﻿using Microsoft.Win32;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace MemoryUsageMonitor
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            HideForm();
            SetStartupToolItem();
            UpdateIcon();
            timer1.Interval = 5000;
            timer1.Start();
        }

        void HideForm()
        {
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
        }

        void SetStartupToolItem()
        {
            string key = "MemoryUsageMonitor";

            startupToolStripMenuItem.CheckedChanged -= startupToolStripMenuItem_CheckedChanged;

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue(key) == null)
            {
                startupToolStripMenuItem.Checked = false;
            }
            else
            {
                startupToolStripMenuItem.Checked = true;
            }

            startupToolStripMenuItem.CheckedChanged += startupToolStripMenuItem_CheckedChanged;
        }

        void UpdateIcon()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();


            if (memoryValues != null)
            {
                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                var percentRound = Math.Round(percent, 0, MidpointRounding.AwayFromZero).ToString();

                Color color = new Color();
                if(percent < 79.5)
                {
                    color = Color.LightGreen;
                }
                else if(percent < 89.5)
                {
                    color = Color.Orange;
                }
                else
                {
                    color = Color.Red;
                }

                Brush brush = new SolidBrush(color);
                Font drawFont = new Font("Arial", 10, FontStyle.Bold);

                Bitmap bitmap = new Bitmap(16, 16);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawString(percentRound, drawFont, brush, 0, 0);

                Icon icon = Icon.FromHandle(bitmap.GetHicon());

                notifyIcon1.Icon = icon;
                Console.WriteLine(percent);
            }

        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
        }

        void UpdateRegistrySettings(bool launchAtStartUp)
        {
            string key = "MemoryUsageMonitor";

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (launchAtStartUp)
            {
                rk.SetValue(key, "\"" + Application.ExecutablePath.ToString() + "\"");
            }
            else
            {
                rk.DeleteValue("MemoryUsageMonitor");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateIcon();
        }

        private void startupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRegistrySettings(startupToolStripMenuItem.Checked);
        }
    }
}