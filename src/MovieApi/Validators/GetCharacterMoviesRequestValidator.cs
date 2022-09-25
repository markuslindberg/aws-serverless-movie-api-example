using FluentValidation;
using MovieApi.Requests;

namespace MovieApi.Validators;

public sealed class GetCharacterMoviesRequestValidator : AbstractValidator<GetCharacterMoviesRequest>
{
    public GetCharacterMoviesRequestValidator()
    {
        RuleFor(x => x.CharacterId).CharacterIdPattern();
    }
}
