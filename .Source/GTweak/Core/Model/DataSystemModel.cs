﻿using System.Windows;

namespace GTweak.Core.Model
{
    internal sealed class DataSystemModel
    {
        public string Name { get; set; }
        public string Data { get; set; }
        internal int BlurValue { get; set; }
        internal Visibility IpVisibility { get; set; }
    }
}
