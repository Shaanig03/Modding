using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{
    public static class Vars
    {

        // config & language
        public static Config config = new Config(); 
        public static Lang lang = new Lang();


        // key to save
        public static KeyCode key_save = KeyCode.F5;
        public static KeyCode key_turretConfig = KeyCode.T;


        // mod folder
        public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public static string modFolder = Path.GetDirectoryName(Assembly.Location);

        // assets
        public static string assetsFolder = modFolder + @"\Assets";
        public static AssetBundle assetBundle = AssetBundle.LoadFromFile(assetsFolder + @"\ve_assets");

        // lang.json & config.json
        public static string fileLangJSON = modFolder + @"\lang.json";
        public static string fileConfigJSON = modFolder + @"\config.json";


        // vanilla solar panel
        public static GameObject vanillaSolarPanel;

        // textures
        public static Texture2D texture_alarm_dark_red;
        public static Texture2D texture_alarm_bright_red;

        // layer masks
        public static LayerMask layerMask_ignoreRaycast;
        public static LayerMask layerMask_terrain;

        // max vision radius
        public static float turretMaxVisionRadius = 1000;

        public static void Init()
        {
            layerMask_ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            layerMask_terrain = Voxeland.GetTerrainLayerMask();
            LangJSON();
            ConfigJSON();
        }

        // lang.json & config.json
        private static void LangJSON()
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
        private static void ConfigJSON()
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
            Vars.key_turretConfig = (KeyCode)Enum.Parse(typeof(KeyCode), Vars.config.key_turretConfig, true);
        }
    }


}
