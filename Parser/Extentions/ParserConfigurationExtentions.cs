using Parser.Settings;
using FileInfo = Parser.Settings.FileInfo;
using Settings_FileInfo = Parser.Settings.FileInfo;

namespace Parser.Extentions;

public static class ParserConfigurationExtentions
{
    public static ParserConfigurationBuilder AddFile(this ParserConfigurationBuilder builder, string filePath, string? fileName = null)
    {
        return builder.AddFile(new Settings_FileInfo()
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath)),
            FileName = fileName
        });
    }

    
}