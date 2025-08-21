using Nautilus.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaExpandedLoreFriendly
{
    [System.Serializable]
    public class VESavedData
    {
        public bool saved = false;
        public bool any_player_base_found = false;
    };

    internal class VESaveData : SaveDataCache
    {
        public static Action<VESaveData> onSaveDataLoaded;
        public VESavedData savedData;
        public static VESaveData Get;

        public override Task LoadAsync(bool createFileIfNotExist = true)
        {
            Plugin.Log("#temp world loaded", false, true);
            return base.LoadAsync(createFileIfNotExist);
        }

        public override Task SaveAsync()
        {
            Plugin.Log("#temp world saved", false, true);
            return base.SaveAsync();
        }
        public override void Load(bool createFileIfNotExist = true)
        {
            base.Load(createFileIfNotExist);

            // create new save data if it doesn't exist
            if (savedData == null) { savedData = new VESavedData(); }
            Plugin.Log("#temp world loaded", false, true);
        }

        public override void Save()
        {
            
            base.Save();
        }
    }
}
