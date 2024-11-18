using System;
using System.Collections.Generic;
using System.Linq;

namespace SkinManager.Models;

public static class MaybeExtention
{
    public static Maybe<TOut> ChainMaybe<TIn,MOut, TOut>(this Maybe<TIn> @this, Func<TIn, MOut> maybeFunc) where MOut : Maybe<TOut>
    {
        return @this switch
        {
            Something<TIn> something => maybeFunc(something.Value),
            _ => new Nothing<TOut>()
        };
    }

    public static Maybe<TOut> Chain<TIn, TOut>(this Maybe<TIn> @this, Func<TIn, TOut> func)
    {
        return @this switch
        {
            Something<TIn> something => new Something<TOut> { Value = func(something.Value) },
            _ => new Nothing<TOut>()
        };
    }


    public static Maybe<TOut> Bind<TIn, TOut>(this Maybe<TIn> @this, Func<TIn, Maybe<TOut>> bind)
    {
        return @this switch
        {
            Something<TIn> something => bind(something.Value),
            _ => new Nothing<TOut>()
        };
    }

    public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> items, Func<T, bool> predicate) =>
        items
            .Where(predicate)
            .Select(Something<T>.Create)
            .DefaultIfEmpty(Nothing<T>.Create())
            .First();

    public static Maybe<TOut> Select<TIn, TOut>(this Maybe<TIn> obj, Func<TIn, TOut> map) => obj.Chain(map);

    public static Maybe<TIn> Where<TIn>(this Maybe<TIn> obj, Func<TIn, bool> predicate) =>
        obj.Bind(content => predicate(content) ? obj : Nothing<TIn>.Create());

    public static Maybe<TResult> SelectMany<TIn, TOut, TResult>(this Maybe<TIn> obj, Func<TIn, Maybe<TOut>> bind,
        Func<TIn, TOut, TResult> map) =>
        obj.Bind(original => bind(original).Chain(result => map(original, result)));
}

public abstract record Maybe<T>();

public sealed record Something<T> : Maybe<T>
{
    public required T Value { get; init; }

    public static Maybe<T> Create(T value) => new Something<T> { Value = value };

    public override string ToString() => Value!.ToString() ?? "No object in something???";
}

public sealed record Nothing<T> : Maybe<T>
{
    public static Maybe<T> Create() => new Nothing<T>();
}