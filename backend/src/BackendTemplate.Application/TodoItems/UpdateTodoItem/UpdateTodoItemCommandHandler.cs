namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, Result<TodoItem>>
{
    private readonly ITodoItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTodoItemCommandHandler(ITodoItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TodoItem>> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result<TodoItem>.Failure($"Todo item '{request.Id.Value}' not found.", ErrorKind.NotFound);

        if (request.Title is not null)
            item.UpdateTitle(request.Title);

        if (request.Description is not null)
            item.UpdateDescription(request.Description.Length == 0 ? null : request.Description);

        if (request.Status is not null)
            item.UpdateStatus(request.Status.Value);

        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TodoItem>.Success(item);
    }
}
