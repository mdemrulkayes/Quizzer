using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Queries.GetSupportedProviders;

internal sealed class GetSupportedProvidersQueryHandler
    : IQueryHandler<GetSupportedProvidersQuery, Result<List<SupportedProviderDto>>>
{
    public Task<Result<List<SupportedProviderDto>>> Handle(
        GetSupportedProvidersQuery request, CancellationToken cancellationToken)
    {
        var providers = new List<SupportedProviderDto>
        {
            new("gemini", "Google Gemini", "Google's Gemini AI model with free tier access", "gemini-2.0-flash"),
            new("groq", "Groq (Llama 3)", "Groq-hosted Llama 3 model with free tier access", "llama3-8b-8192")
        };

        Result<List<SupportedProviderDto>> result = providers;
        return Task.FromResult(result);
    }
}
