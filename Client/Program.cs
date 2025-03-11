
using System.CommandLine;
using System.Reflection;
using Parser;
using Parser.Extentions;
using Parser.Settings;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using CommentsParser = Parser;

class Program
{

    public static async Task<int> Main(string[] args)
    {
            var configuration = BuildConfiguration();
            
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

     
        Log.Logger = logger;
        
        try
        {
            

            /*var parserConfiguration = new ParserSettingsConfiguration()
            {
                Files =
                [
                    new FileInfo()
                    {
                        FilePath = "appsettings.json",
                    }
                ],
                CommentsInfo = new CommentsInfo()
                {
                    Enabled = false,
                    Comments = new List<ParserCommentInfo>()
                    {
                        new()
                        {
                            CommentType = "Block",
                            Start = "sdsdsds"
                        }
                        
                    }
                }
            
            };*/
            
            
            
            /*ICommentsParser parser = new Parser.Parser(
                parserConfiguration,
                logger);*/
            
            // parser.Parse(new List<string>());


            // var parser = new ParserConfiguration()
            //     .ReadFrom.Configuration(configuration)
            //     .CreateParser();
            //
            // parser.Parse();

            // var parser = new ParserConfigurationBuilder()
            //     .AddFile(new FileInfo()
            //     {
            //         FilePath = "appsettings.json",
            //         FileName = "appps"
            //     })
            //     .AddConsole()
            //     .AddLogger(logger)
            //     .Create();



            var parserConfiguration = configuration
                .GetSection("Parser")
                .Get<ParserInfo>();


          
            var parser = new CommentsParser.Parser(parserConfiguration, logger);
            
            parser.Parse();

            // var parser = new ParserConfigurationBuilder()
            //     .ad
            //     .AddLogger(logger).Create();
            //
            // parser.Parse();



        }
        catch (Exception)
        {
            Log.Logger?.Fatal("Application start up failed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return 0;
    }


    private static IConfiguration BuildConfiguration()
    {
        var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
       
        Environment.SetEnvironmentVariable("BASEDIR", rootPath);
        var environmentVar = Environment.GetEnvironmentVariable("ENVIRONMENT");

        return new ConfigurationBuilder()
            .SetBasePath(rootPath)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environmentVar}.json", true)
            .AddJsonFile($"appsettings.{environmentVar}.User.json", true)
            .Build();
        
    }
    
}