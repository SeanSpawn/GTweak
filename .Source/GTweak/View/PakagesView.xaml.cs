﻿using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class PakagesView : UserControl
    {
        private readonly DispatcherTimer timer;
        private TimeSpan time = TimeSpan.FromSeconds(0);
        private string applicationName = string.Empty;

        public PakagesView()
        {
            InitializeComponent();

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time.TotalSeconds % 6 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += delegate { new UninstallingPakages().ViewInstalledPackages(); };
                    backgroundWorker.RunWorkerCompleted += delegate { UpdateViewStatePakages(); };
                    backgroundWorker.RunWorkerAsync();
                }

                time = time.Add(TimeSpan.FromSeconds(+1));
            }, Application.Current.Dispatcher);

            timer.Start();
        }

        private void Apps_MouseEnter(object sender, MouseEventArgs e)
        {
            string descriptionApp = (string)FindResource(((Image)sender).Name + "_applications");

            if (CommentApp.Text != descriptionApp)
                CommentApp.Text = descriptionApp;
        }

        private void Apps_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CommentApp.Text != (string)FindResource("defaultDescriptionApp"))
                CommentApp.Text = (string)FindResource("defaultDescriptionApp");
        }

        private async void ClickApp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image appImage = (Image)sender;
            applicationName = appImage.Name;

            switch (e.LeftButton)
            {
                case MouseButtonState.Pressed when appImage.Source == (DrawingImage)FindResource("A_DI_" + applicationName):
                    {
                        timer.Stop();

                        BackgroundQueue backgroundQueue = new BackgroundQueue();
                        await backgroundQueue.QueueTask(delegate { UninstallingPakages.DeletingPackage(applicationName); });

                        await backgroundQueue.QueueTask(async delegate
                        {
                            try
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    UninstallingPakages.PackagesDetails[appImage.Name] = (UninstallingPakages.PackagesDetails[appImage.Name].Alias, true, UninstallingPakages.PackagesDetails[appImage.Name].Scripts);
                                    UpdateViewStatePakages();
                                });

                                await UninstallingPakages.DeletingPackage(applicationName);

                                await Task.Delay(3000);

                                await Dispatcher.InvokeAsync(() =>
                                {
                                    UninstallingPakages.PackagesDetails[appImage.Name] = (UninstallingPakages.PackagesDetails[appImage.Name].Alias, false, UninstallingPakages.PackagesDetails[appImage.Name].Scripts);
                                    UpdateViewStatePakages();
                                });
                            }
                            catch (Exception ex) { Debug.WriteLine(ex.Message); }
                        });

                        timer.Start();
                        break;
                    }

                case MouseButtonState.Pressed when appImage.Source == (DrawingImage)FindResource("DA_DI_" + applicationName) && applicationName == "OneDrive":
                    {
                        timer.Stop();

                        new ViewNotification().Show("", "info", "onedrive_notification");

                        BackgroundQueue backgroundQueue = new BackgroundQueue();
                        await backgroundQueue.QueueTask(async delegate
                        {
                            try
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    UninstallingPakages.PackagesDetails[appImage.Name] = (UninstallingPakages.PackagesDetails[appImage.Name].Alias, true, UninstallingPakages.PackagesDetails[appImage.Name].Scripts);
                                    UpdateViewStatePakages();
                                });

                                await UninstallingPakages.ResetOneDrive();

                                await Dispatcher.InvokeAsync(() =>
                                {
                                    UninstallingPakages.PackagesDetails[appImage.Name] = (UninstallingPakages.PackagesDetails[appImage.Name].Alias, false, UninstallingPakages.PackagesDetails[appImage.Name].Scripts);
                                    UpdateViewStatePakages();
                                });
                            }
                            catch (Exception ex) { Debug.WriteLine(ex.Message); }
                        });

                        timer.Start();
                        break;
                    }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => UpdateViewStatePakages();

        private ImageSource AvailabilityInstalledPackage(string packageName, string partName, bool isOneDrive = false)
        {
            static bool isContains(string pattern) => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace).IsMatch(UninstallingPakages.InstalledPackages);

            return !isOneDrive
                ? !UninstallingPakages.PackagesDetails[packageName].IsUnavailable ? isContains(partName) ? (DrawingImage)FindResource($"A_DI_{packageName}") : (DrawingImage)FindResource($"DA_DI_{packageName}") : (DrawingImage)FindResource("DI_Sandtime")
                : !UninstallingPakages.PackagesDetails[packageName].IsUnavailable ? UninstallingPakages.IsOneDriveInstalled ? (DrawingImage)FindResource($"A_DI_{packageName}") : (DrawingImage)FindResource($"DA_DI_{packageName}") : (DrawingImage)FindResource("DI_Sandtime");
        }

        private void UpdateViewStatePakages()
        {
            MicrosoftStore.Source = AvailabilityInstalledPackage("MicrosoftStore", "Microsoft.WindowsStore");
            Todos.Source = AvailabilityInstalledPackage("Todos", "Microsoft.Todos");
            BingWeather.Source = AvailabilityInstalledPackage("BingWeather", "Microsoft.BingWeather");
            Microsoft3D.Source = AvailabilityInstalledPackage("Microsoft3D", "Microsoft.Microsoft3DViewer");
            Music.Source = AvailabilityInstalledPackage("Music", "Microsoft.ZuneMusic");
            GetHelp.Source = AvailabilityInstalledPackage("GetHelp", "Microsoft.GetHelp");
            MicrosoftOfficeHub.Source = AvailabilityInstalledPackage("MicrosoftOfficeHub", "Microsoft.MicrosoftOfficeHub");
            MicrosoftSolitaireCollection.Source = AvailabilityInstalledPackage("MicrosoftSolitaireCollection", "Microsoft.MicrosoftSolitaireCollection");
            MixedReality.Source = AvailabilityInstalledPackage("MixedReality", "Microsoft.MixedReality.Portal");
            Xbox.Source = AvailabilityInstalledPackage("Xbox", "Microsoft.XboxApp|Microsoft.GamingApp|Microsoft.XboxGamingOverlay|Microsoft.XboxGameOverlay|Microsoft.XboxIdentityProvider|Microsoft.Xbox.TCUI|Microsoft.XboxSpeechToTextOverlay");
            Paint3D.Source = AvailabilityInstalledPackage("Paint3D", "Microsoft.MSPaint");
            OneNote.Source = AvailabilityInstalledPackage("OneNote", "Microsoft.Office.OneNote");
            People.Source = AvailabilityInstalledPackage("People", "Microsoft.People");
            MicrosoftStickyNotes.Source = AvailabilityInstalledPackage("MicrosoftStickyNotes", "Microsoft.MicrosoftStickyNotes");
            Widgets.Source = AvailabilityInstalledPackage("Widgets", "MicrosoftWindows.Client.WebExperience|Microsoft.WidgetsPlatformRuntime");
            ScreenSketch.Source = AvailabilityInstalledPackage("ScreenSketch", "Microsoft.ScreenSketch");
            Phone.Source = AvailabilityInstalledPackage("Phone", "Microsoft.YourPhone|MicrosoftWindows.CrossDevice");
            Photos.Source = AvailabilityInstalledPackage("Photos", "Microsoft.Windows.Photos");
            FeedbackHub.Source = AvailabilityInstalledPackage("FeedbackHub", "Microsoft.WindowsFeedbackHub");
            SoundRecorder.Source = AvailabilityInstalledPackage("SoundRecorder", "Microsoft.WindowsSoundRecorder");
            Alarms.Source = AvailabilityInstalledPackage("Alarms", "Microsoft.WindowsAlarms");
            SkypeApp.Source = AvailabilityInstalledPackage("SkypeApp", "Microsoft.SkypeApp");
            Maps.Source = AvailabilityInstalledPackage("Maps", "Microsoft.WindowsMaps");
            Camera.Source = AvailabilityInstalledPackage("Camera", "Microsoft.WindowsCamera");
            Video.Source = AvailabilityInstalledPackage("Video", "Microsoft.ZuneVideo");
            BingNews.Source = AvailabilityInstalledPackage("BingNews", "Microsoft.BingNews");
            Mail.Source = AvailabilityInstalledPackage("Mail", "microsoft.windowscommunicationsapps");
            MicrosoftTeams.Source = AvailabilityInstalledPackage("MicrosoftTeams", "MicrosoftTeams|MSTeams");
            PowerAutomateDesktop.Source = AvailabilityInstalledPackage("PowerAutomateDesktop", "Microsoft.PowerAutomateDesktop");
            Cortana.Source = AvailabilityInstalledPackage("Cortana", "Microsoft.549981C3F5F10");
            ClipChamp.Source = AvailabilityInstalledPackage("ClipChamp", "Clipchamp.Clipchamp");
            GetStarted.Source = AvailabilityInstalledPackage("GetStarted", "Microsoft.Getstarted");
            OneDrive.Source = AvailabilityInstalledPackage("OneDrive", "OneDrive", true);
            BingSports.Source = AvailabilityInstalledPackage("BingSports", "Microsoft.BingSports");
            BingFinance.Source = AvailabilityInstalledPackage("BingFinance", "Microsoft.BingFinance");
            MicrosoftFamily.Source = AvailabilityInstalledPackage("MicrosoftFamily", "MicrosoftCorporationII.MicrosoftFamily");
            BingSearch.Source = AvailabilityInstalledPackage("BingSearch", "Microsoft.BingSearch");
            Outlook.Source = AvailabilityInstalledPackage("Outlook", "Microsoft.OutlookForWindows");
            QuickAssist.Source = AvailabilityInstalledPackage("QuickAssist", "MicrosoftCorporationII.QuickAssist");
            DevHome.Source = AvailabilityInstalledPackage("DevHome", "Microsoft.Windows.DevHome");
            WindowsTerminal.Source = AvailabilityInstalledPackage("WindowsTerminal", "Microsoft.WindowsTerminal");
            LinkedIn.Source = AvailabilityInstalledPackage("LinkedIn", "Microsoft.LinkedIn");
            WebMediaExtensions.Source = AvailabilityInstalledPackage("WebMediaExtensions", "Microsoft.WebMediaExtensions");
            OneConnect.Source = AvailabilityInstalledPackage("OneConnect", "Microsoft.OneConnect");
            Edge.Source = AvailabilityInstalledPackage("Edge", "Microsoft.MicrosoftEdge.Stable");
        }
    }
}
