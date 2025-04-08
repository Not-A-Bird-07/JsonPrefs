using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace JsonPrefs
{
    [BepInPlugin("NOTABIRD.JSON.PREFS", "Json Prefs", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static string SavePath;
        public static Dictionary<string, object> prefs = new();

        Plugin()
        {
            new Harmony("NOTABIRD.JSON.PREFS").PatchAll();
        }

        void Awake()
        {
            Instance = this;
            SavePath = Path.Combine(BepInEx.Paths.ConfigPath, "JsonPrefs.json");

            LoadPrefs();
        }
        

        public static void LoadPrefs()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    prefs = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(SavePath));
                }
                catch
                {
                    prefs = new();
                }
            }
        }

        public static void SavePrefs()
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(prefs, Formatting.Indented));
        }
    }

    [HarmonyPatch(typeof(PlayerPrefs))]
    public class PrefsPatch
    {
        [HarmonyPatch("GetInt", new[] { typeof(string), typeof(int) }), HarmonyPrefix]
        public static bool GetIntPatch(ref int __result, string key, int defaultValue)
        {
            if (Plugin.prefs.TryGetValue(key, out var value)) 
            {
                __result = Convert.ToInt32(value);
                return false;
            }
            __result = defaultValue;
            return false;
        }

        [HarmonyPatch("GetFloat", new[] { typeof(string), typeof(float) }), HarmonyPrefix]
        public static bool GetFloatPatch(ref float __result, string key, float defaultValue)
        {
            if (Plugin.prefs.TryGetValue(key, out var value))
            {
                __result = Convert.ToSingle(value);
                return false;
            }
            __result = defaultValue;
            return false;
        }

        [HarmonyPatch("GetString", new[] { typeof(string), typeof(string) }), HarmonyPrefix]
        public static bool GetStringPatch(ref string __result, string key, string defaultValue)
        {
            if (Plugin.prefs.TryGetValue(key, out var value) && value is string)
            {
                __result = (string)value;
                return false;
            }
            __result = defaultValue;
            return false;
        }

        [HarmonyPatch("SetInt"), HarmonyPrefix]
        public static bool SetIntPatch(string key, int value)
        {
            Plugin.prefs[key] = value;
            return false;
        }

        [HarmonyPatch("SetFloat"), HarmonyPrefix]
        public static bool SetFloatPatch(string key, float value)
        {
            Plugin.prefs[key] = value;
            return false;
        }

        [HarmonyPatch("SetString"), HarmonyPrefix]
        public static bool SetStringPatch(string key, string value)
        {
            Plugin.prefs[key] = value;
            return false;
        }

        [HarmonyPatch("DeleteKey"), HarmonyPrefix]
        public static bool DeleteKeyPatch(string key)
        {
            if (Plugin.prefs.ContainsKey(key))
                Plugin.prefs.Remove(key);
            return false;
        }

        [HarmonyPatch("DeleteAll"), HarmonyPrefix]
        public static bool DeleteAllPatch()
        {
            Plugin.prefs.Clear();
            if (File.Exists(Plugin.SavePath))
                File.Delete(Plugin.SavePath);
            return false;
        }

        [HarmonyPatch("Save"), HarmonyPrefix]
        public static bool SavePatch()
        {
            Plugin.SavePrefs();
            return false;
        }
    }
}