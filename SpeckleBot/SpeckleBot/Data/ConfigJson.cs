using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpeckleBot.Data
{
  /// <summary>
  /// Defines a structure that holds data from our json config file
  /// </summary>
  public struct ConfigJson
  {
    [JsonProperty("token")]
    public string Token { get; private set; }

    [JsonProperty("prefix")]
    public List<String> CommandPrefix { get; private set; }

    [JsonProperty("loglevel")]
    public string LogLevel { get; private set; }

  }

}
