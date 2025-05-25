using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Xml;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace CharacterLeveling
{
    [BepInPlugin(ModInfo.GUID, ModInfo.Name, ModInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }
        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public static BaseUnityPlugin plugin;


        public static string modFolder = Path.GetDirectoryName(Assembly.Location);
        public static string configFile = modFolder + @"\config.xml";

        public static string assembyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string assetsFolder = assembyLocation + @"\" + "Assets";

        public static AssetBundle assetBundle;

        private void SetupConfig()
        {
            if (File.Exists(configFile))
            {
                // load config file
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(configFile);
                XmlNode config = xdoc.SelectSingleNode("config");

                LevelingDefs.config_maxLevel = float.Parse(config.SelectSingleNode("config_maxLevel").InnerText);
                LevelingDefs.config_spendingPointsPerLevel = int.Parse(config.SelectSingleNode("config_spendingPointsPerLevel").InnerText);
                LevelingDefs.config_requiredXPToLevelUp = float.Parse(config.SelectSingleNode("config_requiredXPToLevelUp").InnerText);
                LevelingDefs.config_requiredXPToLevelUpMultiplierPerLevel = float.Parse(config.SelectSingleNode("config_requiredXPToLevelUpMultiplierPerLevel").InnerText);

                LevelingDefs.config_xpadd_onItemSalvage = float.Parse(config.SelectSingleNode("config_xpadd_onItemSalvage").InnerText);
                LevelingDefs.config_xpadd_onAIKill = float.Parse(config.SelectSingleNode("config_xpadd_onAIKill").InnerText);
                LevelingDefs.config_xpadd_onItemLoot = float.Parse(config.SelectSingleNode("config_xpadd_onItemLoot").InnerText);

                LevelingDefs.config_health_increasePerPoint = float.Parse(config.SelectSingleNode("config_health_increasePerPoint").InnerText);
                LevelingDefs.config_stamina_increasePerPoint = float.Parse(config.SelectSingleNode("config_stamina_increasePerPoint").InnerText);
                LevelingDefs.config_oxygen_increasePerPoint = float.Parse(config.SelectSingleNode("config_oxygen_increasePerPoint").InnerText);
                LevelingDefs.config_swimming_increasePerPoint = float.Parse(config.SelectSingleNode("config_swimming_increasePerPoint").InnerText);
                LevelingDefs.config_walkrun_increasePerPoint = float.Parse(config.SelectSingleNode("config_walkrun_increasePerPoint").InnerText);
                LevelingDefs.config_lootSpeed_increasePerPoint = float.Parse(config.SelectSingleNode("config_lootSpeed_increasePerPoint").InnerText);
                LevelingDefs.config_salvageYield_newItemCountPerPoint = float.Parse(config.SelectSingleNode("config_salvageYield_newItemCountPerPoint").InnerText);
                LevelingDefs.config_salvageYield_newItemChance = float.Parse(config.SelectSingleNode("config_salvageYield_newItemChance").InnerText);
            } else
            {
                // create a new config file
                XmlWriter writer = LevelingDefs.NewXmlWriter(configFile);
                writer.WriteStartElement("config");

                writer.WriteElementString("config_maxLevel", LevelingDefs.config_maxLevel.ToString());
                writer.WriteElementString("config_spendingPointsPerLevel", LevelingDefs.config_spendingPointsPerLevel.ToString());
                writer.WriteElementString("config_requiredXPToLevelUp", LevelingDefs.config_requiredXPToLevelUp.ToString());
                writer.WriteElementString("config_requiredXPToLevelUpMultiplierPerLevel", LevelingDefs.config_requiredXPToLevelUpMultiplierPerLevel.ToString());

                writer.WriteElementString("config_xpadd_onItemSalvage", LevelingDefs.config_xpadd_onItemSalvage.ToString());
                writer.WriteElementString("config_xpadd_onAIKill", LevelingDefs.config_xpadd_onAIKill.ToString());
                writer.WriteElementString("config_xpadd_onItemLoot", LevelingDefs.config_xpadd_onItemLoot.ToString());

                writer.WriteElementString("config_health_increasePerPoint", LevelingDefs.config_health_increasePerPoint.ToString());
                writer.WriteElementString("config_stamina_increasePerPoint", LevelingDefs.config_stamina_increasePerPoint.ToString());
                writer.WriteElementString("config_oxygen_increasePerPoint", LevelingDefs.config_oxygen_increasePerPoint.ToString());
                writer.WriteElementString("config_swimming_increasePerPoint", LevelingDefs.config_swimming_increasePerPoint.ToString());
                writer.WriteElementString("config_walkrun_increasePerPoint", LevelingDefs.config_walkrun_increasePerPoint.ToString());
                writer.WriteElementString("config_lootSpeed_increasePerPoint", LevelingDefs.config_lootSpeed_increasePerPoint.ToString());
                writer.WriteElementString("config_salvageYield_newItemCountPerPoint", LevelingDefs.config_salvageYield_newItemCountPerPoint.ToString());
                writer.WriteElementString("config_salvageYield_newItemChance", LevelingDefs.config_salvageYield_newItemChance.ToString());

                writer.WriteEndElement();
                writer.Close();
            }
        }


        Sprite LoadTextureToSprite(string file)
        {
            Texture2D texture2D = IMG2Sprite.LoadTexture(file);
            return IMG2Sprite.ConvertTextureToSprite(texture2D);
        }

        async void ASAwake()
        {
            LevelingDefs.audioClip_levelingBookOpen = await LoadClip(assetsFolder + @"\lvlingbook_open.wav");
            LevelingDefs.audioClip_spendPoint = await LoadClip(assetsFolder + @"\spendpoint.wav");
            
        }
        private void Awake()
        {
            Logger = base.Logger;
            plugin = this;

            SetupConfig();

            LevelingDefs.icon_health = LoadTextureToSprite(assetsFolder + @"\Health_Normal.png");
            LevelingDefs.icon_stamina = LoadTextureToSprite(assetsFolder + @"\Stamina_Normal.png");
            LevelingDefs.icon_oxygen = LoadTextureToSprite(assetsFolder + @"\ICON AIR CANISTER.png");
            LevelingDefs.icon_swimming = LoadTextureToSprite(assetsFolder + @"\ICON SWIMMING.png");
            LevelingDefs.icon_salvage = LoadTextureToSprite(assetsFolder + @"\sack.png");
            LevelingDefs.icon_plus = LoadTextureToSprite(assetsFolder + @"\Plus.png");
            LevelingDefs.icon_tipJar = LoadTextureToSprite(assetsFolder + @"\tipjar.png");

            ASAwake();



           
            Harmony.CreateAndPatchAll(Assembly, $"{ModInfo.GUID}");

            Logger.LogDebug($"Mod successfully loaded");
        }

        async Task<AudioClip> LoadClip(string path)
        {
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
            {
                uwr.SendWebRequest();

                // wrap tasks in try/catch, otherwise it'll fail silently
                try
                {
                    while (!uwr.isDone) await Task.Delay(5);

                    if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }

            return clip;
        }
    }
}
