using System.Reflection;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VanillaExpandedLoreFriendly.Items.Equipment;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }
        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        public static string modFolder = Path.GetDirectoryName(Assembly.Location);

        public static string fileLangJSON = modFolder + @"\lang.json";
        public static string fileConfigJSON = modFolder + @"\config.json";


        public static Plugin Get;

        // creates or loads lang.json
        private void LangJSON()
        {
            
            if (!File.Exists(fileLangJSON))
            {
                // creates a new .json file
                FileStream fs = new FileStream(fileLangJSON, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Close();
                File.WriteAllText(fileLangJSON, JsonConvert.SerializeObject(Vars.lang, Formatting.Indented));
            }
            else
            {
                // loads .json file
                Vars.lang = JsonConvert.DeserializeObject<Lang>(File.ReadAllText(fileLangJSON));
            }
        }

        // creates or loads config.json
        private void ConfigJSON()
        {
            if (!File.Exists(fileConfigJSON))
            {
                // creates a new .json file
                FileStream fs = new FileStream(fileConfigJSON, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Close();

                File.WriteAllText(fileConfigJSON, JsonConvert.SerializeObject(Vars.config, Formatting.Indented));
            }
            else
            {
                // loads .json file
                Vars.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(fileConfigJSON));
            }

            // load key save from .json
            Vars.key_save = (KeyCode)Enum.Parse(typeof(KeyCode), Vars.config.key_save, true);
        }

        private void Awake()
        {
            Get = this;

            // set project-scoped logger instance
            Logger = base.Logger;


            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");


            // create config.json
            ConfigJSON();

            // create lang.json
            LangJSON();




            // print mod has loaded
            Log(Vars.lang.mod_has_loaded);
        }


        public static void Log(string txt, bool error = false) { if (!error) { Logger.LogDebug(txt); } else { Logger.LogError($"#Error: {txt}"); } }



    }
}