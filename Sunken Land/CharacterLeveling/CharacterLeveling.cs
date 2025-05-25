using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using HarmonyLib;
using System.Xml;
using System.Data;
using TMPro;
using Cysharp.Threading.Tasks.Triggers;
using Condition;
using System.Diagnostics.Eventing.Reader;
using UnityEngine.UI;
using UltimateSurvival;
using Fusion;
using UnityEngine.Localization.Components;
using ES3Types;
using static UnityEngine.Random;
using DG.Tweening;

namespace CharacterLeveling
{
    public class CharacterLeveling : MonoBehaviour
    {

        public PlayerCharacter playerCharacter;

        public int level { get { return _level; }set { _level = value; UpdateLvlTxt(); if (_lvlbar != null) { _lvlbar.fillAmount = 0; } } }
        private int _level = 1;

        public int spendingPoints { get { return _spendingPoints; } set { _spendingPoints = value; } }
        private int _spendingPoints;

        public static CharacterLeveling Instance;
        public float xp { get { return _xp; }set {
                _xp = value;

                UpdateLvlbarValue();
                // while there is xp && required xp to level up is assigned
                if (_xp >= _requiredXPToLevelUp && _requiredXPToLevelUp != -1)
                {
                    // while required xp to level up is assigned && there is xp
                    while (_requiredXPToLevelUp != -1 && xp > 0)
                    {
                        // if xp reaches required xp to level up
                        if (_xp >= _requiredXPToLevelUp)
                        {
                            // if level hasn't reached max level
                            if (level < LevelingDefs.config_maxLevel)
                            {
                                // reduce xp and level up
                                _xp -= _requiredXPToLevelUp;
                                level += 1;
                                spendingPoints += LevelingDefs.config_spendingPointsPerLevel;

                                // set next required xp to level up
                                SetNextRequiredXPToLevelUp();
                            }
                            else { break; }
                        }
                        else
                        {
                            break;
                        }
                    }

                }
            } }

        private float _xp;
        private float _requiredXPToLevelUp = -1;

        
        

        private SaveManager saveManager;
        private string file_leveling;


        public float original_health;
        public float original_stamina;
        public float original_oxygen = 120f;
        public float original_swimSpeed = 2.645f;
        //public float original_walkSpeed = 3.3f;
        public float original_runSpeed;
        public int points_health;
        public int points_stamina;
        public int points_oxygen;
        public int points_swimming;
        public int points_running;
        public int points_lootSpeed;
        public int points_salvageYield;

        public float swimSpeed;
        public float walkSpeed;
        public float runSpeed;
        public float lootSpeed;

        private RectTransform panel_levelingBook;
        /*
        private RectTransform ui_lvlingBook;
        private TextMeshProUGUI lvlingBookUI_txt_title;
        private TextMeshProUGUI lvlingBookUI_txt_xp_sp;

        private TextMeshProUGUI lvlingBookUI_txt_cs_health;
        private TextMeshProUGUI lvlingBookUI_txt_cs_stamina;
        private TextMeshProUGUI lvlingBookUI_txt_cs_oxygen;
        private TextMeshProUGUI lvlingBookUI_txt_cs_swimming;
        private TextMeshProUGUI lvlingBookUI_txt_cs_walkrun;
        private TextMeshProUGUI lvlingBookUI_txt_cs_salvageSpeed;
        private TextMeshProUGUI lvlingBookUI_txt_cs_salvageYield;*/
        private AudioSource _audioSource_levelingBookOpen;

        void Awake()
        {
            playerCharacter = GetComponent<PlayerCharacter>();
            //swimSpeed = original_swimSpeed;
            //walkSpeed = original_walkSpeed;
            runSpeed = original_runSpeed;

            // create empty gameObject for audio
            GameObject empty_audioSource = new GameObject();
            empty_audioSource.transform.SetParent(transform);
            _audioSource_levelingBookOpen = empty_audioSource.AddComponent<AudioSource>();
            _audioSource_levelingBookOpen.clip = LevelingDefs.audioClip_levelingBookOpen;

            Instance = this;
        }


        void Start()
        {

            Load();
            SetNextRequiredXPToLevelUp();
            UpdateAfterLevel();
            CreateLevelingBookUI();
            CreateLevelHUD();
            UpdateLvlTxt();
        }

        private bool _levelingBookPanelVisible;


        void LevelingBookPanelInput()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                if (_levelingBookPanelVisible) { HideLevelingBookPanel(); } else { ShowLevelingBookPanel(); }
            }
        }
        void ShowLevelingBookPanel()
        {
            Global.code.uiMain.Open(UIPanel.Quest);

            _levelingBookPanelVisible = true;
            panel_levelingBook.gameObject.SetActive(true);

            // play leveling book open audio clip
            _audioSource_levelingBookOpen.Play();
        }

        void HideLevelingBookPanel()
        {
            _levelingBookPanelVisible = false;
            panel_levelingBook.gameObject.SetActive(false);
            Global.code.uiMain.Close();
        }
        void CreateLevelHUD()
        {

            // find canvas
            var global = Global.code;
            Canvas canvas = Traverse.Create(global).Field("canvas").GetValue<Canvas>();

            // find main panels
            Transform t_mainPanels = canvas.transform.Find("Main Panels");
            Transform t_stats = t_mainPanels.transform.Find("UICombat/Root/Combat GUI Group/StatePenal");

            // create empty gameObject 'ui_lvlinfo'
            GameObject ui_lvlinfo = new GameObject("ui_lvlinfo", typeof(RectTransform));
            RectTransform t_lvlinfo = ui_lvlinfo.GetComponent<RectTransform>();
            ui_lvlinfo.transform.SetParent(t_stats);

            // clone progress bar background from energy background
            GameObject cloned_bg = GameObject.Instantiate(t_stats.Find("Player energy Panel (1)/bg").gameObject);
            RectTransform t_clonedbg = cloned_bg.GetComponent<RectTransform>();
            t_clonedbg.SetParent(t_lvlinfo);
            t_clonedbg.localPosition = new Vector3(424.0901f, 21.9021f, 0);
            t_clonedbg.sizeDelta = new Vector2(146, 16.3342f);

            // clone progress bar from hunger bar
            RectTransform t_cloned_pb = GameObject.Instantiate(t_mainPanels.Find("UICombat/Root/Combat GUI Group/StatePenal/Hunger Panel/hunger bar").gameObject).GetComponent<RectTransform>();
            t_cloned_pb.gameObject.name = "lvlbar";
            t_cloned_pb.SetParent(t_lvlinfo);
            t_cloned_pb.localPosition = new Vector3(424.0901f, 21.9021f, 0);
            t_cloned_pb.sizeDelta = new Vector2(146, 16.3342f);
            _lvlbar = t_cloned_pb.GetComponent<Image>();
            _lvlbar.color = Color.white;
            _lvlbar.fillAmount = 0;

            UpdateLvlbarValue();

            RectTransform t_txt_lvlinfo = GameObject.Instantiate(t_mainPanels.transform.Find("UIQuest/ContentRoot/DairyEntry_SurvivalGuide(Clone)/Page_1/Localized Text (TMP)_1").gameObject).GetComponent<RectTransform>();
            t_txt_lvlinfo.gameObject.name = "txt_lvlinfo";
            _txt_lvlinfo = t_txt_lvlinfo.GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(_txt_lvlinfo);
            t_txt_lvlinfo.anchorMin = new Vector2(0.5f, 0.5f);
            t_txt_lvlinfo.anchorMax = new Vector2(0.5f, 0.5f);
            t_txt_lvlinfo.pivot = new Vector2(0.5f, 0.5f);
            _txt_lvlinfo.alignment = TextAlignmentOptions.Center;
            _txt_lvlinfo.margin = new Vector4(140, 0, 0, 0);
            t_txt_lvlinfo.SetParent(t_lvlinfo);
            t_txt_lvlinfo.localPosition = new Vector3(321.6909f, 21.9021f, 0);
            _txt_lvlinfo.fontSize = 7;
            _txt_lvlinfo.fontSizeMax = 14;
            _txt_lvlinfo.color = Color.white;



            _txt_lvlinfo.text = "Lvl 0:";
            
        }

        private Image _lvlbar;
        private TextMeshProUGUI _txt_lvlinfo;
        private float _lvlbar_value;

        void UpdateLvlbarValue() { _lvlbar_value = LevelingDefs.ConvertRange(0, _requiredXPToLevelUp, 0, 1, _xp); }
        void UpdateLvlbarUI() { if (_lvlbar == null) { return; } _lvlbar.fillAmount = Mathf.Lerp(_lvlbar.fillAmount, _lvlbar_value, 5 * Time.deltaTime); }
        void UpdateLvlTxt() { if (_txt_lvlinfo == null) { return; } _txt_lvlinfo.text = $"Lvl {level}:"; }
        void CreateLevelingBookUI()
        {
            var global = Global.code;

            Canvas canvas = Traverse.Create(global).Field("canvas").GetValue<Canvas>();


            // find main panels
            Transform t_mainPanels = canvas.transform.Find("Main Panels");


            // create leveling book panel
            GameObject levelingBookPanel = new GameObject("LevelingBookPanelUI", typeof(RectTransform));
            levelingBookPanel.transform.SetParent(t_mainPanels.transform);
            RectTransform t_panel = levelingBookPanel.GetComponent<RectTransform>();
            panel_levelingBook = t_panel;


            
            RectTransform t_questUI = t_mainPanels.Find("UIQuest").GetComponent<RectTransform>();
            t_panel.localPosition = t_questUI.localPosition;

            // clone & create black background
            //GameObject bg_black = GameObject.Instantiate(t_questUI.transform.Find("BG Black_1").gameObject);
            //bg_black.GetComponent<RectTransform>().SetParent(t_panel);
            


            // clone & create book background
            GameObject bg_book = GameObject.Instantiate(t_questUI.transform.Find("ContentRoot/DairyEntry_SurvivalGuide(Clone)/Page_1/bg").gameObject);
            RectTransform t_bg_book = bg_book.transform.GetComponent<RectTransform>();
            t_bg_book.SetParent(t_panel);
            t_bg_book.localPosition = Vector3.zero;

            // clone & create left side text
            GameObject text_left = GameObject.Instantiate(t_questUI.transform.Find("ContentRoot/DairyEntry_SurvivalGuide(Clone)/Page_1/Localized Text (TMP)_1").gameObject);
            var t_text_left = text_left.transform.GetComponent<RectTransform>();
            t_text_left.SetParent(t_panel);
            TextMeshProUGUI txt = text_left.GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(txt);
            t_text_left.localPosition = new Vector3(-255.9252f, 343.0836f, 0);


            // clone & create right side text
            GameObject text_right = GameObject.Instantiate(t_questUI.transform.Find("ContentRoot/DairyEntry_SurvivalGuide(Clone)/Page_1/Localized Text (TMP)_1").gameObject);
            var t_text_right = text_right.transform.GetComponent<RectTransform>();
            t_text_right.SetParent(t_panel);
            txt_right = t_text_right.GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(txt_right);
            t_text_right.localPosition = new Vector3(250, 357.0906f, 0);


            //  - right txt title
            //250 343.0836 0 - right txt


            // clone & create left side text title
            GameObject text_left_title = GameObject.Instantiate(t_questUI.transform.Find("ContentRoot/DairyEntry_SurvivalGuide(Clone)/Page_1/Localized Text (TMP) Title").gameObject);
            var t_text_left_title = text_left_title.transform.GetComponent<RectTransform>();
            t_text_left_title.SetParent(t_panel);
            TextMeshProUGUI txt_title = text_left_title.GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(txt_title);
            t_text_left_title.localPosition = new Vector3(-264.2709f, 357.0906f, 0);

            // clone & create right side text title
            GameObject text_right_title = GameObject.Instantiate(t_questUI.transform.Find("ContentRoot/DairyEntry_SurvivalGuide(Clone)/Page_1/Localized Text (TMP) Title").gameObject);
            var t_text_right_title = text_right_title.transform.GetComponent<RectTransform>();
            t_text_right_title.SetParent(t_panel);
            TextMeshProUGUI txt_title_right = text_right_title.GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(txt_title_right);



            t_text_right_title.localPosition = new Vector3(250, 357.0906f, 0);
            txt_right_title = txt_title_right;

            txt_title.text = "Notes:";
            txt.text = "earn xp by salvaging, killing enemies and looting, level up and use spending points to increase your stats, a mod made within 3 days for my gameplay that kinda lacked the excitement and rewards for killing enemies";

            // txt character stats 
            TextMeshProUGUI cloned_txt_right_charStats = GameObject.Instantiate(text_right_title).GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(cloned_txt_right_charStats);
            cloned_txt_right_charStats.text = $"Character Stats:";
            cloned_txt_right_charStats.GetComponent<RectTransform>().localPosition = new Vector3(250, 350.0015f, 0);

            // btns list
            GameObject BtnsList = new GameObject("BtnList", typeof(RectTransform));
            var t_btnslist = BtnsList.GetComponent<RectTransform>();
            t_btnslist.SetParent(t_panel);
            t_btnslist.localPosition = new Vector3(354.4731f, 113.5994f, 0);

            VerticalLayoutGroup vlg_btns = BtnsList.AddComponent<VerticalLayoutGroup>();
            vlg_btns.spacing = 20;
            vlg_btns.childForceExpandHeight = false;
            vlg_btns.childForceExpandWidth = false;
            vlg_btns.childControlHeight = false;
            vlg_btns.childControlWidth = false;

            // stats list
            GameObject StatList = new GameObject("StatsList", typeof(RectTransform));
            var t_Statlist = StatList.GetComponent<RectTransform>();
            t_Statlist.SetParent(t_panel);
            t_Statlist.localPosition = new Vector3(116.7327f, 113.5994f, 0);

            VerticalLayoutGroup vlg_Stat = StatList.AddComponent<VerticalLayoutGroup>();
            vlg_Stat.spacing = 20;
            vlg_Stat.childForceExpandHeight = false;
            vlg_Stat.childForceExpandWidth = false;
            vlg_Stat.childControlHeight = false;
            vlg_Stat.childControlWidth = false;


            float defaultIconSize = 28f;

            LevelingBookPanelCreateStat("health", "Health", LevelingDefs.icon_health, defaultIconSize, defaultIconSize, vlg_btns, text_right, vlg_Stat);
            LevelingBookPanelCreateStat("stamina", "Stamina", LevelingDefs.icon_stamina, defaultIconSize, defaultIconSize, vlg_btns, text_right, vlg_Stat);
            LevelingBookPanelCreateStat("oxygen", "Oxygen", LevelingDefs.icon_oxygen, 23, 28, vlg_btns, text_right, vlg_Stat);
            LevelingBookPanelCreateStat("swimming", "Swimming", LevelingDefs.icon_swimming, defaultIconSize, 24, vlg_btns, text_right, vlg_Stat);
            LevelingBookPanelCreateStat("walkrun", "Walk/Run", LevelingDefs.icon_stamina, defaultIconSize, defaultIconSize, vlg_btns, text_right, vlg_Stat);
            LevelingBookPanelCreateStat("salvagespeed", "Salvage Speed", LevelingDefs.icon_salvage, defaultIconSize, defaultIconSize, vlg_btns, text_right, vlg_Stat);
            LevelingBookPanelCreateStat("salvageyield", "Salvage Yield", LevelingDefs.icon_salvage, defaultIconSize, defaultIconSize, vlg_btns, text_right, vlg_Stat);
            HideLevelingBookPanel();
        }


        void ClearTxtLocalization(TextMeshProUGUI txt)
        {
            GameObject.Destroy(txt.GetComponent<LocalizeStringEvent>());
        }

        List<TextMeshProUGUI> levelingBookUI_txtStats = new List<TextMeshProUGUI>();
        void LevelingBookPanelCreateStat(string statName, string displayName, Sprite icon, float icon_width, float icon_height, VerticalLayoutGroup vlg_btns, GameObject cloning_txt, VerticalLayoutGroup vlg_stat)
        {
            // create 'btn' gameObject
            //----
            GameObject btn_plus = new GameObject("btn", typeof(RectTransform));
            RectTransform t_btn_plus = btn_plus.GetComponent<RectTransform>();
            
            Image image = btn_plus.AddComponent<Image>();
            image.sprite = LevelingDefs.icon_plus;
            float btnPlusSize = 28f;
            t_btn_plus.sizeDelta = new Vector2(btnPlusSize, btnPlusSize);

            t_btn_plus.SetParent(vlg_btns.transform);
            UILevelingBookBtnSpendPoint spendPoint_script = btn_plus.AddComponent<UILevelingBookBtnSpendPoint>();
            spendPoint_script.characterLeveling = this;
            spendPoint_script.stat = statName;
            spendPoint_script.displayName = displayName;

            //----
            
            // create 'stat' gameObject
            //----
            GameObject icon_plus = new GameObject("icon", typeof(RectTransform));
            RectTransform t_icon_plus = icon_plus.GetComponent<RectTransform>();

            Image image_icon = icon_plus.AddComponent<Image>();
            image_icon.sprite = icon;
            image_icon.color = ColorUtils.HexToColor("353535");
            float iconPlusSize = icon_width;
            t_icon_plus.sizeDelta = new Vector2(iconPlusSize, iconPlusSize);

            t_icon_plus.SetParent(vlg_stat.transform);
            //
            //----

            TextMeshProUGUI cloned_txt = GameObject.Instantiate(cloning_txt).GetComponent<TextMeshProUGUI>();
            levelingBookUI_txtStats.Add(cloned_txt);
            ClearTxtLocalization(cloned_txt);
            

            RectTransform t_cloned_txt = cloned_txt.GetComponent<RectTransform>();
            t_cloned_txt.anchorMin = new Vector2(0.5f, 0.5f);
            t_cloned_txt.anchorMax = new Vector2(0.5f, 0.5f);
            t_cloned_txt.pivot = new Vector2(0.5f, 0.5f);
            cloned_txt.alignment = TextAlignmentOptions.Center;
            t_cloned_txt.SetParent(t_icon_plus);
           
            t_cloned_txt.localPosition = new Vector3(icon_width + 10, 0, 0);
            cloned_txt.text = displayName;
            cloned_txt.margin = new Vector4(140, 0, 0, 0);
            spendPoint_script.txt = cloned_txt;
            /*
            // create stat object
            GameObject stat = new GameObject("stat", typeof(RectTransform));
            RectTransform t_stat = stat.GetComponent<RectTransform>();

            // create stat icon
            GameObject stat_icon = new GameObject("icon", typeof(RectTransform));

            RectTransform t_stat_icon = stat_icon.GetComponent<RectTransform>();
            Image image = stat_icon.AddComponent<Image>();
            image.color = ColorUtils.HexToColor("353535");
            image.sprite = icon;
            t_stat_icon.sizeDelta = new Vector2(icon_width, icon_height);
            t_stat_icon.localPosition = Vector3.zero;


            // create button plus
            GameObject btn_plus = new GameObject("btn", typeof(RectTransform));

            RectTransform t_stat_btn_plus = btn_plus.GetComponent<RectTransform>();
            Image image2 = btn_plus.AddComponent<Image>();
            image2.color = Color.white;
            image2.sprite = LevelingDefs.icon_plus;
            t_stat_btn_plus.sizeDelta = new Vector2(icon_width * 1.2f, icon_height * 1.2f);
            t_stat_btn_plus.localPosition = Vector3.zero;

            t_stat_btn_plus.SetParent(t_stat);
            t_stat_btn_plus.localPosition = new Vector3(432.9075f, 0, 0);


            TextMeshProUGUI cloned_txt = GameObject.Instantiate(cloning_txt).GetComponent<TextMeshProUGUI>();
            ClearTxtLocalization(cloned_txt);
            cloned_txt.text = $"{displayName} (0p):";
            cloned_txt.fontSize = 25;
            cloned_txt.fontSizeMax = 25;
            cloned_txt.fontSizeMin = 10;

            cloned_txt.transform.SetParent(t_stat_icon);

            t_stat_icon.SetParent(t_stat_btn_plus);
            t_stat.SetParent(vlg.transform);

            cloned_txt.GetComponent<RectTransform>().localPosition = new Vector3(icon_width + 2, 0, 0);*/
        }



        private TextMeshProUGUI txt_right;
        private TextMeshProUGUI txt_right_title;
        void UpdateLevelingBookUI()
        {
            // exit if leveling book panel is not visible
            if (!_levelingBookPanelVisible) { return; }

            txt_right_title.text = $"{playerCharacter.CharacterName} (Level {level})";
            txt_right.text = $"XP: {xp}/{_requiredXPToLevelUp}, Spending Points: {spendingPoints}";
        }


        void Update()
        {
            CollectableLootSpeedModifier();
            UpdateLevelingBookUI();
            LevelingBookPanelInput();
            UpdateLvlbarUI();
        }

        public void Save()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(file_leveling);

            XmlNode node_lvling = xdoc.SelectSingleNode("lvling");

            node_lvling.SelectSingleNode("level").InnerText = level.ToString();
            node_lvling.SelectSingleNode("xp").InnerText = xp.ToString();
            node_lvling.SelectSingleNode("spendingPoints").InnerText = spendingPoints.ToString();

            node_lvling.SelectSingleNode("points_health").InnerText = points_health.ToString();
            node_lvling.SelectSingleNode("points_stamina").InnerText = points_stamina.ToString();
            node_lvling.SelectSingleNode("points_oxygen").InnerText = points_oxygen.ToString();
            node_lvling.SelectSingleNode("points_swimming").InnerText = points_swimming.ToString();
            node_lvling.SelectSingleNode("points_running").InnerText = points_running.ToString();
            node_lvling.SelectSingleNode("points_lootSpeed").InnerText = points_lootSpeed.ToString();
            node_lvling.SelectSingleNode("points_salvageYield").InnerText = points_salvageYield.ToString();
            xdoc.Save(file_leveling);

            Plugin.Logger.LogDebug($"character levels saved");
        }

   

        

        void CollectableLootSpeedModifier()
        {
            var focusedInteraction = playerCharacter.FocusedInteraction;
            if(focusedInteraction == null) { return; }

            var focusedInteractionParent = focusedInteraction.transform.parent;

            if(focusedInteractionParent == null) { return; }

            var collectableInteraction = focusedInteractionParent.GetComponent<CollectableContinuingInteraction>();
            if(collectableInteraction == null) { return; }

            ModifiedCollectable modifiedCollectable = focusedInteractionParent.GetComponent<ModifiedCollectable>();   
            if(modifiedCollectable == null)
            {
                modifiedCollectable = focusedInteractionParent.gameObject.AddComponent<ModifiedCollectable>();
                modifiedCollectable.collectable = collectableInteraction;

                var interaction = focusedInteractionParent.GetComponentInChildren<Interaction>();
                modifiedCollectable.originalDuration = interaction.InteractionTime;
                modifiedCollectable.interaction = interaction;
                modifiedCollectable.ModifySalvageYield(points_salvageYield, level);
            }
            modifiedCollectable.setDuration = modifiedCollectable.originalDuration - lootSpeed;
            

            /*
            // get focused interaction, exit if not found
            var interaction = playerCharacter.FocusedInteraction;
            if(interaction == null) { return; }

            // get collectable, exit if not found
            var collectable = interaction.transform.parent.GetComponent<CollectableContinuingInteraction>();
            if(collectable == null) { return; };


            // get modified collectable, add component if not found
            ModifiedCollectable modifiedCollectable = interaction.GetComponent<ModifiedCollectable>();
            if(modifiedCollectable == null)
            {
                modifiedCollectable = interaction.gameObject.AddComponent<ModifiedCollectable>();
                modifiedCollectable.originalDuration = interactioGetComponentInChildren<Interaction>();
                modifiedCollectable.collectable = collectable;
                
            }

            modifiedCollectable.setDuration = modifiedCollectable.originalDuration - lootSpeed;*/
        }
        public void UpdateAfterLevel()
        {
            PlayerDefaultStatus defaultStatus = Traverse.Create(playerCharacter).Field("playerDefaultStatus").GetValue<PlayerDefaultStatus>();
            var tr_defaultStatus = Traverse.Create(defaultStatus);

            // update health
            float newHealthIncreaseAmount = (LevelingDefs.config_health_increasePerPoint * points_health);
            float newHealth = original_health + newHealthIncreaseAmount;

            // modify health
            tr_defaultStatus.Field("maxHealth").SetValue(newHealth);
            tr_defaultStatus.Field("health").SetValue(newHealth);
            tr_defaultStatus.Field("targetHealth").SetValue(newHealth);
            playerCharacter.MaxHealth = newHealth;
            playerCharacter.TargetHealth = newHealth;

            // clamp health
            playerCharacter.Health += newHealthIncreaseAmount;
            if(playerCharacter.Health > newHealth) { playerCharacter.Health = newHealth; }


            // update stamina
            float newStaminaIncreaseAmount = (LevelingDefs.config_stamina_increasePerPoint * points_stamina);
            float newStamina = original_stamina + newStaminaIncreaseAmount;
            tr_defaultStatus.Field("maxEnergy").SetValue(newStamina);
            tr_defaultStatus.Field("energy").SetValue(newStamina);
            
            playerCharacter.MaxEnergy = newStamina;
            playerCharacter.Energy += newStaminaIncreaseAmount;

            if (playerCharacter.Energy > newStamina) { playerCharacter.Energy = newStamina; }

            
            // update oxygen // #continue #oxygen bug
            float newOxygenIncreaseAmount = (LevelingDefs.config_oxygen_increasePerPoint * points_oxygen);
            float newOxygen = original_oxygen + newOxygenIncreaseAmount;
            tr_defaultStatus.Field("maxAir").SetValue(newOxygen);
            tr_defaultStatus.Field("air").SetValue(newOxygen);

            playerCharacter.MaxAir = newOxygen;
            playerCharacter.Air += newOxygenIncreaseAmount;
            if(playerCharacter.Air > newOxygen) { playerCharacter.Air = newOxygen; }
            

            // update swimming
            float newSwimmingIncreaseAmount = (LevelingDefs.config_swimming_increasePerPoint * points_swimming);
            float newSwimming = original_swimSpeed + newSwimmingIncreaseAmount;
            swimSpeed = newSwimming;

            // update walk/run
            float newWalkRunIncreaseAmount = (LevelingDefs.config_walkrun_increasePerPoint * points_running);
            walkSpeed = newWalkRunIncreaseAmount;
            runSpeed = newWalkRunIncreaseAmount;

            // update loot speed
            lootSpeed = (LevelingDefs.config_lootSpeed_increasePerPoint * points_lootSpeed);
        }
        void SetNextRequiredXPToLevelUp()
        {
            float multiplier = 1 + (LevelingDefs.config_requiredXPToLevelUpMultiplierPerLevel * level);
            _requiredXPToLevelUp = LevelingDefs.config_requiredXPToLevelUp * multiplier;
        }

       
        void Load()
        {
            // store original values
            original_health = playerCharacter.MaxHealth;
            original_stamina = playerCharacter.MaxEnergy;
            //original_oxygen = playerCharacter.MaxAir;

            // get save manager
            saveManager = GameObject.FindObjectOfType<SaveManager>();
            if (saveManager == null) { Plugin.Logger.LogError($"LevelingPlayer.Load(): SaveManager component not found"); return; }

            // get save location
            string saveLoc = Path.GetDirectoryName(ES3Settings.defaultSettings.FullPath);

            // get save location 'characters'
            string saveLoc_characters = saveLoc + @"\Characters";


            // get character GUID & name
            string characterGUID = saveManager.CurrentCharacterGuid.ToString();
            string characterName = saveManager.CurrentCharacterName;

            // get character folder
            string saveLoc_character = saveLoc_characters + $@"\{characterName}~{characterGUID}";

            // get character world folder
            string saveLoc_characterWorld = saveLoc_character + @"\" + saveManager.CurrentWorldGuid.ToString();



            if (!Directory.Exists(saveLoc_characterWorld)) { Directory.CreateDirectory(saveLoc_characterWorld); }

            file_leveling = saveLoc_characterWorld + @"\leveling.xml";

            if (!File.Exists(file_leveling))
            {
                XmlWriter writer = LevelingDefs.NewXmlWriter(file_leveling);
                writer.WriteStartElement("lvling");
                writer.WriteElementString("originalHealth", original_health.ToString());
                writer.WriteElementString("originalStamina", original_stamina.ToString());
                writer.WriteElementString("originalOxygen", original_oxygen.ToString());

                writer.WriteElementString("level", level.ToString());
                writer.WriteElementString("xp", xp.ToString());
                writer.WriteElementString("spendingPoints", spendingPoints.ToString());

                writer.WriteElementString("points_health", points_health.ToString());
                writer.WriteElementString("points_stamina", points_stamina.ToString());
                writer.WriteElementString("points_oxygen", points_oxygen.ToString());
                writer.WriteElementString("points_swimming", points_swimming.ToString());
                writer.WriteElementString("points_running", points_running.ToString());
                writer.WriteElementString("points_lootSpeed", points_lootSpeed.ToString());
                writer.WriteElementString("points_salvageYield", points_salvageYield.ToString());

                writer.WriteEndElement();
                writer.Close();
            }
            else
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(file_leveling);

                XmlNode node_lvling = xdoc.SelectSingleNode("lvling");

                original_health = float.Parse(node_lvling.SelectSingleNode("originalHealth").InnerText);
                original_stamina = float.Parse(node_lvling.SelectSingleNode("originalStamina").InnerText);
                original_oxygen = float.Parse(node_lvling.SelectSingleNode("originalOxygen").InnerText);

                level = int.Parse(node_lvling.SelectSingleNode("level").InnerText);
                xp = float.Parse(node_lvling.SelectSingleNode("xp").InnerText);
                spendingPoints = int.Parse(node_lvling.SelectSingleNode("spendingPoints").InnerText);

                points_health = int.Parse(node_lvling.SelectSingleNode("points_health").InnerText);
                points_stamina = int.Parse(node_lvling.SelectSingleNode("points_stamina").InnerText);
                points_oxygen = int.Parse(node_lvling.SelectSingleNode("points_oxygen").InnerText);
                points_swimming = int.Parse(node_lvling.SelectSingleNode("points_swimming").InnerText);
                points_running = int.Parse(node_lvling.SelectSingleNode("points_running").InnerText);
                points_lootSpeed = int.Parse(node_lvling.SelectSingleNode("points_lootSpeed").InnerText);
                points_salvageYield = int.Parse(node_lvling.SelectSingleNode("points_salvageYield").InnerText);
            }
        }

    }
}
