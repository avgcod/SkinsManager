using System;
using SkinManager.Models;

namespace SkinManager.Extensions;

//Credit Anay Kamat at  to http://anaykamat.com/2009/08/09/simple-equivalent-of-with-statement-in-c-sharp/
public static class MetaExtensions
{
    public static Maybe<T> With<T, R>(this T obj, params (string PropertyName, R PropertyValue)[] properties)
    {
        if (obj is { } validObject)
        {
            foreach ((string PropertyName, R PropertyValue) propertyInfo in properties)
            {
                validObject.GetType()
                    .GetProperty(propertyInfo.PropertyName)?
                    .SetValue(validObject, propertyInfo.PropertyValue, null);
            }

            return Something<T>.Create(validObject);
        }
        else
        {
            return Nothing<T>.Create();
        }
    }

    /*
    public static void Set(this object obj, params Func<string, object>[] hash)
    {
        foreach (Func<string, object> member in hash)
        {
            var propertyName = member.Method.GetParameters()[0].Name;
            var propertyValue = member(string.Empty);
            obj.GetType()
                .GetProperty(propertyName)?
                .SetValue(obj, propertyValue, null);
        }
    }
    */
}