using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UltimateSurvival;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CharacterLeveling
{
    internal class UILevelingBookBtnSpendPoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public CharacterLeveling characterLeveling;

        public string stat;

        public Color color_default = Color.white;
        public Color color_hovered;
        public Color color_clicked;

        public Image image;
        public string displayName;
        public TextMeshProUGUI txt;

        void Awake()
        {
            color_hovered = ColorUtils.HexToColor("FFEE9B");
            color_clicked = ColorUtils.HexToColor("FFBB4A");
            image = GetComponent<Image>();
        }

        void OnEnable()
        {
            image.color = color_default;
        }

        void Update()
        {
            if(txt == null) { return; }



            Color color = image.color; 
            if(characterLeveling.spendingPoints > 0)
            {
                color.a = 1;
            } else { color.a = 0; }
            image.color = color;


            int current_points = -1;
            switch (stat)
            {
                case "health":
                    {
                        current_points = characterLeveling.stats.points_health;
                        break;
                    }
                case "stamina":
                    {
                        current_points = characterLeveling.stats.points_stamina;
                        break;
                    }
                case "oxygen":
                    {
                        current_points = characterLeveling.stats.points_oxygen;
                        break;
                    }
                case "swimming":
                    {
                        current_points = characterLeveling.stats.points_swimming;
                        break;
                    }
                case "walkrun":
                    {
                        current_points = characterLeveling.stats.points_running;
                        break;
                    }
                case "salvagespeed":
                    {
                        current_points = characterLeveling.stats.points_lootSpeed;
                        break;
                    }
                case "salvageyield":
                    {
                        current_points = characterLeveling.stats.points_salvageYield;
                        break;
                    }
                default: { break; }
            }
            txt.text = $"{displayName}({current_points}p)";
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // exit if its not left click
            if(eventData.button != PointerEventData.InputButton.Left) { return; }

            image.color = color_clicked;

            if(characterLeveling == null) { return; }
            if(characterLeveling.spendingPoints == 0) { return; }

            switch(stat)
            {
                case "health":
                    {
                        characterLeveling.stats.points_health += 1;
                        break;
                    }
                case "stamina":
                    {
                        characterLeveling.stats.points_stamina += 1;
                        break;
                    }
                case "oxygen":
                    {
                        characterLeveling.stats.points_oxygen += 1;
                        break;
                    }
                case "swimming":
                    {
                        characterLeveling.stats.points_swimming += 1;
                        break;
                    }
                case "walkrun":
                    {
                        characterLeveling.stats.points_running += 1;
                        break;
                    }
                case "salvagespeed":
                    {
                        characterLeveling.stats.points_lootSpeed += 1;
                        break;
                    }
                case "salvageyield":
                    {
                        characterLeveling.stats.points_salvageYield += 1;
                        break;
                    }
                default: { break; }
            }

            characterLeveling.spendingPoints -= 1;
            characterLeveling.UpdateAfterLevel();

            GameObject empty = new GameObject();
            empty.transform.position = characterLeveling.transform.position;

            AudioSource audioSource = empty.AddComponent<AudioSource>();
            audioSource.clip = LevelingDefs.audioClip_spendPoint;
            audioSource.Play();
            GameObject.Destroy(empty, 1.5f);
        }
        private bool _hovering;
        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            image.color = color_hovered;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            image.color = color_default;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_hovering) { image.color = color_default; } else { image.color = color_hovered; }
            
        }
    }
}
