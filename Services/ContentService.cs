using System.IO;
using ExcursionTextbook.Models;
using Newtonsoft.Json;

namespace ExcursionTextbook.Services;

public class ContentService
{
    private readonly string _contentPath;
    private List<Chapter>? _chapters;

    public ContentService()
    {
        _contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "content.json");
    }

    public List<Chapter> GetChapters()
    {
        if (_chapters != null) return _chapters;

        if (!File.Exists(_contentPath))
            throw new FileNotFoundException("content.json не найден", _contentPath);

        var json = File.ReadAllText(_contentPath);
        var root = JsonConvert.DeserializeObject<ContentRoot>(json)
            ?? throw new InvalidDataException("Не удалось десериализовать content.json");

        _chapters = root.Chapters;
        return _chapters;
    }

    public Section? FindSection(int chapterId, int sectionId)
    {
        return GetChapters()
            .FirstOrDefault(c => c.Id == chapterId)?
            .Sections.FirstOrDefault(s => s.Id == sectionId);
    }

    private class ContentRoot
    {
        [JsonProperty("chapters")]
        public List<Chapter> Chapters { get; set; } = new();
    }
}
