﻿using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using GTweak.Windows;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace GTweak.View
{
    public partial class MoreView : UserControl
    {
        public MoreView()
        {
            InitializeComponent();
        }

        private void BtnLicenseWindows_ClickButton(object sender, EventArgs e)
        {
            if (WindowsLicense.IsWindowsActivated)
                new ViewNotification().Show("", "info", "readyactivate_notification");
            else
            {
                new ViewNotification().Show("", "warn", "activatewin_notification");
                WindowsLicense.StartActivation();
            }
        }

        private async void BtnRestorePoint_ClickButton(object sender, EventArgs e)
        {
            WaitingWindow waitingWindow = new WaitingWindow();
            waitingWindow.Show();
            new ViewNotification().Show("", "info", "createpoint_notification");
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate { SystemMaintenance.CreateRestorePoint(); });
            waitingWindow.Close();
        }

        private void BtnRecoveyLaunch_ClickButton(object sender, EventArgs e) => SystemMaintenance.StartRecovery();

        private void BtnClear_ClickButton(object sender, EventArgs e) => new ClearingMemory().StartMemoryCleanup();

        private void BtnDisableDefrag_ClickButton(object sender, EventArgs e) => SystemMaintenance.DisableDefrag();

        private async void BtnDisableRecovery_ClickButton(object sender, EventArgs e)
        {
            BackgroundQueue backgroundQueue = new BackgroundQueue();
            await backgroundQueue.QueueTask(delegate
            {
                try { SystemMaintenance.DisableRestorePoint(); } catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
            await backgroundQueue.QueueTask(delegate { new ViewNotification(300).Show("", "info", "disable_recovery_notification"); });
        }
    }
}
