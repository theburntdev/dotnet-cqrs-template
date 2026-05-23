using BackendTemplate.Domain.Common;
using MediatR;

namespace BackendTemplate.Application.TodoItems.DeleteTodoItem;

public record DeleteTodoItemCommand(Guid Id) : IRequest<Result<bool>>;
