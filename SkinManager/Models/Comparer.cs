using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SkinManager.Models
{
    public class Comparer : IEqualityComparer<SkinType>
    {
        public bool Equals(SkinType? x, SkinType? y)
        {
            if (x is not null && x.Equals(y))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode([DisallowNull] SkinType obj)
        {
            return obj.GetHashCode();
        }
    }
}
