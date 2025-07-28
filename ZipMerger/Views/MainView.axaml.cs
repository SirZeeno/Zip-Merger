using Avalonia.Controls;

namespace ZipMerger.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        FileHandler.MainView = this;
    }
    
    public void ScrollConsoleToEnd()
    {
        if (ConsoleBox.Text != null) ConsoleBox.CaretIndex = ConsoleBox.Text.Length;
    }
}