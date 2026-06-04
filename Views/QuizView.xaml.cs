using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExcursionTextbook.ViewModels;

namespace ExcursionTextbook.Views;

public partial class QuizView : UserControl
{
    private QuizViewModel? _vm;
    private readonly List<RadioButton> _radioButtons = new();
    private int _selectedIndex = -1;

    public QuizView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is QuizViewModel old)
            old.PropertyChanged -= Vm_PropertyChanged;

        _vm = e.NewValue as QuizViewModel;

        if (_vm == null)
        {
            NullText.Visibility = Visibility.Visible;
            ContentScroll.Visibility = Visibility.Collapsed;
            return;
        }

        NullText.Visibility = Visibility.Collapsed;
        ContentScroll.Visibility = Visibility.Visible;
        _vm.PropertyChanged += Vm_PropertyChanged;
        RefreshQuestion();
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(QuizViewModel.CurrentQuestion)
                           or nameof(QuizViewModel.IsAnswered)
                           or nameof(QuizViewModel.IsFinished))
            RefreshQuestion();
    }

    private void RefreshQuestion()
    {
        if (_vm == null) return;

        if (_vm.IsFinished)
        {
            QuestionPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Visible;
            ResultLabel.Text = _vm.ResultText;
            var color = (Color)ColorConverter.ConvertFromString(_vm.ResultColor);
            var brush = new SolidColorBrush(color);
            ResultLabel.Foreground = brush;
            ResultBorder.BorderBrush = brush;
            ResultBorder.Background = new SolidColorBrush(Color.FromArgb(30, color.R, color.G, color.B));
            return;
        }

        QuestionPanel.Visibility = Visibility.Visible;
        ResultPanel.Visibility = Visibility.Collapsed;

        var q = _vm.CurrentQuestion;
        QuestionText.Text = q.Question;
        ProgressLabel.Text = $"Вопрос {_vm.QuestionNumber} из {_vm.TotalQuestions}";

        _selectedIndex = -1;
        _radioButtons.Clear();
        OptionsPanel.Children.Clear();

        var optionStyle = (Style)FindResource("QuizOptionStyle");

        for (int i = 0; i < q.Options.Count; i++)
        {
            var idx = i;

            var rb = new RadioButton
            {
                Content = q.Options[i],
                Style = optionStyle,
                GroupName = "QuizOptions",
                IsEnabled = !_vm.IsAnswered
            };

            if (_vm.IsAnswered)
            {
                if (i == q.CorrectIndex)
                {
                    rb.Background = new SolidColorBrush(Color.FromRgb(200, 240, 200));
                    rb.BorderBrush = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                }
                else if (i == _vm.SelectedOptionIndex)
                {
                    rb.Background = new SolidColorBrush(Color.FromRgb(255, 205, 210));
                    rb.BorderBrush = new SolidColorBrush(Color.FromRgb(198, 40, 40));
                }
            }

            rb.Checked += (s, e) => _selectedIndex = idx;
            _radioButtons.Add(rb);

            // Clicking anywhere on the option (not just the circle) selects it
            rb.MouseLeftButtonDown += (s, e) =>
            {
                if (!_vm.IsAnswered)
                    _selectedIndex = idx;
            };

            OptionsPanel.Children.Add(rb);
        }

        AnswerBtn.Visibility = _vm.IsAnswered ? Visibility.Collapsed : Visibility.Visible;
        NextBtn.Visibility = _vm.IsAnswered && !_vm.IsFinished ? Visibility.Visible : Visibility.Collapsed;
    }

    private void AnswerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_vm == null || _selectedIndex < 0) return;
        _vm.SelectedOptionIndex = _selectedIndex;
        _vm.AnswerCommand.Execute(null);
        RefreshQuestion();
    }

    private void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_vm == null) return;
        _vm.NextCommand.Execute(null);
        RefreshQuestion();
    }
}
