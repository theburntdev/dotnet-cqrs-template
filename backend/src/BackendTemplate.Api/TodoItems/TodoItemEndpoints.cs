using BackendTemplate.Api.Common;
using BackendTemplate.Application.TodoItems.CreateTodoItem;
using BackendTemplate.Application.TodoItems.DeleteTodoItem;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Application.TodoItems.GetTodoItems;
using BackendTemplate.Application.TodoItems.UpdateTodoItem;
using BackendTemplate.Domain.TodoItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.TodoItems;

public static class TodoItemEndpoints
{
    public static IEndpointRouteBuilder MapTodoItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/todo-items").WithTags("TodoItems");

        group.MapPost("/", CreateTodoItem);
        group.MapGet("/", GetTodoItems);
        group.MapGet("/{id:guid}", GetTodoItem);
        group.MapPatch("/{id:guid}", UpdateTodoItem);
        group.MapDelete("/{id:guid}", DeleteTodoItem);

        return app;
    }

    private static async Task<IResult> CreateTodoItem(
        CreateTodoItemRequest request,
        IMediator mediator,
        TodoItemMapper mapper,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CreateTodoItemCommand(request.Title), ct);
        return result.ToHttpResult(item =>
            TypedResults.Created($"/todo-items/{item.Id.Value}", mapper.ToResult(item)));
    }

    private static async Task<IResult> GetTodoItems(
        IMediator mediator,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TodoStatus? status = null)
    {
        var result = await mediator.Send(new GetTodoItemsQuery(page, pageSize, status), ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetTodoItem(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetTodoItemQuery(id), ct);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> UpdateTodoItem(
        Guid id,
        UpdateTodoItemRequest request,
        IMediator mediator,
        TodoItemMapper mapper,
        CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateTodoItemCommand(id, request.Title, request.Status), ct);
        return result.ToHttpResult(item => TypedResults.Ok(mapper.ToResult(item)));
    }

    private static async Task<IResult> DeleteTodoItem(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteTodoItemCommand(id), ct);
        return result.ToHttpResult(_ => TypedResults.NoContent());
    }
}
