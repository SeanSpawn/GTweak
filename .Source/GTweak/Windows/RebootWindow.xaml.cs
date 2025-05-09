﻿using GTweak.Utilities.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Windows
{
    public partial class RebootWindow
    {
        public RebootWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 100,
                EasingFunction = new QuadraticEase(),
                Duration = TimeSpan.FromSeconds(1.5)
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            doubleAnim.Completed += delegate { SettingsRepository.SelfReboot(); };
            RestartProgress.BeginAnimation(ProgressBar.ValueProperty, doubleAnim);
        }
    }
}
