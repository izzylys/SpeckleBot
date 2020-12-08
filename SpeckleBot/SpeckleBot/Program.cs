using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeckleBot.Commands;
using SpeckleBot.Data;
using SpeckleBot.Events;

namespace SpeckleBot
{
  class Program
  {
    public static string BasePath { get; private set; }
    public static string DataPath { get; private set; }

    public static EventId BotEventId { get; private set; } = new EventId(420, "SpeckleBot");

    public DiscordClient Client { get; set; }
    public CommandsNextExtension Commands { get; set; }
    private static string configPath;

    static void Main(string[ ] args)
    {
      BasePath = AppDomain.CurrentDomain.BaseDirectory;
      DataPath = Path.Combine(BasePath, "Data");
      configPath = Path.Combine(BasePath, "config.json");

      // pass execution through async method
      var program = new Program();
      program.RunBotAsync().GetAwaiter().GetResult();
    }

    public async Task RunBotAsync()
    {
      // load config file and set up client
      var cfgJson = await ReadConfigFile(configPath);
      var cfgDiscord = ConfigureDiscord(cfgJson);
      Client = new DiscordClient(cfgDiscord);
      ConfigureCommands(cfgJson);
      ConfigureEvents();

      // connect, baby!
      Debug.WriteLine("Connecting to disord...");
      await Client.ConnectAsync();

      // prevent premature quitting
      await Task.Delay(-1);
    }

    private async Task<ConfigJson> ReadConfigFile(string path)
    {
      Debug.WriteLine("Reading config file...");
      var data = "";
      try
      {
        using(FileStream fs = File.OpenRead(path))
        using(StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
        {
          data = await sr.ReadToEndAsync();
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Error reading config file: {e.GetType()}: {e.Message}");
        Console.Read();
        Environment.Exit(1);
      }
      ConfigJson cfgjson = JsonConvert.DeserializeObject<ConfigJson>(data);

      return cfgjson;
    }

    private DiscordConfiguration ConfigureDiscord(ConfigJson config)
    {
      Debug.WriteLine("Setting up discord configuration...");
      // Setup loglevel based on config file string
      LogLevel ll = new LogLevel();
      switch (config.LogLevel)
      {
        case "debug":
          ll = LogLevel.Debug;
          break;
        case "info":
          ll = LogLevel.Information;
          break;
        case "critical":
          ll = LogLevel.Critical;
          break;
        case "error":
          ll = LogLevel.Error;
          break;
        case "warning":
          ll = LogLevel.Warning;
          break;
        default:
          ll = LogLevel.Information;
          break;
      }

      var cfg = new DiscordConfiguration
      {
        Token = config.Token,
        TokenType = TokenType.Bot,

        AutoReconnect = true,
        MinimumLogLevel = ll
      };

      return cfg;
    }

    private void ConfigureCommands(ConfigJson config)
    {
      Debug.WriteLine(Program.BotEventId, $"Setting up commands...");
      var ccfg = new CommandsNextConfiguration()
      {
        StringPrefixes = config.CommandPrefix,
        CaseSensitive = false,
        EnableDms = true,
        EnableMentionPrefix = true
      };
      Commands = Client.UseCommandsNext(ccfg);

      Commands.RegisterCommands<GraphQLCommands>();

      Commands.CommandExecuted += StaticEvents.Commands_CommandExecuted;
      Commands.CommandErrored += StaticEvents.Commands_CommandErrored;
    }

    private void ConfigureEvents()
    {
      Debug.WriteLine(Program.BotEventId, "Setting up events...");
      Client.Ready += StaticEvents.Client_Ready;
      Client.ClientErrored += StaticEvents.Client_ClientError;
    }
  }
}
