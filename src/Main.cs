using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Polytopia;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using Il2Gen = Il2CppSystem.Collections.Generic;
using EnumsNET;

namespace SplashNClash;

public static class Main
{
    public static ManualLogSource modLogger;
    public static void Load(ManualLogSource logger)
    {
        modLogger = logger;
        modLogger.LogInfo("SplashNClash loaded");
        Harmony.CreateAndPatchAll(typeof(Main));
        PolyMod.Loader.AddPatchDataType("tileEffect", typeof(TileData.EffectType));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.MakeOcean))]
    private static void AddReefs(MapGenerator __instance, MapData map, GameState gameState, bool shouldConvertShallows = true)
    {
        foreach (TileData tile in map.tiles)
        {
            int chance = 0;
            float modifier = 1f;
            
            if (tile.terrain == Polytopia.Data.TerrainData.Type.Ocean) chance = 40;
            else if (tile.terrain == Polytopia.Data.TerrainData.Type.Water) chance = 20;

            if (__instance.random.Next(0, 101) < chance * modifier)
            {
                tile.AddEffect(EnumCache<TileData.EffectType>.GetType("coralreef"));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof (MapRenderContext))]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Render), typeof (MapRenderContext), typeof (SkinVisualsTransientData))]
    private static void RenderReef(Tile __instance)
    {
        if (__instance.IsHidden)
        {
            return;
        }
        LogNuking(__instance);
        ShowCoral(__instance, __instance.data.HasEffect(EnumCache<TileData.EffectType>.GetType("coralreef")));
    }

    public static void LogNuking(Tile tile)
    {
        GameObject gobj = tile.gameObject;
        if (gobj != null)
        {
            foreach (PolytopiaSpriteRenderer renderer in gobj.GetComponents<PolytopiaSpriteRenderer>())
            {
                if (renderer.name == "coralReefRenderer")
                {
                    renderer.SortingLayer = tile.terrainRenderer.spriteRenderer.SortingLayer;//-1327643303;
                    renderer.SortingOrder = tile.terrainRenderer.spriteRenderer.SortingOrder - 1;//-1883;
                    modLogger.LogInfo($"Refresh, Layer: {tile.terrainRenderer.spriteRenderer.sortingLayer}, Order: {tile.terrainRenderer.spriteRenderer.sortingOrder - 1}");
                    renderer.SharedMaterial = tile.terrainRenderer.spriteRenderer.SharedMaterial;
                    return;
                }
            }

            PolytopiaSpriteRenderer reefRenderer = gobj.AddComponent<PolytopiaSpriteRenderer>();
            if (reefRenderer != null)
            {
                reefRenderer.Init();
                reefRenderer.name = "coralReefRenderer";

                string style = "";
                if (tile.data.Skin != SkinType.None || tile.data.Skin != SkinType.Default) style = tile.data.Skin.ToString().ToLower();
                else if (tile.data.climate != TribeType.None || tile.data.climate != TribeType.Nature) style = tile.data.climate.ToString().ToLower();

                reefRenderer.Sprite = PolyMod.Registry.GetSprite("coralreef", style);
                reefRenderer.SortingLayer = tile.terrainRenderer.spriteRenderer.SortingLayer;//-1327643303;
                reefRenderer.SortingOrder = tile.terrainRenderer.spriteRenderer.SortingOrder - 1;//-1883;
                modLogger.LogInfo($"Layer: {tile.terrainRenderer.spriteRenderer.sortingLayer}, Order: {tile.terrainRenderer.spriteRenderer.sortingOrder - 1}");
                reefRenderer.SharedMaterial = tile.terrainRenderer.spriteRenderer.SharedMaterial;
            }
        }
    }

    public static void ShowCoral(Tile tile, bool show)
    {
        GameObject gobj = tile.gameObject;
        if (gobj != null)
        {
            foreach (PolytopiaSpriteRenderer renderer in gobj.GetComponents<PolytopiaSpriteRenderer>())
            {
                if (renderer.name == "coralReefRenderer")
                {
                    renderer.enabled = show;
                }
            }
        }
    }
}
