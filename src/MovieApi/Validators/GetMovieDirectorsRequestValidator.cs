using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class GetMovieDirectorsRequestValidator : AbstractValidator<GetMovieDirectorsRequest>
{
    public GetMovieDirectorsRequestValidator()
    {
        RuleFor(x => x.MovieId).MovieIdPattern();
    }
}