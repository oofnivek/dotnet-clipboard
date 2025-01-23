namespace DotNetClipboard.Models
{
    public class Clipboard
    {
        public int Id { get; set; }
        public string? Alias { get; set; }
        public required string Content { get; set; }
    }
}
