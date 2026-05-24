namespace BackendTemplate.Application.TodoItems.UpdateTodoItem;

using BackendTemplate.Domain.TodoItems;
using FluentValidation;

public sealed class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TodoItem.TitleMaxLength)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength)
            .When(x => x.Description is not null && x.Description.Length > 0);
    }
}
