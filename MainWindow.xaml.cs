using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExcursionTextbook.ViewModels;

namespace ExcursionTextbook;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;
    }

    private void NavTree_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;
        e.Handled = true;
        NavScrollViewer.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        });
    }

    private void NavTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is NavItem item && !item.IsChapter)
        {
            _vm.SelectSection(item.ChapterId, item.SectionId);
        }
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        SearchBorder.BorderBrush = (Brush)FindResource("SearchBarFocusBorder");
        SearchBorder.BorderThickness = new Thickness(1.5);
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SearchBorder.BorderBrush = (Brush)FindResource("SearchBarIdleBorder");
        SearchBorder.BorderThickness = new Thickness(1);
    }
}
