using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Serilog;
using Parser.Interfaces;
using Parser.Settings;
using FileInfo = Parser.Settings.FileInfo;

namespace Parser;

public class Parser : IParser
{
    private readonly ILogger? _logger;
    private readonly ParserInfo _settings;

    
    public Parser(ParserInfo settings, ILogger? logger = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;
    }

    /// <summary>
    /// Парсинг файлов с удалением комментариев.
    /// </summary>
    public void Parse()
    {
        try
        {
            _logger?.Information($"Starting parse from {nameof(Parser)}");
            
            _logger?.Information("Outputting to console:  {console}", _settings.Output.Console);
            
            _logger?.Information("Outputting to file {file} at path: {path}", _settings.Output.File.Enabled, (_settings.Output.File.Enabled ? _settings.Output.File.Path : "%BASEDIR%/" ));
            
            foreach (var source in _settings.ReadFrom)
                (source.Name.Equals("Console", StringComparison.OrdinalIgnoreCase) ? 
                    (Action)ParseFromConsole : () => ParseFromFiles(source.Path))();
            
            _logger?.Information("Parsing {count} files", _settings.Files.Count);

        }
        finally
        {
            _logger?.Information($"Ending parse from {nameof(Parser)}");
        }
    }

    // Обработка ввода из консоли
        private void ParseFromConsole()
        {
            _logger?.Information("Reading from console... (Press 'Esc' to parse, 'Enter' for new line)");

            var inputBuffer = new StringBuilder();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    // Завершаем ввод и обрабатываем текст
                    string modifiedContent = RemoveComments(inputBuffer.ToString());
                    Output(modifiedContent);
                    inputBuffer.Clear();
                   
                    _logger?.Information("Enter new input or press 'Esc' to exit.");

                    // Ждем следующую клавишу, если 'Escape' — выходим
                    key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        _logger?.Information("Exiting...");
                        break; // Выход из цикла и метода
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    // Добавляем новую строку
                    inputBuffer.AppendLine();
                    Console.WriteLine();
                }
                else if (key.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                {
                    // Удаляем последний символ
                    inputBuffer.Length--;
                    Console.Write("\b \b"); // Визуальное удаление символа в консоли
                }
                else if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                {
                    // Прерывание ввода (Ctrl + C)
                    _logger?.Information("Input canceled.");
                    
                }
                else
                {
                    // Добавляем символ в буфер
                    inputBuffer.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
            
        }

    // Обработка ввода из файлов
    private void ParseFromFiles(string path)
    {
        var directory = Environment.ExpandEnvironmentVariables(path);
        
        
        
        for (int i = 0; i < _settings.Files.Count; i++)
        {
            
            string filePath = Path.Combine(directory, _settings.Files[i].FilePath);
            if (File.Exists(filePath))
            {
                _logger?.Information("Reading file: {path}", filePath);
                var content = File.ReadAllText(filePath);
                string modifiedContent = RemoveComments(content);
                Output(modifiedContent);
            }
            else
            {
                _logger?.Warning($"File not found: { _settings.Files[i].FilePath}");
            }
        }
    }
    
  private string RemoveComments(string content)
{
    var modifiedContent = new StringBuilder();

    foreach (var token in _settings.Comments.Tokens)
    {
        if (token.Enabled)
        {
            if (token.Type.Equals("Line", StringComparison.OrdinalIgnoreCase))
            {
                using (var reader = new StringReader(content))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        int index = line.IndexOf(token.Start, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                        {
                            line = line.Substring(0, index);
                        }
                        modifiedContent.AppendLine(line);
                    }
                }
                content = modifiedContent.ToString();
                modifiedContent.Clear();
            }
            else if (token.Type.Equals("Block", StringComparison.OrdinalIgnoreCase))
            {
                int startIndex = 0;
                int depth = 0;

                while (startIndex < content.Length)
                {
                    int start = content.IndexOf(token.Start, startIndex, StringComparison.OrdinalIgnoreCase);
                    int end = content.IndexOf(token.End, startIndex, StringComparison.OrdinalIgnoreCase);

                    if (start >= 0 && (start < end || end == -1))
                    {
                        depth++;
                        if (depth == 1)
                        {
                            modifiedContent.Append(content.Substring(startIndex, start - startIndex));
                        }
                        startIndex = start + token.Start.Length;
                    }
                    else if (end >= 0)
                    {
                        depth--;
                        startIndex = end + token.End.Length;
                    }
                    else
                    {
                        if (depth == 0)
                        {
                            modifiedContent.Append(content.Substring(startIndex));
                        }
                        break;
                    }
                }

                content = modifiedContent.ToString();
                modifiedContent.Clear();
            }
        }
    }

    return content;
}



    private void Output(string content)
    {
        if (_settings.Output.Console　&& !string.IsNullOrWhiteSpace(content))
        {
            _logger?.Information($"\n{content}"); 
        }

        if (_settings.Output.File.Enabled)
        {
           
            var outputPath = Environment.ExpandEnvironmentVariables(_settings.Output.File.Path);

           
            var extensionIndex = outputPath.LastIndexOf('.');

            
            if (extensionIndex == -1)
            {
                extensionIndex = outputPath.Length;
            }

           
            var fileNameWithoutExtension = outputPath.Substring(0, extensionIndex);
           
            var fileName = $"{fileNameWithoutExtension.Replace("\\", "/")}{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
            
            var fullOutputPath = Path.Combine(Path.GetDirectoryName(outputPath) ?? string.Empty, fileName);

            
            var directory = Path.GetDirectoryName(fullOutputPath);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(fullOutputPath, content);

            _logger?.Information("Output written to: {path}",fullOutputPath);
        }
    }
}