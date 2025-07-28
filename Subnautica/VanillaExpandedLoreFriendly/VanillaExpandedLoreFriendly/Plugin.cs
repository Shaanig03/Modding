using System.Reflection;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VanillaExpandedLoreFriendly.Items.Equipment;
using Newtonsoft.Json;

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


        private void LangJson()
        {
            if (!File.Exists(fileLangJSON))
            {
                FileStream fs = new FileStream(fileLangJSON, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Close();
                File.WriteAllText(fileLangJSON, JsonConvert.SerializeObject(Vars.lang, Formatting.Indented));
            }
        }


        private void Awake()
        {
            // set project-scoped logger instance
            Logger = base.Logger;


            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");


            // create lang.json
            LangJson();


            // print mod has loaded
            Log(Vars.lang.mod_has_loaded);
        }


        public static void Log(string txt, bool error = false) { if (!error) { Logger.LogDebug(txt); } else { Logger.LogError($"#Error: {txt}"); } }



    }
}