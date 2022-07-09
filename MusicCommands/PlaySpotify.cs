using DSharpPlus.CommandsNext;
using SpotifyAPI.Web;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
namespace Music.Commands
{
    public partial class MusicCommands : BaseCommandModule
    {
        public SpotifyClient Spotify { get; set; }

        private async Task<SearchResult> GetSpotifyTrack(ServerInstance inst, string search)
        {
            var track = await Spotify.Tracks.Get(search);
            var searchTerm = $"{track.Artists[0].Name} - {track.Name}";
            var lavaSearch = await inst.connection.Node.Rest.GetTracksAsync(searchTerm);
            if (lavaSearch.LoadResultType == LavalinkLoadResultType.NoMatches || lavaSearch.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                return new SearchResult();
            }
            else
            {
                return new SearchResult
                {
                    PlayListName = "",
                    Tracks = new List<LavalinkTrack> { lavaSearch.Tracks.First() }
                };
            }
        }

        private async Task<SearchResult> GetSpotifyPlaylist(ServerInstance inst, string search, DSharpPlus.Entities.DiscordMember fuck, int shit = 0)
        {
            var playlist = await Spotify.Playlists.Get(search);
            var LavaTracks = new List<LavalinkTrack>();
            if (playlist.Tracks.Items == null)
            {
                return new SearchResult();
            }
            foreach (var item in await Spotify.PaginateAll(playlist.Tracks))
            {

                if (item.Track is FullTrack track)
                {

                    var searchTerm = $"{track.Artists[0].Name} - {track.Name}";
                    var lavaSearch = await inst.connection.Node.Rest.GetTracksAsync(searchTerm);

                    if (lavaSearch.LoadResultType == LavalinkLoadResultType.NoMatches || lavaSearch.LoadResultType == LavalinkLoadResultType.LoadFailed)
                    {
                        continue;
                    }
                    else
                    {
                        switch (shit)
                        {
                            case 0:
                                await inst.AddSong(new SearchResult{Tracks = new List<LavalinkTrack>{lavaSearch.Tracks.First()}}, fuck, silent: true);
                                break;
                            case 1:
                                await inst.AddNext(new SearchResult
                                {
                                    PlayListName = "",
                                    Tracks = new List<LavalinkTrack>{lavaSearch.Tracks.First()}
                                }, fuck, silent: true);
                                break;
                        }
                    }
                }

            }
            return new SearchResult
            {
                PlayListName = playlist.Name,
                Tracks = new List<LavalinkTrack>()
            };
        }
    }
}