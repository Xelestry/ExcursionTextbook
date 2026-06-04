namespace ExcursionTextbook.Models;

public class Bookmark
{
    public int ChapterId { get; set; }
    public int SectionId { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
}
