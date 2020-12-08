using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Speckle.Core.Api;
using Speckle.Core.Credentials;

namespace SpeckleBot.Commands
{
  public class GraphQLCommands : BaseCommandModule
  {
    public Account DefaultAccount;
    public Client SpeckleClient;

    public GraphQLCommands()
    {
      DefaultAccount = AccountManager.GetDefaultAccount();
      SpeckleClient = new Client(DefaultAccount);
    }

    [Command("streams")]
    [Description("get a list of the user's streams")]
    public async Task UserStreams(CommandContext ctx)
    {
      var streams = await SpeckleClient.StreamsGet(limit: 9);

      var embed = new DiscordEmbedBuilder
      {
        Title = $"Here are {streams.Count} of {SpeckleClient.Account.userInfo.name}'s streams",
        Color = DiscordColor.Blurple
      };

      foreach (var stream in streams)
      {
        embed.AddField(stream.name, stream.ToString(), true);
      }

      await ctx.Channel.SendMessageAsync(embed: embed);
    }

    [Command("stream")]
    [Description("gets a stream by its ID")]
    public async Task Stream(CommandContext ctx, [RemainingText] string streamId = null)
    {
      streamId = streamId?.Trim();
      if (string.IsNullOrWhiteSpace(streamId))
      {
        DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":thinking:");
        await ctx.Channel.SendMessageAsync($"Hmm I need a steam idea to fetch a stream for you {emoji}");
        return;
      }

      Stream stream = null;
      try
      {
        stream = await SpeckleClient.StreamGet(streamId);
      }
      catch (Exception e)
      {
        DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":thinking:");
        await ctx.Channel.SendMessageAsync($"Hmm something went wrong {emoji} - {e.Message}");
        return;
      }

      var embed = new DiscordEmbedBuilder
      {
        Title = stream.name,
        Description = stream.description,
        Color = DiscordColor.CornflowerBlue,
      };
      embed.AddField("Stream ID", stream.id, true);
      embed.AddField("Is Public", stream.isPublic.ToString(), true);
      embed.AddField("Branches", string.Join(", ", stream.branches.items.Select(b => b.name)), true);
      embed.AddField("Collaborators", string.Join(", ", stream.collaborators.Select(c => c.name)), true);

      await ctx.Channel.SendMessageAsync(embed: embed);
    }
  }

}
