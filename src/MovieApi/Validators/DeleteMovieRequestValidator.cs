using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class DeleteMovieRequestValidator : AbstractValidator<DeleteMovieRequest>
{
    public DeleteMovieRequestValidator()
    {
        RuleFor(x => x.MovieId).MovieIdPattern();
    }
}