using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Domain.Core.Data;

public static class MapLoader
{
    public static HexMap LoadMap(TextAsset jsonFile)
    {
        if (jsonFile == null)
        {
            Debug.LogError("Map file is null!");
            return null;
        }

        var def = JsonConvert.DeserializeObject<HexMapDefinition>(jsonFile.text);
        var map = new HexMap();

        int r = 0;
        foreach (var row in def.Cells)
        {
            var gridRow = new List<HexTile>();
            map.Grid.Add(gridRow);
            int c = 0;
            foreach (var cell in row)
            {
                HexState state = ConvertToState(cell);
                gridRow.Add(new HexTile(r, c, state));
                c++;
            }
            r++;
        }
        map.RegisterSpecialTiles();
        return map;
    }

    private static HexState ConvertToState(int val)
    {
        return val switch
        {
            1 => HexState.Walkable,
            2 => HexState.Tower,
            3 => HexState.Trap,
            4 => HexState.Spawn,
            5 => HexState.Target,
            _ => HexState.None
        };
    }
}