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
            foreach (SpriteRenderer renderer in gobj.GetComponents<SpriteRenderer>())
            {
                if (renderer.name == "coralReefRenderer")
                {
                    return;
                }
            }

            GameObject rendererObject = GameObject.Instantiate(tile.);
            SpriteRenderer reefRenderer = gobj.AddComponent<SpriteRenderer>();
            if (reefRenderer != null)
            {
                reefRenderer.name = "coralReefRenderer";

                string style = "";
                if (tile.data.Skin != SkinType.None || tile.data.Skin != SkinType.Default) style = tile.data.Skin.ToString().ToLower();
                else if (tile.data.climate != TribeType.None || tile.data.climate != TribeType.Nature) style = tile.data.climate.ToString().ToLower();

                reefRenderer.sprite = PolyMod.Registry.GetSprite("coralreef", style);
                reefRenderer.sortingLayerID = 0;
                /*
                reefRenderer.sortingOrder = tile.algaeRenderer.sortingOrder;
                modLogger.LogInfo($"L{reefRenderer.sortingLayerID} O{reefRenderer.sortingOrder} TL{tile.terrainRenderer.SpriteRenderer.SortingLayer} TO{tile.terrainRenderer.SpriteRenderer.SortingOrder}");
                if (tile.Improvement != null)
                {
                    modLogger.LogInfo($"IL{tile.Improvement.SpriteRenderer.SortingLayer} IO{tile.Improvement.SpriteRenderer.SortingOrder} IN{tile.data.improvement.type.ToString()}");
                }*/
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Tile), nameof(Tile.Depth), MethodType.Setter)]
    private static void DepthSetter(Tile __instance, int value)
    {
        GameObject gobj = __instance.gameObject;
        if (gobj != null)
        {
            foreach (SpriteRenderer renderer in gobj.GetComponents<SpriteRenderer>())
            {
                if (renderer.name == "coralReefRenderer")
                {
                    renderer.sortingOrder = value + 4;
                    modLogger.LogInfo($"{value}|{renderer.sortingOrder}");
                }
            }
        }
    }

    public static void ShowCoral(Tile tile, bool show)
    {
        GameObject gobj = tile.gameObject;
        if (gobj != null)
        {
            foreach (SpriteRenderer renderer in gobj.GetComponents<SpriteRenderer>())
            {
                if (renderer.name == "coralReefRenderer")
                {
                    renderer.enabled = show;
                }
            }
        }
    }
}
