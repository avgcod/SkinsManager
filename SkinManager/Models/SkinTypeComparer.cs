using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Models
{
    public class SkinTypeComparer : IEqualityComparer<SkinType>
    {
        public bool Equals(SkinType? x, SkinType? y)
        {
            return x?.Name == y?.Name;
        }

        public int GetHashCode([DisallowNull] SkinType obj)
        {
            return obj.GetHashCode();
        }
    }
}
