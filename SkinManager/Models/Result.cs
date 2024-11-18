using System;

namespace SkinManager.Models;

public class Result<T, TError>
{
    private readonly bool _success;
    public readonly Maybe<T> Value;
    public readonly Maybe<TError> Error;

    private Result(Maybe<T> v, Maybe<TError> e, bool success)
    {
        Value = v;
        Error = e;
        _success = success;
    }

    public bool Succeeded => Value is Something<T>;

    public static Result<T, TError> Success(T v)
    {
        return new(Something<T>.Create(v), Nothing<TError>.Create(), true);
    }

    public static Result<T, TError> Failure(TError e)
    {
        return new(Nothing<T>.Create(), Something<TError>.Create(e), false);
    }

    public static implicit operator Result<T, TError>(T v) => new(Something<T>.Create(v), Nothing<TError>.Create(), true);
    public static implicit operator Result<T, TError>(TError e) => new(Nothing<T>.Create(), Something<TError>.Create(e), false);

    public R Match<R>(
        Func<T, R> success,
        Func<TError, R> failure)
    {
        if (Succeeded)
        {
            Something<T> successValue = (Something<T>) Value;
            return success(successValue.Value);
        }
        else
        {
            Something<TError> errorValue = (Something<TError>) Error;
            return failure(errorValue.Value);
        }
    }
}