using Nautilus.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaExpandedLoreFriendly.Buildables;

namespace VanillaExpandedLoreFriendly
{
    [System.Serializable]
    public class VESavedVars
    {
        public bool saved = false;
        public bool any_player_base_found = false;

        public Dictionary<string, VESavedTurretVars> savedTurrets = new Dictionary<string, VESavedTurretVars>();
    };

    [System.Serializable]
    public class VESavedTurretVars
    {
        public float visionRadius;
        public string[] targets;
    }
    internal class VESaveData : SaveDataCache
    {
        public static Action<VESaveData> onSaveDataLoaded;
        public VESavedVars savedVars;
        public static VESaveData Get;

        public override Task LoadAsync(bool createFileIfNotExist = true)
        {
            if (savedVars == null) { savedVars = new VESavedVars(); }

            SaveLoadTurrets(false);
            return base.LoadAsync(createFileIfNotExist);
        }

        public override Task SaveAsync()
        {
            SaveLoadTurrets(true);
            return base.SaveAsync();
        }

        public void SaveLoadTurrets(bool save)
        {
            if (save)
            {
                // if saving

                // clear current saved turrets
                savedVars.savedTurrets.Clear();

                TurretBase[] turrets = GameObject.FindObjectsOfType<TurretBase>();
                foreach(TurretBase _turret in turrets)
                {
                    // save turret
                    string turretUID = _turret.GetComponent<PrefabIdentifier>().id;
                    savedVars.savedTurrets.Add(turretUID, new VESavedTurretVars
                    {
                        visionRadius = _turret.targetingCore.visionRadius,
                        targets = _turret.targetingCore.targetNames
                    });
                }
            }
            else
            {
                Plugin.Get.StartCoroutine(ILoadTurrets());
            }
        }

        private System.Collections.IEnumerator ILoadTurrets()
        {
            yield return new WaitUntil(() => Player.main != null);

            // load turrets
            foreach (KeyValuePair<string, VESavedTurretVars> _keyValue in savedVars.savedTurrets)
            {
                Plugin.Get.StartCoroutine(ILoadTurret(_keyValue.Key, _keyValue.Value));
                
            }
        }
        WaitForSeconds _delay_loadTurret = new WaitForSeconds(0.05f);

        private System.Collections.IEnumerator ILoadTurret(string id, VESavedTurretVars savedTurretVars)
        {
            
            while (true)
            {
                UniqueIdentifier uniqueIdentifier;
                if (PrefabIdentifier.TryGetIdentifier(id, out uniqueIdentifier))
                {

                    TurretBase turret = uniqueIdentifier.GetComponent<TurretBase>();
                    var targetingCore = turret.targetingCore;

                    targetingCore.targetNames = savedTurretVars.targets;
                    targetingCore.visionRadius = savedTurretVars.visionRadius;
                    ErrorMessage.AddMessage($"#temp turret loaded");
                    break;
                }
                yield return _delay_loadTurret;
            }
        }
    }
}
