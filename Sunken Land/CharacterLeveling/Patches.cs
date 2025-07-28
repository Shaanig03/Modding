using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using Fusion;
using Cysharp.Threading.Tasks;
using System.Reflection;

namespace CharacterLeveling
{

    

    [HarmonyPatch(typeof(BuildController))]
    internal class BuildControllerPatches
    {
        [HarmonyPatch(nameof(BuildController.OnBuildBuilding))]
        [HarmonyPostfix]
        public static void OnBuildBuilding(UIBuilding __instance, BuildingPiece piece)
        {
            CharacterLeveling characterLeveling = CharacterLeveling.Instance;
            if (characterLeveling == null) { return; }

            Plugin.Logger.LogDebug($"#temp OnBuildBuilding");
            foreach (var _requirement in piece.Craftable.itemRequirements)
            {
                Plugin.Logger.LogDebug($"#temp {_requirement.item} {_requirement.amount}x");
                characterLeveling.xp += LevelingDefs.config_xpadd_onBuildPerItemRequirement * _requirement.amount;
            }
        }

    }

    [HarmonyPatch(typeof(UIBuilding))]
    internal class UIBuildingPatches
    {
        [HarmonyPatch(nameof(UIBuilding.OnItemCrafted))]
        [HarmonyPostfix]
        public static void OnItemCrafted(UIBuilding __instance, Item item, bool isNetwork)
        {
            var requirements = item.Craftable.itemRequirements;
            Plugin.Logger.LogDebug($"#temp UIBuilding item crafted {item.DisplayName}");
            CharacterLeveling characterLeveling = CharacterLeveling.Instance;
            if (characterLeveling == null) { return; }

            foreach (var _requirement in requirements)
            {
                characterLeveling.xp += LevelingDefs.config_xpadd_onItemCraftPerItemRequirement * _requirement.amount;

                Plugin.Logger.LogDebug($"#temp requirement '{_requirement.item}' {_requirement.amount}");
            }
        }
        
    }

    [HarmonyPatch] // at least one Harmony annotation makes Harmony not skip this patch class when calling PatchAll()
    class PatchChoppableChopB
    {
        // here, inside the patch class, you can place the auxiliary patch methods
        // for example TargetMethod:

        public static MethodBase TargetMethod()
        {
            return typeof(Choppable).GetMethods().Find(x => x.Name == "Chop" && x.GetParameters()[1].Name.Contains("chopPlayerID"));
        }

        // your patches
        [HarmonyPrefix]
        public static bool Prefix(Choppable __instance, Vector3 chopPosition, int chopPlayerID, int _damage)
        {
            if (__instance.ChopAmount <= 0)
            {
                return true;
            }

            float num = (float)_damage;
            //this.RPC_EntityChopEvent(RPGCamera.code.transform.forward, (int)num, chopPosition, chopPlayerID, true);

            float CumulativeInjury = __instance.CumulativeInjury + num;

            if (CumulativeInjury >= __instance.CumulativeInjuryValue && __instance.generatedDropItems.Count > 0)
            {
                CharacterLeveling characterLeveling = CharacterLeveling.Instance;
                if (characterLeveling != null)
                {
                    characterLeveling.xp += LevelingDefs.config_xpadd_onItemChop;
                }
            }


            return true;
        }
    }

    [HarmonyPatch] // at least one Harmony annotation makes Harmony not skip this patch class when calling PatchAll()
    class PatchChoppableChopA
    {
        // here, inside the patch class, you can place the auxiliary patch methods
        // for example TargetMethod:

        public static MethodBase TargetMethod()
        {
            return typeof(Choppable).GetMethods().Find(x => x.Name == "Chop" && x.GetParameters()[1].Name.Contains("processor"));
        }

        // your patches
        [HarmonyPrefix]
        public static bool Prefix(Choppable __instance, Vector3 chopPosition, PlayerMeleeWeaponProcessor processor, int chopPlayerID)
        {
            if (__instance.ChopAmount <= 0)
            {
                return true;
            }
            float loggingValue = Global.code.Player.weaponInHand.LoggingValue;
            bool flag = false;
            switch (__instance.CollectableType)
            {
                case CollectableObjectType.Tree:
                    flag = processor.CutTree;
                    break;
                case CollectableObjectType.Mining:
                    flag = processor.Mining;
                    break;
                case CollectableObjectType.SmallTree:
                    flag = (processor.CutSmallTree || processor.CutTree);
                    break;
                case CollectableObjectType.BreakableObject:
                    flag = processor.BreakObject;
                    break;
            }
            if (flag)
            {
                float CumulativeInjury = __instance.CumulativeInjury + loggingValue;
                if (CumulativeInjury >= __instance.CumulativeInjuryValue && __instance.generatedDropItems.Count > 0)
                {
                    CharacterLeveling characterLeveling = CharacterLeveling.Instance;
                    if (characterLeveling != null)
                    {
                        characterLeveling.xp += LevelingDefs.config_xpadd_onItemChop;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Scavengeable))]
    internal class ScavengeablePatches
    {
        [HarmonyPatch(nameof(Scavengeable.Chop))]
        [HarmonyPrefix]
        public static bool Interact(Scavengeable __instance, Vector3 chopPosition, PlayerMeleeWeaponProcessor processor, int ScavengeCount = 1)
        {
            if (__instance.ChopAmount <= 0)
            {
                return true;
            }
            bool flag = false;
            if (processor.BreakObject)
            {
                flag = true;
            }
            if (flag)
            {
                for (int i = 0; i < ScavengeCount; i++)
                {
                    if (__instance.nextItemIndex < __instance.generatedDropItems.Count)
                    {
                        CharacterLeveling characterLeveling = CharacterLeveling.Instance;
                        if(characterLeveling != null)
                        {
                            characterLeveling.xp += LevelingDefs.config_xpadd_onItemChop;
                        }
                    }
                }
            }
            return true;
        }

    }


    [HarmonyPatch(typeof(Chest))]   
    internal class ChestPatches
    {
        [HarmonyPatch(nameof(Chest.Interact))]
        [HarmonyPostfix]
        public static void Interact(Chest __instance, bool needGenerateLoots = true)
        {
            // if chest is opened
            if(__instance.opened)
            {
                CharacterLeveling characterLeveling = CharacterLeveling.Instance;

                if(characterLeveling != null)
                {
                    // add xp for each contained item
                    float xpvalue = LevelingDefs.config_xpadd_onItemLoot * __instance.GetComponent<Storage>().Items.Count(x => x != null);
                    characterLeveling.xp += xpvalue;
                }
            }
        }
   
    }

    [HarmonyPatch(typeof(Character))]
    internal class CharacterPatches
    {
        [HarmonyPatch(nameof(Character.TakeDamage))]
        [HarmonyPostfix]
        public static void TakeDamage(Character __instance, DamageInfo damageInfo)
        {
            // if ai is dead
            if (__instance.IsDead)
            {
                // get damage source
                var source = damageInfo.Source;
                if(source != null)
                {
                    // get character leveling component and if found
                    CharacterLeveling characterLeveling = source.GetComponent<CharacterLeveling>();
                    if (characterLeveling != null)
                    {
                        characterLeveling.xp += LevelingDefs.config_xpadd_onAIKill;
                    }
                }
            }
        }

    }


            [HarmonyPatch(typeof(CollectableContinuingInteraction))]
    internal class CollectableContinuingInteractionPatches
    {
        [HarmonyPatch(nameof(CollectableContinuingInteraction.Interact))]
        [HarmonyPrefix]
        public static bool Interact(CollectableContinuingInteraction __instance, float interactingTimer)
        {
            
            Interaction _interaction = Traverse.Create(__instance).Field("_interaction").GetValue<Interaction>();
            
            /*
            if (!_interaction)
            {
                return true;
            }
            if (!__instance.OnStartInteractionTriggered)
            {
                __instance.OnStartInteraction();
            }
            if (!__instance.CanBeInteracted)
            {
                __instance.alreadyInteractedDuration = 0f;
                return true;
            }
            */

            if (__instance.alreadyInteractedDuration + interactingTimer < __instance.nextItemDropTime)
            {
                return true;
            }

            if (__instance.nextItemIndex < __instance.generatedDropItems.Count)
            {
                CharacterLeveling instance = CharacterLeveling.Instance;
                if (instance != null)
                {
                    float xpaddvalue = LevelingDefs.config_xpadd_onItemSalvage;
                    instance.xp += xpaddvalue;
                }
            }
            
            return true;
        }


        [HarmonyPatch(nameof(CollectableContinuingInteraction.Interact))]
        [HarmonyPostfix]
        public static void InteractPost(CollectableContinuingInteraction __instance, float interactingTimer)
        {
            Interaction _interaction = Traverse.Create(__instance).Field("_interaction").GetValue<Interaction>();

            if (__instance.alreadyInteractedDuration + interactingTimer < __instance.nextItemDropTime)
            {
                return;
            }
            __instance.nextItemDropTime = (_interaction.InteractionTime / __instance.generatedDropItems.Count) * (__instance.nextItemIndex + 1);

            // __instance.nextItemDropTime = _interaction.InteractionTime / (float)__instance.generatedDropItems.Count * (float)(__instance.nextItemIndex + 1);
        }

        //

        [HarmonyPatch(nameof(CollectableContinuingInteraction.OnLootCollected))]
        [HarmonyPrefix]
        public static bool OnLootCollected(CollectableContinuingInteraction __instance)
        {
            
            ModifiedCollectable modifiedCollectable = __instance.GetComponent<ModifiedCollectable>();


            if(modifiedCollectable != null)
            {
                float xpaddvalue = LevelingDefs.config_xpadd_onItemSalvage;

                int i = 0;
                // loop through each salvage yield items
                foreach (Item _item in modifiedCollectable.salvageYieldItems)
                {
                    // add item to player inventory
                    Transform transform = Utility.Instantiate(_item.transform, null);
                    transform.GetComponent<Item>().Amount = 1;

                    var characterLeveling = CharacterLeveling.Instance;
                    if(characterLeveling != null)
                    {
                        characterLeveling.playerCharacter.AddItem(transform.GetComponent<Item>(), true);
                        characterLeveling.xp += xpaddvalue;
                        i++;
                    }

                    var audioClips = Traverse.Create(__instance).Field("sndCollect").GetValue<AudioClip[]>();

                    int clipCount = audioClips.Length;
                    if(clipCount > 0)
                    {
                        RM.code.PlayOneShot(audioClips[UnityEngine.Random.Range(0, clipCount)], UnityEngine.Random.Range(0.9f, 1.1f));
                    }
                }
                Plugin.Logger.LogDebug($"{i} bonus items salvaged");
            }
            
            return true;
        }

    }


    [HarmonyPatch(typeof(PlayerCharacter))]
    internal class PlayerPatches
    {
        [HarmonyPatch(nameof(PlayerCharacter.Awake))]
        [HarmonyPostfix]
        public static void Awake(PlayerCharacter __instance)
        {
            __instance.gameObject.AddComponent<CharacterLeveling>();

            
        }
        [HarmonyPatch(nameof(PlayerCharacter.CalculatePlayerStats))]
        [HarmonyPostfix]
        public static void CalculatePlayerStats(PlayerCharacter __instance)
        {
            var characterLvling = CharacterLeveling.Instance;
            if (characterLvling == null) { return; }

            if (FPSRigidBodyWalker.code.isUnderWater)
            {
                FPSRigidBodyWalker.code.swimSpeed = characterLvling.swimSpeed;
            }

            FPSRigidBodyWalker.code.walkSpeed += characterLvling.walkSpeed;
            FPSRigidBodyWalker.code.sprintSpeed += characterLvling.runSpeed;
        }
    }
    [HarmonyPatch(typeof(SaveManager))]
    internal class SaveManagerPatches
    {

        static void SaveCharacter()
        {
            var instance = CharacterLeveling.Instance;
            if (instance != null)
            {
                instance.Save();
            }

        }
        [HarmonyPatch(nameof(SaveManager.SaveGameAsync))]
        [HarmonyPrefix]
        public static bool SaveWorldPostfix(SaveManager __instance, ref UniTask __result)
        {
            // save character leveling
            SaveCharacter();

            __result = UniTask.CompletedTask;
            return true;
        }

        [HarmonyPatch(nameof(SaveManager.SaveGame))]
        [HarmonyPrefix]
        public static bool SaveGame(SaveManager __instance)
        {
            // save character leveling
            SaveCharacter();
            return true;
        }
    }

}
