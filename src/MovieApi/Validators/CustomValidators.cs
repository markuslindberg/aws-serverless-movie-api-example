using FluentValidation;

namespace MovieApi.Validators;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> MovieIdPattern<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches("^[A-Za-z0-9\\-]{1,100}$")
            .WithMessage("MovieId must be alphanumeric with length between 1 and 100");
    }
}