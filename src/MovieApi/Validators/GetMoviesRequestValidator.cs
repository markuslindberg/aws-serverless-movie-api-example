using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class GetMoviesRequestValidator : AbstractValidator<GetMoviesRequest>
{
    public GetMoviesRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty();
    }
}