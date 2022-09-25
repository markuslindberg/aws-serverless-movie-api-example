using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class GetDirectorMoviesRequestValidator : AbstractValidator<GetDirectorMoviesRequest>
{
    public GetDirectorMoviesRequestValidator()
    {
        RuleFor(x => x.DirectorId).DirectorIdPattern();
    }
}