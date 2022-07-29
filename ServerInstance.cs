using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Music.Commands;
namespace Music
{


    public class UserSongList
    {
        public DiscordMember member;
        public List<LavalinkTrack> queue;
        public UserSongList(DiscordMember member)
        {
            this.member = member;
            queue = new List<LavalinkTrack>();
        }

        public void Shuffle()
        {
            // Fisher-Yates shuffle
            Random rng = new Random();
            int n = queue.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                LavalinkTrack value = queue[k];
                queue[k] = queue[n];
                queue[n] = value;
            }
        }
    }
    public struct NowSong
    {
        public DiscordMember member;
        public LavalinkTrack track;
        public NowSong(DiscordMember member, LavalinkTrack track)
        {
            this.track = track;
            this.member = member;
        }
    }
    public class ServerInstance
    {
        public LavalinkGuildConnection connection;
        public DiscordChannel channel;
        public List<UserSongList> userSongs;
        public int currentListIndex = 0;
        public DiscordChannel msgChannel;
        public NowSong NowPlaying;
        private async Task PlaybackFinished(LavalinkGuildConnection ll, TrackFinishEventArgs e)
        {
            if(!connection.IsConnected) 
                return;
            

            if (userSongs.Count == 0)
            {
                await connection.StopAsync();
                return;
            }



            NowPlaying = new NowSong(userSongs[currentListIndex].member, userSongs[currentListIndex].queue[0]);
            userSongs[currentListIndex].queue.RemoveAt(0);
            if (userSongs[currentListIndex].queue.Count == 0)
            {
                userSongs.RemoveAt(currentListIndex);
            }

            if (currentListIndex >= userSongs.Count - 1)
            {
                currentListIndex = 0;
            }
            else
            {
                currentListIndex++;
            }
            if (!connection.IsConnected)
                return;
            await connection.PlayAsync(NowPlaying.track);
            await PrintPlaying();
        }
        private async Task PrintPlaying()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Now playing",
                Description = $"{NowPlaying.track.Title}",
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = NowPlaying.track.Author
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = NowPlaying.member.GetAvatarUrl(ImageFormat.Png),
                    Name = NowPlaying.member.DisplayName
                },
                Color = DiscordColor.Green,
                Url = NowPlaying.track.Uri.ToString(),
            };
            await msgChannel.SendMessageAsync(embed: embed);
        }
        public ServerInstance(DiscordChannel vc, DiscordClient client, DiscordChannel msgChan)
        {
            this.channel = vc;
            var lava = client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                throw new Exception("No nodes connected!");
            }
            var node = lava.ConnectedNodes.Values.First();

            var con = node.GetGuildConnection(channel.Guild);
            if (con != null)
            {
                Console.WriteLine("Existing Connection");
                con.DisconnectAsync();
            }
            
            
            this.connection = node.ConnectAsync(channel).ConfigureAwait(false).GetAwaiter().GetResult();
            this.connection.StopAsync();
            connection.PlaybackFinished += PlaybackFinished;
            this.userSongs = new List<UserSongList>();
            this.msgChannel = msgChan;
        }
        public async Task Skip()
        {

            await connection.StopAsync();
        }


        public async Task AddMessage(LavalinkTrack track, DiscordMember member)
        {
            var list = userSongs.FindIndex(x => x.member == member);
            if (connection.CurrentState.CurrentTrack == null)
            {
                NowPlaying = new NowSong(userSongs[list].member, userSongs[list].queue[0]);
                await connection.PlayAsync(NowPlaying.track);
                if (userSongs[list].queue.Count <= 1)
                {
                    userSongs.RemoveAt(list);
                }
                await PrintPlaying();
            }
            else
            {
                connection.Node.Parent.Client.Logger.Log(LogLevel.Information, "Added song to queue");

                var queue = new DiscordEmbedBuilder
                {
                    Title = "Added to queue",
                    Description = $"{track.Title}",
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = track.Author
                    },
                    Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        IconUrl = member.GetAvatarUrl(ImageFormat.Png),
                        Name = member.DisplayName
                    },
                    Url = track.Uri.ToString(),
                    Color = DiscordColor.Green
                };
                await msgChannel.SendMessageAsync(embed: queue);

            }
        }
        public async Task AddSong(LavalinkTrack track, DiscordMember member)
        {
            var list = userSongs.FindIndex(x => x.member == member);
            if (list == -1)
            {
                userSongs.Add(new UserSongList(member));
                list = userSongs.Count - 1;
            }

            userSongs[list].queue.Add(track);

            await AddMessage(track, member);

        }

        public async Task AddSong(SearchResult tracks, DiscordMember member, string playListName = "", bool silent = false)
        {
            var list = userSongs.FindIndex(x => x.member == member);
            if (list == -1)
            {
                userSongs.Add(new UserSongList(member));
                list = userSongs.Count - 1;
            }

            if (tracks.Tracks.Count() == 1)
            {
                userSongs[list].queue.Add(tracks.Tracks.First());

                if (!silent) {
                    await AddMessage(tracks.Tracks.First(), member);
                }
                
            }
            else
            {
                foreach (var track in tracks.Tracks)
                {
                    userSongs[list].queue.Insert(0, track);
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = $":musical_note: Playlist {tracks.PlayListName} loaded",
                    Description = $"{tracks.Tracks.Count()} tracks added to start of your queue",
                    Color = DiscordColor.Green
                };
                await msgChannel.SendMessageAsync(embed: embed);
            }
            if (connection.CurrentState.CurrentTrack == null)
            {
                NowPlaying = new NowSong(userSongs[list].member, userSongs[list].queue[0]);
                await connection.PlayAsync(NowPlaying.track);
                if (userSongs[list].queue.Count <= 1)
                {
                    userSongs.RemoveAt(list);
                }
                if (!silent)
                    await PrintPlaying();
            }

        }
        public async Task AddNext(SearchResult tracks, DiscordMember member, bool skip = false, bool silent = false)
        {
            var list = userSongs.FindIndex(x => x.member == member);
            if (list == -1)
            {
                userSongs.Add(new UserSongList(member));
                list = userSongs.FindIndex(x => x.member == member);
            }


            if (tracks.Tracks.Count() == 1)
            {

                userSongs[list].queue.Insert(0, tracks.Tracks.First());
            }
            else
            {
                tracks.Tracks.Reverse();
                foreach (var track in tracks.Tracks)
                {
                    userSongs[list].queue.Insert(0, track);
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = $":musical_note: Playlist {tracks.PlayListName} loaded",
                    Description = $"{tracks.Tracks.Count()} tracks added to start of your queue",
                    Color = DiscordColor.Green
                };
                await msgChannel.SendMessageAsync(embed: embed);
            }

            currentListIndex = list;

            if (connection.CurrentState.CurrentTrack == null)
            {
                NowPlaying = new NowSong(userSongs[list].member, userSongs[list].queue[0]);
                await connection.PlayAsync(NowPlaying.track);
                if (userSongs[list].queue.Count <= 1)
                {
                    userSongs.RemoveAt(list);
                }
                if (!silent)
                    await PrintPlaying();
            }
            else if (skip)
            {
                await Skip();
            }
            else
            {
                connection.Node.Parent.Client.Logger.Log(LogLevel.Information, "Added song to queue");
                if (tracks.Tracks.Count() == 1)
                {
                    var queue = new DiscordEmbedBuilder
                    {
                        Title = "Added to front of queue",
                        Description = $"{tracks.Tracks.First().Title}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = tracks.Tracks.First().Author
                        },
                        Author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            IconUrl = member.GetAvatarUrl(ImageFormat.Png),
                            Name = member.DisplayName
                        },
                        Url = tracks.Tracks.First().Uri.ToString(),
                        Color = DiscordColor.Green
                    };
                    await msgChannel.SendMessageAsync(embed: queue);
                }
            }
        }
    }
}