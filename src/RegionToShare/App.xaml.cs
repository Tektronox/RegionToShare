﻿using System.Windows;

namespace RegionToShare;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public App()
    {
        InitializeComponent();

        if (!RegionToShare.MainWindow.ValidateSettings())
        {
            Shutdown();
        }
    }
}