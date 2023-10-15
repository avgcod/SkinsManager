using Avalonia;
using System;
using System.Collections.Generic;
using System.Data;

namespace SkinManager.Models
{
    public class SkinType
    {
        public string Name { get; }
        public List<string> SubTypes { get; }

        public SkinType(string name, List<string> subTypes)
        {
            Name = name;
            SubTypes = subTypes;
        }

        public override string ToString()
        {
            return Name;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, SubTypes.GetHashCode());
        }

        public static bool operator ==(SkinType skinType1, SkinType skinType2)
        {
            if (skinType1 is null && skinType2 is null)
            {
                return true;
            }
            else
            {
                return skinType1 is not null && skinType1.Equals(skinType2);
            }
        }

        public static bool operator !=(SkinType skinType1, SkinType skinType2)
        {
            return !(skinType1 == skinType2);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SkinType);
        }
        public bool Equals(SkinType? obj)
        {
            return obj is not null && 
                Name == obj.Name && 
                SubTypes == obj.SubTypes;
        }
    }
}
