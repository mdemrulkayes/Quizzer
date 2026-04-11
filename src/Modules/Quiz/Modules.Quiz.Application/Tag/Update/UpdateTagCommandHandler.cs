using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core.Tag;
using Shared.Core;

namespace Modules.Quiz.Application.Tag.Update;
internal sealed class UpdateTagCommandHandler(ITagRepository repository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<UpdateTagCommand, Result<TagResponse>>
{
    public async Task<Result<TagResponse>> Handle(UpdateTagCommand request, CancellationToken cancellationToken = default)
    {
        var tag = await repository.FirstOrDefaultAsync(x => x.TagId == request.TagId);
        if (tag == null)
        {
            return TagErrors.TagNotFound;
        }

        var updatedTag = tag.Update(request.Name, request.Description);

        if (!updatedTag.IsSuccess || updatedTag.Value is null)
        {
            return updatedTag.Error;
        }

        repository.Update(updatedTag.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return updatedTag.Value.ToResponse();
    }
}
