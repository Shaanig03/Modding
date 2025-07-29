using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
namespace CharacterLeveling
{
    public class ModifiedCollectable : MonoBehaviour
    {
        public CollectableContinuingInteraction collectable;
        public Interaction interaction;

        public float originalDuration;

        public List<Item> salvageYieldItems = new List<Item>();
        public float setDuration
        {
            set
            {
                // get new duration (reduced one from loot speed subtraction)
                float _newDuration = value;

                // if new duration is less than 0.1, then set it to 0.1
                if(_newDuration < 0.1f) { _newDuration = 0.1f; }

                //interaction.InteractionTime = _newDuration;
                //collectable.oriInteractionTime = _newDuration;
                //collectable.SetInteractionTime(true);


                // convert range 0-3 to 0-1
                //float nextItemDropTimeMultiplier = LevelingDefs.ConvertRange(0.1f, originalDuration, 0, 1, _newDuration);
                //



                collectable.oriInteractionTime = _newDuration;
                interaction.InteractionTime = _newDuration;
                Traverse.Create(interaction).Field("oriInteractionTime").SetValue(_newDuration);
                collectable.nextItemDropTime = (_newDuration / collectable.generatedDropItems.Count) * (collectable.nextItemIndex + 1);

                /*
                // convert range 0-3 to 0-1
                float nextItemDropTimeMultiplier = LevelingDefs.ConvertRange(0.1f, originalDuration, 0, 1, _newDuration);
                collectable.nextItemDropTime *= nextItemDropTimeMultiplier;
              
                collectable.oriInteractionTime = _newDuration;

                Traverse.Create(interaction).Field("oriInteractionTime").SetValue(_newDuration);
                interaction.InteractionTime = _newDuration;*/
            }
        }

        public void ModifySalvageYield(int salvageYieldPoints, int level)
        {
            int itemCount = Mathf.RoundToInt(UnityEngine.Random.Range(0, LevelingDefs.config.config_salvageYield_newItemCountPerPoint * salvageYieldPoints));

            for(int i = 0; i < itemCount; i++)
            {
                if(UnityEngine.Random.Range(0, 10) >= LevelingDefs.config.config_salvageYield_newItemChance) //
                {

                    var possibleItem = collectable.possibleItems[UnityEngine.Random.Range(0, collectable.possibleItems.Count)];
                    salvageYieldItems.Add(possibleItem.item.GetComponent<Item>());
                }
            }
        }
    }
}
