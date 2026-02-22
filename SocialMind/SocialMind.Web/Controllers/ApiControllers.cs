using Microsoft.AspNetCore.Mvc;
using SocialMind.Shared.Models;
using SocialMind.Shared.Services;

namespace SocialMind.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Post>>> GetPosts()
    {
        var posts = await _postService.GetPostsAsync();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(string id)
    {
        var post = await _postService.GetPostAsync(id);
        if (post == null)
            return NotFound();
        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        var createdPost = await _postService.CreatePostAsync(post);
        return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Post>> UpdatePost(string id, Post post)
    {
        post.Id = id;
        var updatedPost = await _postService.UpdatePostAsync(post);
        return Ok(updatedPost);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePost(string id)
    {
        var result = await _postService.DeletePostAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SocialAccount>>> GetAccounts()
    {
        var accounts = await _platformService.GetAccountsAsync();
        return Ok(accounts);
    }

    [HttpPost("connect")]
    public async Task<ActionResult<SocialAccount>> ConnectAccount([FromBody] dynamic request)
    {
        var platform = request["platform"];
        var token = request["token"];
        var account = await _platformService.ConnectAccountAsync(platform, token);
        return Ok(account);
    }

    [HttpPost("{id}/disconnect")]
    public async Task<ActionResult> DisconnectAccount(string id)
    {
        var result = await _platformService.DisconnectAccountAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Analytics>>> GetAnalytics([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var analytics = await _analyticsService.GetAnalyticsAsync(from, to);
        return Ok(analytics);
    }

    [HttpGet("trending-hashtags")]
    public async Task<ActionResult<List<string>>> GetTrendingHashtags()
    {
        var hashtags = await _analyticsService.GetTrendingHashtagsAsync();
        return Ok(hashtags);
    }

    [HttpGet("top-posts")]
    public async Task<ActionResult<Dictionary<string, int>>> GetTopPosts()
    {
        var topPosts = await _analyticsService.GetTopPostsAsync();
        return Ok(topPosts);
    }
}
