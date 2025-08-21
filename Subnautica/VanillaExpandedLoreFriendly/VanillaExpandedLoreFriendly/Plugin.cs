using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using VanillaExpandedLoreFriendly.Items.Equipment;

namespace VanillaExpandedLoreFriendly
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }


        public static InGameConfig ingameConfig = OptionsPanelHandler.RegisterModOptions<InGameConfig>();


        public static Plugin Get;

        /*
        System.Collections.IEnumerator IStart()
        {
            WaitForSeconds delay = new WaitForSeconds(1);

            float alarmSirenVolume = -1;
            while (this != null)
            {
                if(alarmSirenVolume != ingameConfig.alarmSirenVolume) { alarmSirenVolume = ingameConfig.alarmSirenVolume; Registries.RegisterAlarmSirenSound(); }
                yield return delay;
            }
        }*/

        void Start()
        {

        }

        private void Awake()
        {
            Get = this;
            

            // set project-scoped logger instance
            Logger = base.Logger;

            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Vars.Assembly, $"{PluginInfo.PLUGIN_GUID}");

            // get vanilla prefabs
            StartCoroutine(VEMethods.GetVanillaPrefabs());

            // initialize variables
            Vars.Init();

            Vars.texture_alarm_bright_red = VEMethods.LoadTexture(Vars.assetsFolder + @"\bright_red.png", 512, 512);     // Vars.assetBundle.LoadAsset("bright_red") as Texture2D;
            Vars.texture_alarm_dark_red = VEMethods.LoadTexture(Vars.assetsFolder + @"\dark_red.png", 512, 512);

            // register all
            this.StartCoroutine(Registries.RegisterAll());

            VESaveData.Get = SaveDataHandler.RegisterSaveDataCache<VESaveData>();

            // print mod has loaded
            Log(Vars.lang.mod_has_loaded);
        }

        public static void Log(string txt, bool error = false, bool displayOnScreen = false) { 
            if (!error) { Logger.LogDebug(txt); } else { Logger.LogError($"#Error: {txt}"); }
            if (displayOnScreen) { ErrorMessage.AddMessage(txt); }
        }
    }
}