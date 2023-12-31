using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SkinManager.Models
{
    public partial class Skin(SkinType skinType, string subType, string name, string location, string author, string description,
        DateOnly creationDate, DateOnly lastUpdatedDate) : ObservableObject
    {
        public SkinType SkinType { get; } = skinType;
        public string SubType { get; } = subType;
        public string Name { get; } = name;
        public string Location { get; } = location;
        public string Author { get; set; } = author;
        public string Description { get; set; } = description;
        public bool IsOriginal => Location.ToLower().Contains("originals", StringComparison.OrdinalIgnoreCase);
        public DateOnly CreationDate { get; set; } = creationDate;
        public DateOnly LastUpdatedDate { get; set; } = lastUpdatedDate;

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Skin);
        }

        public bool Equals(Skin? obj)
        {
            return obj is not null &&
                SkinType == obj.SkinType &&
                SubType == obj.SubType &&
                Name == obj.Name &&
                Location == obj.Location &&
                Author == obj.Author &&
                Description == obj.Description &&
                CreationDate == obj.CreationDate &&
                LastUpdatedDate == obj.LastUpdatedDate;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SkinType, SubType, Name, Location, Author, Description, CreationDate, LastUpdatedDate);
        }

        public static bool operator ==(Skin skin1, Skin skin2)
        {
            if (skin1 is null && skin2 is null)
            {
                return true;
            }
            else
            {
                return skin1 is not null && skin1.Equals(skin2);
            }
        }

        public static bool operator !=(Skin skin1, Skin skin2)
        {
            return !(skin1 == skin2);
        }
    }
}
