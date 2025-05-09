﻿using GTweak.Utilities.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace GTweak.Windows
{
    /// <summary>
    /// Darkened screen 
    /// </summary>

    public partial class WaitingWindow
    {
        private readonly DisablingWinKeys disablingWinKeys = new DisablingWinKeys();
        public WaitingWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.15));
            doubleAnim.Completed += delegate
            {
                ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
                disablingWinKeys.objKeyboardProcess = new DisablingWinKeys.LowLevelKeyboardProc(disablingWinKeys.CaptureKey);
                disablingWinKeys.ptrHook = DisablingWinKeys.SetWindowsHookEx(13, disablingWinKeys.objKeyboardProcess, DisablingWinKeys.GetModuleHandle(objCurrentModule.ModuleName), 1);
                Close();
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            BeginAnimation(OpacityProperty, doubleAnim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            disablingWinKeys.objKeyboardProcess = new DisablingWinKeys.LowLevelKeyboardProc(disablingWinKeys.CaptureKey);
            disablingWinKeys.ptrHook = DisablingWinKeys.SetWindowsHookEx(13, disablingWinKeys.objKeyboardProcess, DisablingWinKeys.GetModuleHandle(objCurrentModule.ModuleName), 0);

            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 0.5,
                SpeedRatio = 2,
                EasingFunction = new PowerEase(),
                Duration = TimeSpan.FromSeconds(0.3)
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 240);
            BeginAnimation(OpacityProperty, doubleAnim);
        }
    }
}
