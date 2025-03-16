using FluentValidation;

namespace EcoFind.Application.Features.Posts.Queries.GetPostBySlug
{
    public class GetPostBySlugQueryValidator : AbstractValidator<GetPostBySlugQuery>
    {
        public GetPostBySlugQueryValidator()
        {
            RuleFor(q => q.Slug).NotEmpty().WithMessage("Slug must be provided");
        }
    }
}
