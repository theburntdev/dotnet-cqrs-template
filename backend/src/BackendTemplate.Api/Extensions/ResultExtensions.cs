namespace BackendTemplate.Api.Extensions;

using BackendTemplate.Domain.Common;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : MapFailure(result.Error, result.Kind);

    public static IResult ToHttpResult<T>(this Result<T> result)
        => result.ToHttpResult(value => TypedResults.Ok(value));

    public static IResult ToHttpResult(this Result result)
        => result.IsSuccess
            ? TypedResults.NoContent()
            : MapFailure(result.Error, result.Kind);

    private static IResult MapFailure(string error, ErrorKind kind)
        => kind switch
        {
            ErrorKind.NotFound => TypedResults.Problem(error, statusCode: 404),
            ErrorKind.Conflict => TypedResults.Problem(error, statusCode: 409),
            _ => TypedResults.Problem(error, statusCode: 422)
        };
}
