using System;
using LinqToDB.Mapping;
using System.Collections.Generic;

namespace SkinManager.Models
{
    [Table("GameInfo")]
    public class GameInfo
    {
        [PrimaryKey, Identity]
        public int GameID { get; set; }

        [Column, NotNull]
        public string GameName  {get;set;} = string.Empty;

        [Column, NotNull]
        public string SkinsLocation  {get;set;} = string.Empty;

        [Column, NotNull]
        public string GameLocation  {get;set;} = string.Empty;

        [Column, NotNull]
        public string GameExecutable  {get;set;} = string.Empty;

        public List<string> AppliedSkins { get; set; } = [];
        public List<SkinType> SkinTypes { get; set; } = [];
    }
}
