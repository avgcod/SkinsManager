using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkinManager.Models
{
    public partial class SkinType : ObservableObject
    {
        public string Name { get; } = string.Empty;
        [ObservableProperty]
        private List<string> _subTypes = [];

        public SkinType(string name, IEnumerable<string> subTypes)
        {
            Name = name;
            SubTypes = subTypes.ToList();
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
