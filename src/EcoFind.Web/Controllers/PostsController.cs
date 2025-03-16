using EcoFind.Application.Features.Posts.Queries.GetAllPosts;
using EcoFind.Application.Features.Posts.Queries.GetPostBySlug;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Myrtus.Clarity.Core.Infrastructure.Authorization;

namespace EcoFind.Web.Controllers
{
    [EnableRateLimiting("Fixed")]
    [Route("blog/posts")]
    public class PostsController : Controller
    {
        private readonly ISender _mediator;

        public PostsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HasPermission("post:read")]
        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 0, int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllPostsQuery(pageIndex, pageSize));

            if (result.IsSuccess)
            {
                return View(result.Value);
            }

            return View("Error", result.Errors);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var result = await _mediator.Send(new GetPostBySlugQuery(slug));
            if (!result.IsSuccess)
            {
                return NotFound();
            }
            return View(result.Value);
        }
    }
}
