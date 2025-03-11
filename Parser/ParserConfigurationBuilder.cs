using Microsoft.Extensions.Configuration;
using Serilog;
using Parser.Settings;
using Serilog.Configuration;
using FileInfo = Parser.Settings.FileInfo;

namespace Parser;

using FileInfo = global::Parser.Settings.FileInfo;

public class ParserConfigurationBuilder
{
    
    private ILogger _logger;
    
    private List<FileInfo> _files = new();
    
    
    public ParserConfigurationBuilder()
    {
        
    }

    
    
    public ParserConfigurationBuilder AddFiles(List<FileInfo> parserFilesInfo)
    {
        _files.AddRange(parserFilesInfo);
        return this;
    }
    public ParserConfigurationBuilder AddFile(FileInfo fileInfo)
    {
        _files.Add(fileInfo);
        return this;
    }
    
    public ParserConfigurationBuilder AddConsole()
    {
        return this;
    }
    
    public ParserConfigurationBuilder AddLogger(ILogger? logger = null)
    {
        _logger = logger ?? new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        return this;
    }

    
    
    //
    // public Parser Create()
    // {
    //     
    //
    // }

}