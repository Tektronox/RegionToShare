using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RegionToShare;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
