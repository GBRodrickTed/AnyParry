using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Diagnostics;
using System.Reflection;
using HarmonyLib;
/*using PluginConfig;
using PluginConfig.API.Fields;
using PluginConfig.API;
using PluginConfig.API.Functionals;*/
using UnityEngine.EventSystems;
using System.Xml.Serialization;
//using static AnyParry.ConfigManager;

namespace AnyParry
{
    /*public static class ConfigManager
    {
        private static PluginConfigurator config;
        private static ButtonField openParryFolder;
        private static ButtonField refresh;
        public static EnumField<FilterType> filterType;
        static string workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string iconFilePath = Path.Combine(Path.Combine(workingDirectory, "Data"), "icon.png");
        public enum FilterType
        {
            Point = FilterMode.Point,
            Bilinear = FilterMode.Bilinear,
            Trilinear = FilterMode.Trilinear
        }
        public static void Setup()
        {
            config = PluginConfigurator.Create(PluginInfo.Name, PluginInfo.GUID);
            openParryFolder = new ButtonField(config.rootPanel, "Open Parry Folder", "button.openfolder");
            openParryFolder.onClick += new ButtonField.OnClick(OpenFolder);
            refresh = new ButtonField(config.rootPanel, "Refresh Parry Images", "button.refresh");
            refresh.onClick += new ButtonField.OnClick(Plugin.UpdateParryList);
            filterType = new EnumField<FilterType>(config.rootPanel, "Texture Filtering Mode", "field.filter", FilterType.Point, true);

            filterType.onValueChange += (e) =>
            {
                Plugin.UpdateParryList();
            };
            ConfigManager.config.SetIconWithURL("file://" + iconFilePath);
        }
        public static void OpenFolder()
        {
            Application.OpenURL(Path.Combine(workingDirectory, "ParryImages"));
        }
    }*/
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static List<string> imageTypes = new List<string> {".jpeg", ".jpg", ".png", ".bmp"};
        public static List<Sprite> parrySprites = new List<Sprite>();
        public static string ModDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public Harmony harm;
        public static FilterMode filterType = FilterMode.Point;
        public void Start()
        {
            //ConfigManager.Setup();
            UpdateParryList();
            harm = new Harmony(PluginInfo.GUID);
            harm.PatchAll(typeof(PatchDude));
        }

        public static void UpdateParryList()
        {
            parrySprites = new List<Sprite>();
            foreach (string file in Directory.EnumerateFiles(Path.Combine(Path.Combine(ModDir, "ParryImages"))))
            {
                for (int i = 0; i < imageTypes.Count; i++)
                {
                    if (string.Equals(imageTypes[i], Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log(file);
                        byte[] data = File.ReadAllBytes(file);
                        Texture2D parryTex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                        parryTex.filterMode = (FilterMode)filterType;
                        parryTex.LoadImage(data);
                        Sprite parrySprite = Sprite.Create(parryTex, new Rect(0, 0, parryTex.width, parryTex.height), new Vector2(0.5f, 0.5f));
                        parrySprites.Add(parrySprite);
                        break;
                    }
                }
            }
        }

        public static class PatchDude
        {
            [HarmonyPatch(typeof(TimeController), nameof(TimeController.ParryFlash))]
            [HarmonyPostfix]
            private static void SetParryImage(TimeController __instance)
            {
                if (parrySprites.Count < 1)
                {
                    __instance.parryFlash.GetComponent<Image>().sprite = null;
                } else
                {
                    __instance.parryFlash.GetComponent<Image>().sprite = parrySprites[UnityEngine.Random.Range(0, parrySprites.Count)];
                }
            }
        }
    }
}
