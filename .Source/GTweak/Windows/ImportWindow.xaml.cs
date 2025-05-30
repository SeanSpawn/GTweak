﻿using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Animation;
using GTweak.Utilities.Helpers.Managers;
using GTweak.Utilities.Helpers.Storage;
using GTweak.Utilities.Tweaks;
using GTweak.Utilities.Tweaks.DefenderManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GTweak.Windows
{
    public partial class ImportWindow : Window
    {
        private bool isRestartNeed = false, isLogoutNeed = false, isExpRestartNeed = false;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ImportWindow(in string importedFile)
        {
            InitializeComponent();
            ImportedFile.Text = importedFile;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(OpacityProperty, FadeAnimation.FadeIn(1, 0.2));
            Progress<byte> progress = new Progress<byte>(ReportProgress);
            try { await SortByPageDate(_cancellationTokenSource.Token, progress); } catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        private void ReportProgress(byte valueProgress)
        {
            if (valueProgress == 100)
            {
                if (isExpRestartNeed)
                    ExplorerManager.Restart(new Process());

                if (isRestartNeed)
                    new ViewNotification().Show("restart");
                else if (!isRestartNeed && isLogoutNeed)
                    new ViewNotification().Show("logout");
                App.UpdateImport();
                BeginAnimation(OpacityProperty, FadeAnimation.FadeTo(0.1, () => { Close(); }));
            }
        }

        private async Task SortByPageDate(CancellationToken _token, IProgress<byte> _progress)
        {
            List<string> tempKeys = new List<string>(), tempValues = new List<string>();

            INIManager iniManager = new INIManager(StoragePaths.Config);

            for (byte i = 1; i <= 100; i++)
            {
                _token.ThrowIfCancellationRequested();

                if (i == 2 & iniManager.IsThereSection(INIManager.SectionConf))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionConf);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionConf, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
                        ConfidentialityTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));

                        isRestartNeed = NotifActionsStorage.ConfNotifActions.Any(get => get.Key == set.Tweak && get.Value == "restart");
                        isLogoutNeed = NotifActionsStorage.ConfNotifActions.Any(get => get.Key == set.Tweak && get.Value == "logout");
                    }
                }

                if (i == 30 & iniManager.IsThereSection(INIManager.SectionIntf))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionIntf);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionIntf, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
                        InterfaceTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));

                        isRestartNeed = NotifActionsStorage.IntfNotifActions.Any(get => get.Key == set.Tweak && get.Value == "restart");
                        isLogoutNeed = NotifActionsStorage.IntfNotifActions.Any(get => get.Key == set.Tweak && get.Value == "logout");
                        isExpRestartNeed = ExplorerManager.InterfBtnMapping.Any(get => get.Key == set.Tweak && get.Value == true);
                    }
                }

                if (i == 50 & iniManager.IsThereSection(INIManager.SectionSvc))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionSvc);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionSvc, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);
                        ServicesTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));
                        isRestartNeed = true;
                    }
                }

                if (i == 80 & iniManager.IsThereSection(INIManager.SectionSys))
                {
                    tempKeys.Clear(); tempValues.Clear();

                    tempKeys = iniManager.GetKeysOrValue(INIManager.SectionSys);
                    tempValues = iniManager.GetKeysOrValue(INIManager.SectionSys, false);

                    var acceptanceList = tempKeys.Zip(tempValues, (t, v) => new { Tweak = t, Value = v });

                    foreach (var set in acceptanceList)
                    {
                        await Task.Delay(700, _token);

                        if (set.Tweak.StartsWith("TglButton") && set.Tweak != "TglButton8")
                            SystemTweaks.ApplyTweaks(set.Tweak, Convert.ToBoolean(set.Value));
                        else if (set.Tweak == "TglButton8")
                        {
                            BackgroundQueue backgroundQueue = new BackgroundQueue();
                            await backgroundQueue.QueueTask(delegate
                            {
                                if (Convert.ToBoolean(set.Value))
                                    WindowsDefender.Deactivate();
                                else
                                    WindowsDefender.Activate();
                            });
                        }
                        else
                            SystemTweaks.ApplyTweaksSlider(set.Tweak, Convert.ToUInt32(set.Value));

                        isRestartNeed = NotifActionsStorage.SysNotifActions.Any(get => get.Key == set.Tweak && get.Value == "restart");
                        isLogoutNeed = NotifActionsStorage.SysNotifActions.Any(get => get.Key == set.Tweak && get.Value == "logout");
                    }
                }

                await Task.Delay(1, _token);

                _progress.Report(i);
            }
        }

    }
}

