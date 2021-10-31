using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Internal;
using Octokit.GraphQL.Model;

namespace SpotiHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        [HttpPost("github", Name = "LinkGitHub")]
        public async Task<IActionResult> LinkGitHub(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("spotify", Name = "LinkSpotify")]
        public async Task<IActionResult> LinkSpotify(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}