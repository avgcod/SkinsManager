﻿using System;
using System.Collections.Generic;

namespace SkinManager.Models;

public record GameInfo(string GameName, string SkinsLocation, string GameLocation, string GameExecutable, List<string> AppliedSkins,
List<SkinType> SkinTypes, bool StructureCreated)
{
    public static GameInfo Create(string gameName, string skinsLocation, string gameLocation,
        string gameExecutable, List<string> appliedSkins, List<SkinType> skinTypes, bool structureCreated)
        => new GameInfo(gameName, skinsLocation, gameLocation, gameExecutable, appliedSkins, skinTypes, structureCreated);
}