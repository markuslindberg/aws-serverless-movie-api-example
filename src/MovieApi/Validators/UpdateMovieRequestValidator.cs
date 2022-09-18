using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class UpdateMovieRequestValidator : AbstractValidator<UpdateMovieRequest>
{
    public UpdateMovieRequestValidator()
    {
        RuleFor(x => x.MovieId).MovieIdPattern();
        RuleFor(x => x.Movie).NotNull();
        RuleFor(x => x.Movie).SetValidator(new MovieValidator());
        RuleFor(x => x.Movie.MovieId).Equal(x => x.MovieId);
    }
}