using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Blish_HUD.GameService;

namespace Nekres.Stream_Out.Core.Services
{
    internal class GuildService : ExportService
    {
        private Gw2ApiManager Gw2ApiManager => StreamOutModule.Instance?.Gw2ApiManager;
        private DirectoriesManager DirectoriesManager => StreamOutModule.Instance?.DirectoriesManager;

        private const string GUILD_NAME = "guild_name.txt";
        private const string GUILD_TAG = "guild_tag.txt";
        private const string GUILD_EMBLEM = "guild_emblem.png";
        private const string GUILD_MOTD = "guild_motd.txt";

        private Regex GUILD_MOTD_PUBLIC = new Regex(@"(?<=\[public\]).*(?=\[\/public\])", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public GuildService()
        {
        }

        private async Task UpdateGuild()
        {
            if (!Gw2Mumble.IsAvailable || !Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters, TokenPermission.Guilds }))
                return;

            var guildId = await Gw2ApiManager.Gw2ApiClient.V2.Characters[Gw2Mumble.PlayerCharacter.Name].Core.GetAsync().ContinueWith(task => task.IsFaulted ? Guid.Empty : task.Result.Guild);

            if (guildId.Equals(Guid.Empty))
            {
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_NAME}", string.Empty);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_TAG}", string.Empty);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_MOTD}", string.Empty);
                await TextureUtil.ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}");
                return;
            }

            await Gw2ApiManager.Gw2ApiClient.V2.Guild[guildId].GetAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted)
                    return;
                var name = task.Result.Name;
                var tag = task.Result.Tag;
                var motd = task.Result.Motd ?? string.Empty;

                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_NAME}", name);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_TAG}", $"[{tag}]");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_MOTD}", GUILD_MOTD_PUBLIC.Match(motd).Value);

                var emblem = task.Result.Emblem;
                if (emblem == null)
                {
                    await TextureUtil.ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}");
                    return;
                }

                var bg = await Gw2ApiManager.Gw2ApiClient.V2.Emblem.Backgrounds.GetAsync(emblem.Background.Id);
                var fg = await Gw2ApiManager.Gw2ApiClient.V2.Emblem.Foregrounds.GetAsync(emblem.Foreground.Id);

                var layersCombined = new List<Gw2Sharp.WebApi.RenderUrl>();
                if (bg != null)
                    layersCombined.AddRange(bg.Layers);
                if (fg != null)
                    layersCombined.AddRange(fg.Layers.Skip(1));
                var layers = new List<Bitmap>();
                foreach (var renderUrl in layersCombined)
                {
                    using var textureStream = new MemoryStream();
                    await renderUrl.DownloadToStreamAsync(textureStream);
                    layers.Add(new Bitmap(textureStream));
                }
                if (!layers.Any())
                {
                    await TextureUtil.ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}");
                    return;
                }

                var colorsCombined = new List<int>();
                colorsCombined.AddRange(emblem.Background.Colors);
                colorsCombined.AddRange(emblem.Foreground.Colors);
                // A bulk request would be more efficient but ArenaNet disposes duplicate ids which ruins matching it against layer indices.
                // See also: https://api.guildwars2.com/v2/colors?ids=1,2,3,4,4,3,2,1
                // var colors = await Gw2ApiManager.Gw2ApiClient.V2.Colors.ManyAsync(colorsCombined);
                var colors = new List<Gw2Sharp.WebApi.V2.Models.Color>();
                foreach (var color in colorsCombined)
                    colors.Add(await Gw2ApiManager.Gw2ApiClient.V2.Colors.GetAsync(color));

                var result = new Bitmap(256, 256);
                for (var i = 0; i < layers.Count; i++)
                {
                    var layer = layers[i].FitTo(result);

                    if (colors.Any())
                    {
                        var color = System.Drawing.Color.FromArgb(colors[i].Cloth.Rgb[0], colors[i].Cloth.Rgb[1], colors[i].Cloth.Rgb[2]);
                        // apply colors
                        layer.Colorize(color);
                    }

                    // apply flags
                    if (bg != null && i < bg.Layers.Count)
                        layer.Flip(emblem.Flags.Any(x => x == GuildEmblemFlag.FlipBackgroundHorizontal), emblem.Flags.Any(x => x == GuildEmblemFlag.FlipBackgroundVertical));
                    else
                        layer.Flip(emblem.Flags.Any(x => x == GuildEmblemFlag.FlipForegroundHorizontal), emblem.Flags.Any(x => x == GuildEmblemFlag.FlipForegroundVertical));

                    // merge layer with result
                    var merged = result.Merge(layer);
                    result.Dispose();
                    layer.Dispose();
                    result = merged;
                }
                result?.Save($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}", ImageFormat.Png);
            });
        }

        protected override async Task Update()
        {
            await UpdateGuild();
        }

        public override async Task Clear()
        {
            var dir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await FileUtil.DeleteAsync(Path.Combine(dir, GUILD_NAME));
            await FileUtil.DeleteAsync(Path.Combine(dir, GUILD_TAG));
            await FileUtil.DeleteAsync(Path.Combine(dir, GUILD_EMBLEM));
            await FileUtil.DeleteAsync(Path.Combine(dir, GUILD_MOTD));
        }

        public override void Dispose()
        {
        }
    }
}
