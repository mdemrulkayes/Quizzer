using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Core.Tag;
using Shared.Core;

namespace Modules.Quiz.Application.Tag.Delete;
internal sealed class DeleteTagCommandHandler(ITagRepository repository, [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<DeleteTagCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteTagCommand request, CancellationToken cancellationToken = default)
    {
        var tag = await repository.FirstOrDefaultAsync(x => x.TagId == request.TagId);

        if (tag == null)
        {
            return TagErrors.TagNotFound;
        }

        repository.Delete(tag);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
