using System.IO;
using System.Reflection;
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

        var json = LoadContentJson();
        var root = JsonConvert.DeserializeObject<ContentRoot>(json)
            ?? throw new InvalidDataException("Не удалось десериализовать content.json");

        _chapters = root.Chapters;
        return _chapters;
    }

    // Prefer an external Data\content.json (lets the content be edited without rebuilding);
    // otherwise fall back to the copy embedded in the executable.
    private string LoadContentJson()
    {
        if (File.Exists(_contentPath))
            return File.ReadAllText(_contentPath);

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("content.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new FileNotFoundException("content.json не найден ни на диске, ни в ресурсах", _contentPath);

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
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
