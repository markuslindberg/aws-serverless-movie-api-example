using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class GetMovieRequestValidator : AbstractValidator<GetMovieRequest>
{
    public GetMovieRequestValidator()
    {
        RuleFor(x => x.MovieId).MovieIdPattern();
    }
}