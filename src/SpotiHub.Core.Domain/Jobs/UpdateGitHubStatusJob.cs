using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;
using Quartz;
using SpotifyAPI.Web;
using SpotiHub.Infrastructure.Contract.Services;

namespace SpotiHub.Core.Domain.Jobs;

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
            var trackName = data.Item switch
            {
                FullTrack track => track.Name,
                FullEpisode episode => episode.Name,
                _ => string.Empty
            };

            var action = new Mutation().ChangeUserStatus(
                new Arg<ChangeUserStatusInput>(new ChangeUserStatusInput
                {
                    Message = $"Currently listening to {trackName}!"
                }));

            var github = await _gitHubClientFactory.GetConnectionAsync(user);

            await github.Run(action);
        }
    }
}