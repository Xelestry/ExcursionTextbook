using ExcursionTextbook.Models;
using ExcursionTextbook.Services;

namespace ExcursionTextbook.ViewModels;

public class QuizViewModel : BaseViewModel
{
    private readonly List<QuizQuestion> _questions;
    private readonly ProgressService _progressService;
    private readonly int _chapterId;

    private int _currentIndex;
    private int? _selectedOptionIndex;
    private bool _answered;
    private bool _finished;
    private int _score;

    public QuizViewModel(int chapterId, List<QuizQuestion> questions, ProgressService progressService)
    {
        _chapterId = chapterId;
        _questions = questions;
        _progressService = progressService;

        AnswerCommand = new RelayCommand(_ => Answer(), _ => _selectedOptionIndex.HasValue && !_answered);
        NextCommand = new RelayCommand(_ => Next(), _ => _answered && !_finished);
    }

    public QuizQuestion CurrentQuestion => _questions[_currentIndex];
    public int QuestionNumber => _currentIndex + 1;
    public int TotalQuestions => _questions.Count;
    public bool IsFinished { get => _finished; private set => SetField(ref _finished, value); }
    public bool IsAnswered { get => _answered; private set => SetField(ref _answered, value); }
    public int Score { get => _score; private set => SetField(ref _score, value); }
    public string ResultText => $"{Score} из {TotalQuestions} правильно";
    public string ResultColor => Score >= TotalQuestions * 0.8 ? "#2E7D32"
        : Score >= TotalQuestions * 0.6 ? "#F57F17" : "#C62828";

    public int? SelectedOptionIndex
    {
        get => _selectedOptionIndex;
        set => SetField(ref _selectedOptionIndex, value);
    }

    public bool IsOptionCorrect(int index) => index == CurrentQuestion.CorrectIndex;
    public bool IsOptionWrong(int index) => _answered && index == _selectedOptionIndex && index != CurrentQuestion.CorrectIndex;

    public RelayCommand AnswerCommand { get; }
    public RelayCommand NextCommand { get; }

    private void Answer()
    {
        if (!_selectedOptionIndex.HasValue || _answered) return;
        IsAnswered = true;
        if (_selectedOptionIndex.Value == CurrentQuestion.CorrectIndex)
            Score++;
        OnPropertyChanged(nameof(IsOptionCorrect));
        OnPropertyChanged(nameof(IsOptionWrong));
    }

    private void Next()
    {
        if (_currentIndex < _questions.Count - 1)
        {
            _currentIndex++;
            _selectedOptionIndex = null;
            IsAnswered = false;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(QuestionNumber));
            OnPropertyChanged(nameof(SelectedOptionIndex));
        }
        else
        {
            _progressService.SaveQuizResult(_chapterId, Score, TotalQuestions);
            IsFinished = true;
            OnPropertyChanged(nameof(ResultText));
            OnPropertyChanged(nameof(ResultColor));
        }
    }
}
