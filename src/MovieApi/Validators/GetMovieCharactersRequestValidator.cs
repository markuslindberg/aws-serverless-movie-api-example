using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class GetMovieCharactersRequestValidator : AbstractValidator<GetMovieCharactersRequest>
{
    public GetMovieCharactersRequestValidator()
    {
        RuleFor(x => x.MovieId).MovieIdPattern();
    }
}
