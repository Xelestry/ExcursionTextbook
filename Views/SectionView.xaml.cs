using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ExcursionTextbook.ViewModels;

namespace ExcursionTextbook.Views;

public partial class SectionView : UserControl
{
    public SectionView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
            oldVm.PropertyChanged -= Vm_PropertyChanged;
        if (e.NewValue is MainViewModel vm)
        {
            vm.PropertyChanged += Vm_PropertyChanged;
            Refresh(vm);
        }
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        switch (e.PropertyName)
        {
            case nameof(MainViewModel.CurrentSection):
            case nameof(MainViewModel.CurrentChapter):
                Refresh(vm);
                break;
            case nameof(MainViewModel.IsBookmarked):
                UpdateBookmarkButton(vm);
                break;
            case nameof(MainViewModel.IsCurrentSectionRead):
                UpdateReadButton(vm);
                break;
        }
    }

    private void Refresh(MainViewModel vm)
    {
        var section = vm.CurrentSection;
        if (section == null)
        {
            EmptyStateText.Visibility = Visibility.Visible;
            ContentScroll.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyStateText.Visibility = Visibility.Collapsed;
        ContentScroll.Visibility = Visibility.Visible;

        SectionTitle.Text = section.Title;
        ChapterSubtitle.Text = vm.CurrentChapter?.Title ?? string.Empty;
        ContentText.Text = section.Content;

        UpdateReadButton(vm);
        UpdateBookmarkButton(vm);

        if (!string.IsNullOrWhiteSpace(section.ImagePath))
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, section.ImagePath);
            if (File.Exists(fullPath))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(fullPath);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                SectionImage.Source = bmp;
                SectionImage.Visibility = Visibility.Visible;
            }
            else
            {
                SectionImage.Source = null;
                SectionImage.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            SectionImage.Source = null;
            SectionImage.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateReadButton(MainViewModel vm)
    {
        MarkReadButton.Content = vm.IsCurrentSectionRead
            ? "✓ Отметить как непрочитанное"
            : "✓ Отметить как прочитанное";
    }

    private void UpdateBookmarkButton(MainViewModel vm)
    {
        BookmarkButton.Content = vm.IsBookmarked ? "★ Убрать закладку" : "☆ Добавить закладку";
    }

    private void MarkReadButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ToggleReadCommand.Execute(null);
    }

    private void BookmarkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ToggleBookmarkCommand.Execute(null);
    }
}
