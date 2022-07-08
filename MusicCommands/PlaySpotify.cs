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
            } else {
                return new SearchResult{
                    PlayListName = "",
                    Tracks = lavaSearch.Tracks.ToList()
                };
            }
        }

        private async Task<SearchResult> GetSpotifyPlaylist(ServerInstance inst, string search)
        {
            var playlist = await Spotify.Playlists.Get(search);
            var LavaTracks = new List<LavalinkTrack>();
            if (playlist.Tracks.Items == null)
            {
                return new SearchResult();
            }

            foreach (var item in playlist.Tracks.Items)
            {
                if (item.Track is FullTrack track)
                {
                    var lavaTrack = await GetSpotifyTrack(inst, track.Id);
                    if (lavaTrack.Tracks.Count() != 0)
                    {
                        LavaTracks.Add(lavaTrack.Tracks.First());
                    }
                }
            }
            return new SearchResult
            {
                PlayListName = playlist.Name,
                Tracks = LavaTracks
            };
        }
    }
}