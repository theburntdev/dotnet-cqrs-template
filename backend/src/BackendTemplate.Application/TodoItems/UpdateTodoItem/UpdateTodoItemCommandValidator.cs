using FluentValidation;

namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

public sealed class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);
    }
}
