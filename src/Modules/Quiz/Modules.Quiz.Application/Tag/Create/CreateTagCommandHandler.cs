using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core.Tag;
using Shared.Core;

namespace Modules.Quiz.Application.Tag.Create;
internal sealed class CreateTagCommandHandler(ITagRepository repository, [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<CreateTagCommand, Result<TagResponse>>
{
    public async Task<Result<TagResponse>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = Core.Tag.Tag.Create(request.Name, request.Description);

        if (!tag.IsSuccess || tag.Value is null)
        {
            return tag.Error;
        }

        var dataTag = tag.Value;

        repository.Add(dataTag);
        await unitOfWork.CommitAsync(cancellationToken);

        return dataTag.ToResponse();
    }
}
