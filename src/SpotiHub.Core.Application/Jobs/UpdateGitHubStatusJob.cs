using System;
using System.Linq;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;
using Quartz;
using SpotifyAPI.Web;
using SpotiHub.Infrastructure.Contract.Services;

namespace SpotiHub.Core.Application.Jobs;

[DisallowConcurrentExecution]
public class UpdateGitHubStatusJob : IJob
{
    private readonly ISpotifyClientFactory _spotifyClientFactory;
    private readonly IGitHubClientFactory _gitHubClientFactory;

    public UpdateGitHubStatusJob(ISpotifyClientFactory spotifyClientFactory, IGitHubClientFactory gitHubClientFactory)
    {
        _spotifyClientFactory = spotifyClientFactory;
        _gitHubClientFactory = gitHubClientFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var user = context.JobDetail.JobDataMap.GetString("user")!;
        
        var spotify = await _spotifyClientFactory.GetClientAsync(user);

        var data = await spotify.Player.GetCurrentPlayback();

        if (data.IsPlaying)
        {
            var info = GetTrackInfo(data.Item);
            
            var action = new Mutation().ChangeUserStatus(new ChangeUserStatusInput
                {
                    Message = $"Currently listening to {info.Name} by {info.Artist}!",
                    Emoji = "🎶",
                    ExpiresAt = default,
                    LimitedAvailability = false,
                    OrganizationId = default,
                    ClientMutationId = Guid.NewGuid().ToString()
                }).Select(payload => new { payload.Status.UpdatedAt });

            var github = await _gitHubClientFactory.GetConnectionAsync(user);

            await github.Run(action);
        }
    }

    public (string Name, string Artist) GetTrackInfo(IPlayableItem item)
    {
        return item switch
        {
            FullTrack track => (track.Name, track.Artists.First().Name),
            FullEpisode episode => (episode.Name, episode.Show.Name),
            _ => default
        };
    }
}