﻿using GTweak.Utilities.Helpers;
using GTweak.Utilities.Helpers.Managers;
using GTweak.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GTweak.Utilities.Controls
{
    internal struct StoragePaths
    {
        internal static string Config = string.Empty;
        internal static string FolderLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GTweak");
        internal static string SystemDisk => Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
        internal static string HostsFile => Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
        internal static string PowFile => Path.Combine(FolderLocation, "UltimatePerformance.pow");
        internal static string IconBlank => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Blank.ico");
        internal static string LogFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTweak_Error.log");
        internal static string RegistryLocation => @"HKEY_CURRENT_USER\Software\GTweak";
    }

    internal sealed class SettingsRepository
    {
        [DllImport("winmm.dll")]
        internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        internal static string[] AvailableLangs = { "en", "ko", "ru", "uk" };
        internal static string[] AvailableThemes = { "Dark", "Light", "System" };

        internal static int PID = 0;
        internal static string currentRelease = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException()).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split(' ').Last().Trim();
        internal static readonly string currentName = AppDomain.CurrentDomain.FriendlyName;
        internal static readonly string currentLocation = Assembly.GetExecutingAssembly().Location;

        private static readonly Dictionary<string, Action> _settingsRefreshActions = new Dictionary<string, Action>
        {
            { "Notification", () => IsViewNotification = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "Notification", IsViewNotification) },
            { "Update", () => IsUpdateCheckRequired = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "Update", IsUpdateCheckRequired) },
            { "TopMost", () => IsTopMost = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "TopMost", IsTopMost) },
            { "Sound", () => IsPlayingSound = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "Sound", IsPlayingSound) },
            { "Volume", () => Volume = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "Volume", Volume) },
            { "Language", () => Language = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "Language", Language) },
            { "Theme", () => Theme = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "Theme", Theme) },
            { "HiddenIP", () => IsHiddenIpAddress = RegistryHelp.GetValue(StoragePaths.RegistryLocation, "HiddenIP", IsHiddenIpAddress) }
        };

        internal static readonly Dictionary<string, object> registryDefaults = new Dictionary<string, object>
        {
            { "Notification", true },
            { "Update", true },
            { "TopMost", false },
            { "Sound", true },
            { "Volume", 50 },
            { "Language", App.GettingSystemLanguage },
            { "Theme", "Dark" },
            { "HiddenIP", true }
        };

        internal static bool IsViewNotification { get => (bool)registryDefaults["Notification"]; set => registryDefaults["Notification"] = value; }
        internal static bool IsUpdateCheckRequired { get => (bool)registryDefaults["Update"]; set => registryDefaults["Update"] = value; }
        internal static bool IsTopMost { get => (bool)registryDefaults["TopMost"]; set => registryDefaults["TopMost"] = value; }
        internal static bool IsPlayingSound { get => (bool)registryDefaults["Sound"]; set => registryDefaults["Sound"] = value; }
        internal static int Volume { get => (int)registryDefaults["Volume"]; set => registryDefaults["Volume"] = value; }
        internal static string Language { get => (string)registryDefaults["Language"]; set => registryDefaults["Language"] = value; }
        internal static string Theme { get => (string)registryDefaults["Theme"]; set => registryDefaults["Theme"] = value; }
        internal static bool IsHiddenIpAddress { get => (bool)registryDefaults["HiddenIP"]; set => registryDefaults["HiddenIP"] = value; }

        internal void СheckingParameters()
        {
            bool isRegistryEmpty = false;

            foreach (string key in registryDefaults.Keys)
            {
                if (RegistryHelp.ValueExists(StoragePaths.RegistryLocation, key))
                {
                    isRegistryEmpty = true;
                    break;
                }
            }

            if (isRegistryEmpty)
            {
                foreach (var subkey in registryDefaults)
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\GTweak", subkey.Key, subkey.Value, RegistryValueKind.String);
            }
            else
            {
                foreach (Action executeUpdate in _settingsRefreshActions.Values)
                    executeUpdate();
            }
        }

        internal static void ChangingParameters<T>(T value, string subkey)
        {
            RegistryHelp.Write(Registry.CurrentUser, @"Software\GTweak", subkey, value.ToString(), RegistryValueKind.String);
            _settingsRefreshActions[subkey]();
        }

        internal static void SaveFileConfig()
        {
            if (INIManager.IsAllTempDictionaryEmpty)
                new ViewNotification().Show("", "info", "export_warning_notification");
            else
            {
                VistaSaveFileDialog vistaSaveFileDialog = new VistaSaveFileDialog
                {
                    FileName = "Config GTweak",
                    Filter = "(*.INI)|*.INI",
                    RestoreDirectory = true
                };

                if (vistaSaveFileDialog.ShowDialog() != true)
                    return;

                try
                {
                    StoragePaths.Config = Path.Combine(Path.GetDirectoryName(vistaSaveFileDialog.FileName), Path.GetFileNameWithoutExtension(vistaSaveFileDialog.FileName) + ".ini");

                    if (File.Exists(StoragePaths.Config))
                        File.Delete(StoragePaths.Config);

                    INIManager iniManager = new INIManager(StoragePaths.Config);
                    iniManager.Write("GTweak", "Author", "Greedeks");
                    iniManager.Write("GTweak", "Release", currentRelease);
                    iniManager.WriteAll(INIManager.SectionConf, INIManager.TempTweaksConf);
                    iniManager.WriteAll(INIManager.SectionIntf, INIManager.TempTweaksIntf);
                    iniManager.WriteAll(INIManager.SectionSvc, INIManager.TempTweaksSvc);
                    iniManager.WriteAll(INIManager.SectionSys, INIManager.TempTweaksSys);
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        internal static void OpenFileConfig()
        {
            VistaOpenFileDialog vistaOpenFileDialog = new VistaOpenFileDialog
            {
                Filter = "(*.INI)|*.INI",
                RestoreDirectory = true,
            };

            if (vistaOpenFileDialog.ShowDialog() == false)
                return;

            StoragePaths.Config = vistaOpenFileDialog.FileName;
            INIManager iniManager = new INIManager(StoragePaths.Config);

            if (iniManager.GetKeysOrValue("GTweak", false).Contains("Greedeks") && iniManager.GetKeysOrValue("GTweak").Contains("Release"))
            {
                if (File.ReadLines(StoragePaths.Config).Any(line => line.Contains("TglButton")) || File.ReadLines(StoragePaths.Config).Any(line => line.Contains("Slider")))
                    new ImportWindow(Path.GetFileName(vistaOpenFileDialog.FileName)).ShowDialog();
                else
                    new ViewNotification().Show("", "info", "import_empty_notification");
            }
            else
                new ViewNotification().Show("", "info", "import_warning_notification");
        }

        internal static void SelfRemoval()
        {
            try
            {
                RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\GTweak");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASAPI32");
                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Tracing\GTweak_RASMANCS");

                CommandExecutor.RunCommand($"/c taskkill /f /im \"{currentName}\" & choice /c y /n /d y /t 3 & del \"{currentLocation}\" & " +
                    @$"rd /s /q ""{StoragePaths.FolderLocation}"" & rd /s /q ""{Environment.SystemDirectory}\config\systemprofile\AppData\Local\GTweak""");
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal static void SelfReboot() => CommandExecutor.RunCommand($"/c taskkill /f /im \"{currentName}\" & choice /c y /n /d y /t 1 & start \"\" \"{currentLocation}\"");
    }
}
