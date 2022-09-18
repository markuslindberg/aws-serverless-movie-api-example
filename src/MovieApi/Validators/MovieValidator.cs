using FluentValidation;
using MovieApi.Domain;

namespace MovieApi.Validators;

public sealed class MovieValidator : AbstractValidator<Movie>
{
    public MovieValidator()
    {
        RuleFor(x => x.MovieId).MovieIdPattern();
    }
}