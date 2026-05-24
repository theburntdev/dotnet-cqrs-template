namespace BackendTemplate.Application.TodoItems.CreateTodoItem;

using BackendTemplate.Domain.TodoItems;
using FluentValidation;

public sealed class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TodoItem.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength)
            .When(x => x.Description is not null);
    }
}
