using Microsoft.Extensions.Configuration;

namespace Parser.Settings;

public class ParserInfo
{
    public List<FileInfo> Files { get; set; }
    public CommentsInfo Comments { get; set; }
    public OutputInfo Output { get; set; }
    public List<ReadFromInfo> ReadFrom { get; set; }
}