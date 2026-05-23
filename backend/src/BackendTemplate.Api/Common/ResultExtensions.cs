using BackendTemplate.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace BackendTemplate.Api.Common;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
        where T : notnull
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return result.Kind switch
        {
            ErrorKind.NotFound => Results.Problem(
                title: "Not Found",
                detail: result.Error,
                statusCode: StatusCodes.Status404NotFound),
            ErrorKind.Conflict => Results.Problem(
                title: "Conflict",
                detail: result.Error,
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Validation Error",
                detail: result.Error,
                statusCode: StatusCodes.Status422UnprocessableEntity)
        };
    }
}
