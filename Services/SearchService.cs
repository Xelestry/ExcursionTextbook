using ExcursionTextbook.Models;

namespace ExcursionTextbook.Services;

public class SearchResult
{
    public int ChapterId { get; set; }
    public int SectionId { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
}

public class SearchService
{
    private readonly ContentService _contentService;

    public SearchService(ContentService contentService)
    {
        _contentService = contentService;
    }

    public List<SearchResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<SearchResult>();

        var results = new List<SearchResult>();
        var q = query.Trim();

        foreach (var chapter in _contentService.GetChapters())
        {
            foreach (var section in chapter.Sections)
            {
                if (section.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    section.Content.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new SearchResult
                    {
                        ChapterId = chapter.Id,
                        SectionId = section.Id,
                        ChapterTitle = chapter.Title,
                        SectionTitle = section.Title,
                        Snippet = BuildSnippet(section.Content, q)
                    });
                }
            }

            foreach (var term in chapter.GlossaryTerms)
            {
                if (term.Term.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    term.Definition.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    if (!results.Any(r => r.ChapterId == chapter.Id && r.SectionId == -1 &&
                                         r.SectionTitle == "Глоссарий: " + term.Term))
                    {
                        results.Add(new SearchResult
                        {
                            ChapterId = chapter.Id,
                            SectionId = -1,
                            ChapterTitle = chapter.Title,
                            SectionTitle = "Глоссарий: " + term.Term,
                            Snippet = term.Definition.Length > 120
                                ? term.Definition[..120] + "..."
                                : term.Definition
                        });
                    }
                }
            }
        }

        return results;
    }

    private static string BuildSnippet(string content, string query)
    {
        var idx = content.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return content.Length > 120 ? content[..120] + "..." : content;
        }
        var start = Math.Max(0, idx - 40);
        var end = Math.Min(content.Length, idx + query.Length + 80);
        var snippet = (start > 0 ? "..." : "") + content[start..end] + (end < content.Length ? "..." : "");
        return snippet;
    }
}
