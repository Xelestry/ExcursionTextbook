using System.IO;
using ExcursionTextbook.Models;
using Newtonsoft.Json;

namespace ExcursionTextbook.Services;

public class ProgressService
{
    private readonly string _progressPath;
    private ProgressData _data;

    public ProgressService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ExcursionTextbook");
        Directory.CreateDirectory(dir);
        _progressPath = Path.Combine(dir, "progress.json");
        _data = Load();
    }

    public bool IsSectionRead(int chapterId, int sectionId)
        => _data.ReadSections.Contains(MakeKey(chapterId, sectionId));

    public void MarkSectionRead(int chapterId, int sectionId)
    {
        _data.ReadSections.Add(MakeKey(chapterId, sectionId));
        Save();
    }

    public void UnmarkSectionRead(int chapterId, int sectionId)
    {
        _data.ReadSections.Remove(MakeKey(chapterId, sectionId));
        Save();
    }

    public int GetReadCount() => _data.ReadSections.Count;

    public void SaveQuizResult(int chapterId, int score, int total)
    {
        _data.QuizResults[$"chapter_{chapterId}"] = new QuizResult { Score = score, Total = total };
        Save();
    }

    public QuizResult? GetQuizResult(int chapterId)
    {
        _data.QuizResults.TryGetValue($"chapter_{chapterId}", out var result);
        return result;
    }

    public List<Bookmark> GetBookmarks() => new List<Bookmark>(_data.Bookmarks);

    public void AddBookmark(Bookmark bookmark)
    {
        if (_data.Bookmarks.Any(b => b.ChapterId == bookmark.ChapterId && b.SectionId == bookmark.SectionId))
            return;
        _data.Bookmarks.Add(bookmark);
        Save();
    }

    public void RemoveBookmark(int chapterId, int sectionId)
    {
        _data.Bookmarks.RemoveAll(b => b.ChapterId == chapterId && b.SectionId == sectionId);
        Save();
    }

    public bool IsBookmarked(int chapterId, int sectionId)
        => _data.Bookmarks.Any(b => b.ChapterId == chapterId && b.SectionId == sectionId);

    public string Theme
    {
        get => _data.Theme;
        set { _data.Theme = value; Save(); }
    }

    private static string MakeKey(int chapterId, int sectionId) => $"{chapterId}_{sectionId}";

    private ProgressData Load()
    {
        if (!File.Exists(_progressPath)) return new ProgressData();
        try
        {
            var json = File.ReadAllText(_progressPath);
            return JsonConvert.DeserializeObject<ProgressData>(json) ?? new ProgressData();
        }
        catch { return new ProgressData(); }
    }

    private void Save()
    {
        File.WriteAllText(_progressPath, JsonConvert.SerializeObject(_data, Formatting.Indented));
    }

    private class ProgressData
    {
        public HashSet<string> ReadSections { get; set; } = new();
        public Dictionary<string, QuizResult> QuizResults { get; set; } = new();
        public List<Bookmark> Bookmarks { get; set; } = new();
        public string Theme { get; set; } = "Light";
    }
}

public class QuizResult
{
    public int Score { get; set; }
    public int Total { get; set; }
}
