using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace CharacterLeveling
{
    public static class LevelingDefs
    {
        // level
        public static float config_maxLevel = 150;
        public static int config_spendingPointsPerLevel = 3;
        public static float config_requiredXPToLevelUp = 250;
        public static float config_requiredXPToLevelUpMultiplierPerLevel = 0.05f;
        public static Vector3 config_levelBarOffsetMultiplier = new Vector3(1, 1, 1);

        public static float config_xpadd_onItemSalvage = 2.5f;
        public static float config_xpadd_onAIKill = 15;
        public static float config_xpadd_onItemLoot = 2.5f;
        public static float config_xpadd_onItemChop = 2.5f;
        public static float config_xpadd_onItemCraftPerItemRequirement = 1f;
        public static float config_xpadd_onBuildPerItemRequirement = 2.5f;

        // stats
        public static float config_health_increasePerPoint = 0.35f;
        public static float config_stamina_increasePerPoint = 0.35f;
        public static float config_oxygen_increasePerPoint = 0.5f;
        public static float config_swimming_increasePerPoint = 0.1f;
        public static float config_walkrun_increasePerPoint = 0.1f;
        public static float config_lootSpeed_increasePerPoint = 0.05f;
        public static float config_salvageYield_newItemCountPerPoint = 0.35f;
        public static float config_salvageYield_newItemChance = 5; // chance from 1-10

        


        public static Sprite icon_health;
        public static Sprite icon_stamina;
        public static Sprite icon_oxygen;
        public static Sprite icon_swimming;
        public static Sprite icon_salvage;
        public static Sprite icon_plus;
        public static Sprite icon_tipJar;

        public static AudioClip audioClip_levelingBookOpen;
        public static AudioClip audioClip_spendPoint;

        public static GameObject uiprefab_characterLevelingBook;
        public static XmlWriter NewXmlWriter(this string file)
        {
            return XmlWriter.Create(file, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            });
        }

        public static float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
        {
            double scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
            return (float)(newStart + ((value - originalStart) * scale));
        }
    }
}
