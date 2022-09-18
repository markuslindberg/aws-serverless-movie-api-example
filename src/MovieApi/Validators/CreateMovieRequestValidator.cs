using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class CreateMovieRequestValidator : AbstractValidator<CreateMovieRequest>
{
    public CreateMovieRequestValidator()
    {
        RuleFor(x => x.Movie).NotNull();
        RuleFor(x => x.Movie).SetValidator(new MovieValidator());
    }
}