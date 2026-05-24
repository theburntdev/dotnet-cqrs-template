namespace BackendTemplate.Api.Endpoints;

using BackendTemplate.Api.Extensions;
using BackendTemplate.Api.Mappers;
using BackendTemplate.Api.Models;
using BackendTemplate.Application.TodoItems.CreateTodoItem;
using BackendTemplate.Application.TodoItems.DeleteTodoItem;
using BackendTemplate.Application.TodoItems.GetTodoItem;
using BackendTemplate.Application.TodoItems.GetTodoItems;
using BackendTemplate.Application.TodoItems.UpdateTodoItem;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.TodoItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public static class TodoItemEndpoints
{
    public static IEndpointRouteBuilder MapTodoItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todo-items");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPatch("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        return app;
    }

    private static async Task<IResult> GetAll(
        ISender sender,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TodoStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetTodoItemsQuery(page, pageSize, status), ct);
        return TypedResults.Ok(PagedResponse<TodoItemDto>.From(result));
    }

    private static async Task<IResult> GetById(
        Guid id,
        ISender sender,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetTodoItemQuery(new TodoItemId(id)), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromBody] CreateTodoItemCommand command,
        ISender sender,
        CancellationToken ct = default)
    {
        var result = await sender.Send(command, ct);
        return result.ToHttpResult(item =>
            TypedResults.Created($"/api/todo-items/{item.Id.Value}", TodoItemMapper.Map(item)));
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateTodoItemRequest request,
        ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateTodoItemCommand(
            new TodoItemId(id),
            request.Title,
            request.Description,
            request.Status);
        var result = await sender.Send(command, ct);
        return result.ToHttpResult(item => TypedResults.Ok(TodoItemMapper.Map(item)));
    }

    private static async Task<IResult> Delete(
        Guid id,
        ISender sender,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new DeleteTodoItemCommand(new TodoItemId(id)), ct);
        return result.ToHttpResult();
    }
}
