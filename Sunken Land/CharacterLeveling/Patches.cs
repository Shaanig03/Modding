using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using Fusion;
using Cysharp.Threading.Tasks;

namespace CharacterLeveling
{


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
                    }

                    var audioClips = Traverse.Create(__instance).Field("sndCollect").GetValue<AudioClip[]>();

                    int clipCount = audioClips.Length;
                    if(clipCount > 0)
                    {
                        RM.code.PlayOneShot(audioClips[UnityEngine.Random.Range(0, clipCount)], UnityEngine.Random.Range(0.9f, 1.1f));
                    }
                }
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
