using ExcursionTextbook.Models;
using ExcursionTextbook.Services;

namespace ExcursionTextbook.ViewModels;

public enum PanelMode { Section, Quiz, Glossary, Bookmarks, Search }

public class NavItem : BaseViewModel
{
    public int ChapterId { get; set; }
    public int SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsChapter { get; set; }
    public List<NavItem> Children { get; set; } = new();

    private bool _isRead;
    public bool IsRead { get => _isRead; set => SetField(ref _isRead, value); }

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }
}

public class MainViewModel : BaseViewModel
{
    private readonly ContentService _contentService;
    private readonly ProgressService _progressService;
    private readonly SearchService _searchService;

    private Section? _currentSection;
    private Chapter? _currentChapter;
    private PanelMode _panelMode = PanelMode.Section;
    private QuizViewModel? _quizViewModel;
    private string _searchQuery = string.Empty;
    private List<SearchResult> _searchResults = new();
    private bool _isBookmarked;
    private int _totalSections;
    private bool _isDarkTheme;

    public MainViewModel()
    {
        _contentService = new ContentService();
        _progressService = new ProgressService();
        _searchService = new SearchService(_contentService);

        _isDarkTheme = _progressService.Theme == "Dark";

        BuildNavigation();

        ToggleReadCommand = new RelayCommand(ToggleRead, () => _currentSection != null && _currentChapter != null);
        ToggleBookmarkCommand = new RelayCommand(ToggleBookmark, () => _currentSection != null);
        StartQuizCommand = new RelayCommand(StartQuiz, () => _currentChapter?.Quiz?.Count > 0);
        ShowGlossaryCommand = new RelayCommand(_ => PanelMode = PanelMode.Glossary);
        ShowBookmarksCommand = new RelayCommand(_ => PanelMode = PanelMode.Bookmarks);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        NavigateSearchResultCommand = new RelayCommand(NavigateSearchResult);
        NavigateBookmarkCommand = new RelayCommand(NavigateBookmark);
    }

    public List<NavItem> NavItems { get; private set; } = new();
    public List<Chapter> Chapters => _contentService.GetChapters();
    public List<Bookmark> Bookmarks => _progressService.GetBookmarks();

    public List<GlossaryTerm> AllGlossaryTerms => _contentService.GetChapters()
        .SelectMany(c => c.GlossaryTerms)
        .OrderBy(t => t.Term)
        .ToList();

    public List<IGrouping<string, GlossaryTerm>> GlossaryGroups =>
        AllGlossaryTerms
            .GroupBy(t => t.Term.Length > 0 ? t.Term[0].ToString().ToUpper() : "#")
            .OrderBy(g => g.Key)
            .ToList();

    public Section? CurrentSection { get => _currentSection; private set => SetField(ref _currentSection, value); }
    public Chapter? CurrentChapter { get => _currentChapter; private set => SetField(ref _currentChapter, value); }
    public PanelMode PanelMode { get => _panelMode; set => SetField(ref _panelMode, value); }
    public QuizViewModel? QuizVM { get => _quizViewModel; private set => SetField(ref _quizViewModel, value); }
    public bool IsBookmarked { get => _isBookmarked; private set => SetField(ref _isBookmarked, value); }
    public bool IsDarkTheme { get => _isDarkTheme; private set => SetField(ref _isDarkTheme, value); }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetField(ref _searchQuery, value))
            {
                SearchResults = _searchService.Search(value);
                if (SearchResults.Count > 0 || value.Length >= 2)
                    PanelMode = PanelMode.Search;
            }
        }
    }

    public List<SearchResult> SearchResults
    {
        get => _searchResults;
        private set { SetField(ref _searchResults, value); OnPropertyChanged(nameof(HasSearchResults)); }
    }

    public bool HasSearchResults => _searchResults.Count > 0;

    public string ProgressText
    {
        get
        {
            var read = _progressService.GetReadCount();
            return $"{read} из {_totalSections} разделов прочитано";
        }
    }

    public string StatusText => CurrentSection != null
        ? $"{CurrentChapter?.Title} / {CurrentSection.Title}"
        : "Выберите раздел";

    public string ThemeButtonText => IsDarkTheme ? "☀ Светлая" : "☾ Тёмная";

    public RelayCommand ToggleReadCommand { get; }
    public bool IsCurrentSectionRead =>
        _currentChapter != null && _currentSection != null &&
        _progressService.IsSectionRead(_currentChapter.Id, _currentSection.Id);
    public RelayCommand ToggleBookmarkCommand { get; }
    public RelayCommand StartQuizCommand { get; }
    public RelayCommand ShowGlossaryCommand { get; }
    public RelayCommand ShowBookmarksCommand { get; }
    public RelayCommand ToggleThemeCommand { get; }
    public RelayCommand NavigateSearchResultCommand { get; }
    public RelayCommand NavigateBookmarkCommand { get; }

    public void SelectSection(int chapterId, int sectionId)
    {
        var chapter = _contentService.GetChapters().FirstOrDefault(c => c.Id == chapterId);
        var section = chapter?.Sections.FirstOrDefault(s => s.Id == sectionId);
        if (chapter == null || section == null) return;

        foreach (var nav in NavItems)
        {
            nav.IsSelected = false;
            foreach (var child in nav.Children)
                child.IsSelected = false;
        }

        var navChapter = NavItems.FirstOrDefault(n => n.ChapterId == chapterId);
        var navSection = navChapter?.Children.FirstOrDefault(n => n.SectionId == sectionId);
        if (navSection != null) navSection.IsSelected = true;

        CurrentChapter = chapter;
        CurrentSection = section;
        IsBookmarked = _progressService.IsBookmarked(chapterId, sectionId);
        PanelMode = PanelMode.Section;

        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsCurrentSectionRead));
    }

    private void BuildNavigation()
    {
        var chapters = _contentService.GetChapters();
        _totalSections = chapters.Sum(c => c.Sections.Count);

        NavItems = chapters.Select(c => new NavItem
        {
            ChapterId = c.Id,
            SectionId = 0,
            Title = c.Title,
            IsChapter = true,
            Children = c.Sections.Select(s => new NavItem
            {
                ChapterId = c.Id,
                SectionId = s.Id,
                Title = s.Title,
                IsRead = _progressService.IsSectionRead(c.Id, s.Id)
            }).ToList()
        }).ToList();

        OnPropertyChanged(nameof(NavItems));
    }

    private void ToggleRead()
    {
        if (CurrentChapter == null || CurrentSection == null) return;

        var navSection = NavItems
            .FirstOrDefault(n => n.ChapterId == CurrentChapter.Id)?
            .Children.FirstOrDefault(n => n.SectionId == CurrentSection.Id);

        if (_progressService.IsSectionRead(CurrentChapter.Id, CurrentSection.Id))
        {
            _progressService.UnmarkSectionRead(CurrentChapter.Id, CurrentSection.Id);
            if (navSection != null) navSection.IsRead = false;
        }
        else
        {
            _progressService.MarkSectionRead(CurrentChapter.Id, CurrentSection.Id);
            if (navSection != null) navSection.IsRead = true;
        }

        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(IsCurrentSectionRead));
    }

    private void ToggleBookmark()
    {
        if (CurrentChapter == null || CurrentSection == null) return;
        if (IsBookmarked)
        {
            _progressService.RemoveBookmark(CurrentChapter.Id, CurrentSection.Id);
            IsBookmarked = false;
        }
        else
        {
            _progressService.AddBookmark(new Bookmark
            {
                ChapterId = CurrentChapter.Id,
                SectionId = CurrentSection.Id,
                ChapterTitle = CurrentChapter.Title,
                SectionTitle = CurrentSection.Title
            });
            IsBookmarked = true;
        }
        OnPropertyChanged(nameof(Bookmarks));
    }

    private void StartQuiz()
    {
        if (CurrentChapter == null) return;
        QuizVM = new QuizViewModel(CurrentChapter.Id, CurrentChapter.Quiz, _progressService);
        PanelMode = PanelMode.Quiz;
    }

    private void ToggleTheme(object? _)
    {
        IsDarkTheme = !IsDarkTheme;
        _progressService.Theme = IsDarkTheme ? "Dark" : "Light";
        OnPropertyChanged(nameof(ThemeButtonText));

        var app = System.Windows.Application.Current;
        var themePath = IsDarkTheme
            ? "Themes/DarkTheme.xaml"
            : "Themes/LightTheme.xaml";

        var existing = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme") == true);
        if (existing != null) app.Resources.MergedDictionaries.Remove(existing);

        app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        });
    }

    private void NavigateSearchResult(object? param)
    {
        if (param is SearchResult result && result.SectionId > 0)
            SelectSection(result.ChapterId, result.SectionId);
    }

    private void NavigateBookmark(object? param)
    {
        if (param is Bookmark bookmark)
            SelectSection(bookmark.ChapterId, bookmark.SectionId);
    }
}
