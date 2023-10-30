using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SkinManager.Models
{
    public partial class Skin : ObservableObject
    {
        public SkinType SkinType { get; }
        public string SubType { get; } = string.Empty;
        public string Name { get; } = string.Empty;
        public string Location { get; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsOriginal => Location.ToLower().Contains("originals");
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;

        public Skin(SkinType skinType, string subType, string name, string location, string author, string description,
            DateTime creationDate, DateTime lastUpdatedDate)
        {
            SkinType = skinType;
            SubType = subType;
            Name = name;
            Location = location;
            Author = author;
            Description = description;
            CreationDate = creationDate;
            LastUpdatedDate = lastUpdatedDate;
        }

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
