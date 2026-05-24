namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

using BackendTemplate.Application.Common;
using BackendTemplate.Application.TodoItems;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;

public sealed class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Result<TodoItem>>
{
    private readonly ITodoItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTodoItemCommandHandler(ITodoItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TodoItem>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = TodoItem.Create(request.Title, request.Description);
        await _repository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TodoItem>.Success(item);
    }
}
