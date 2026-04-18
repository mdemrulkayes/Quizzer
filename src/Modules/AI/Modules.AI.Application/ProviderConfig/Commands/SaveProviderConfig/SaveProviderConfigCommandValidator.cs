using FluentValidation;

namespace Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;

public sealed class SaveProviderConfigCommandValidator : AbstractValidator<SaveProviderConfigCommand>
{
    private static readonly string[] SupportedProviders = ["gemini", "groq"];

    public SaveProviderConfigCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.")
            .Must(id => SupportedProviders.Contains(id))
            .WithMessage("Provider must be one of: gemini, groq.");

        RuleFor(x => x.SecretKey)
            .NotEmpty().WithMessage("API key is required.")
            .MinimumLength(10).WithMessage("API key appears too short.");
    }
}
