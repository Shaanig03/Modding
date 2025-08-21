using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VanillaExpandedLoreFriendly.UI
{
    public class UITurretConfiguration : uGUI_InputGroup
    {
        public bool state { get; private set; }
        public CanvasGroup canvasGroup;


        public TargetingCore target;
        public TMP_InputField txtbox_targets;
        public TMP_InputField txtbox_targetingRange;

        public Player player;

        public override void Awake()
        {
            base.Awake();

            canvasGroup = gameObject.EnsureComponent<CanvasGroup>();
            player = Player.main;

            state = true;
            Close();

            transform.Find("BG/BtnApplyForTurret").GetComponent<Button>().onClick.AddListener(ApplyForTurret);
            transform.Find("BG/BtnApplyForTurrets").GetComponent<Button>().onClick.AddListener(ApplyForTurrets);
            transform.Find("BG/BtnCancel").GetComponent<Button>().onClick.AddListener(Cancel);

            transform.Find("BG/txt_title").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_title;
            transform.Find("BG/txt_targetingRange").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_label_targetingRange;
            transform.Find("BG/txt_targets").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_label_targets;
            transform.Find("BG/tip_visionRange").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_label_tipMaxRange;
            transform.Find("BG/tip_targeting").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_label_tipTargets;
            transform.Find("BG/BtnApplyForTurret/Text (TMP)").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_btn_applyForThisTurret;
            transform.Find("BG/BtnApplyForTurrets/Text (TMP)").GetComponent<TextMeshProUGUI>().text = Vars.lang.ui_turretConfig_btn_applyForNearbyTurrets;

            txtbox_targets = transform.Find("BG/Txtbox_targetWords").GetComponent<TMP_InputField>();
            txtbox_targetingRange = transform.Find("BG/Txtbox_visionRange").GetComponent<TMP_InputField>();
        }

        void ApplyForTurret()
        {
            // exit if target is null
            if(target == null) { return; }

            // modify targeting core
            bool successful = ApplyChangesToCore(target);

            // show message
            ErrorMessage.AddMessage(Vars.lang.txt_turretConfig_applyChanges_turretApplied);

            // if there was no errors, then close UI
            if (successful) { Close(); }

        }

        void ApplyForTurrets() 
        {
            // do a spherical collision test
            Collider[] cols = Physics.OverlapSphere(Player.mainCollider.transform.position, 100);
            List<TargetingCore> cores = new List<TargetingCore>();

            bool errorFound = false;
         
            // loop through each collider
         
            int c = 0;
            foreach (Collider _col in cols)
            {
                TargetingCore core = _col.GetComponent<TargetingCore>();
                if(core == null) { core = _col.GetComponentInParent<TargetingCore>(); }

                // if its a turret
                if(core != null && !cores.Contains(core))
                {
                    // apply changes to core
                    bool successful = ApplyChangesToCore(core);
                    if (!successful) { errorFound = true; }

                    cores.Add(core);
                    c++;
                }
            }

            if(c > 0)
            {
                ErrorMessage.AddMessage(string.Format(Vars.lang.txt_turretConfig_applyChanges_turretsApplied, c));  
            }
            if (!errorFound) { Close(); }
        }

        
        bool ApplyChangesToCore(TargetingCore targetingCore)
        {
            float targetingRadius;


            // apply targeting radius
            bool successful = true;
            if (float.TryParse(txtbox_targetingRange.text, out targetingRadius))
            {
                float maxRange = Vars.turretMaxVisionRadius;
                if (targetingRadius > maxRange)
                {
                    targetingRadius = maxRange;
                }
                targetingCore.visionRadius = targetingRadius;
            }
            else
            {
                ErrorMessage.AddMessage(Vars.lang.txt_turretConfig_applyChanges_targetingRangeIsNotNumber);
                successful = false;
            }

            if (txtbox_targets.text.Length > 0)
            {
                // apply target names
                string[] targetNames = txtbox_targets.text.Split(',');
                List<string> list = new List<string>();
                foreach (string _targetName in targetNames)
                {
                    if (_targetName != "" && !String.IsNullOrWhiteSpace(_targetName))
                    {
                        string _targetNameModified = _targetName.ToLower().Replace(" ", "");
                        list.Add(_targetNameModified);
                    }

                }
                targetingCore.targetNames = list.ToArray();
            }

            // set target to null
            targetingCore.Target = null;    
            return successful;
        }

        void Cancel()
        {
            Close();
            
        }

        private WaitForSeconds _delay_enableMovement = new WaitForSeconds(0.1f);
        private System.Collections.IEnumerator _IEnableMovement()
        {
            yield return _delay_enableMovement;
            base.Select(false);
            base.Deselect();
            this.LockMovement(false);
        }

        public void Open(TargetingCore core = null)
        {
            if (core == null) { return; }
            if (this.state) { return; }

            // set target
            target = core;
            LoadTargetCore(core);
            MainCameraControl.main.SaveLockedVRViewModelAngle();
            this.SetState(true);
        }

        public void LoadTargetCore(TargetingCore core)
        {
            // set targeting range (textbox)
            txtbox_targetingRange.text = core.visionRadius.ToString();

            // get target names
            string[] targetNames = core.targetNames;


            string str = "";
            int last_index = targetNames.Length - 1;
            if (last_index != -1)
            {
                for (int i = 0; i < targetNames.Length; i++)
                {
                    if (i != last_index)
                    {
                        if(i != 0)
                        {
                            str += $" {targetNames[i].Replace(" ", "")},";
                        }
                        else
                        {
                            str += $"{targetNames[i].Replace(" ", "")},";
                        }
                        
                    }
                    else
                    {
                        if (i != 0)
                        {
                            str += $" {targetNames[i].Replace(" ", "")}";
                        }
                        else
                        {
                            str += $"{targetNames[i].Replace(" ", "")}";
                        }
                    }
                }
                txtbox_targets.text = str;
            }
            else { txtbox_targets.text = ""; }
        }

        public override void Update()
        {
            base.Update();
            if (this.state)
            {
                MainCameraControl.main.SaveLockedVRViewModelAngle();
                this.canvasGroup.alpha = 1f;
                this.canvasGroup.interactable = true;
                this.canvasGroup.blocksRaycasts = true;
                base.Select(true);

                // if player exists & is dead while UI is opened
                if(player != null && !player.IsAlive())
                {
                    // then close UI
                    Close();
                }
            }
        }

        public void Close()
        {
            if (!this.state) { return; }
            this.SetState(false);
            StartCoroutine(_IEnableMovement());
        }

        private void SetState(bool newState)
        {
            this.state = newState;
            if (this.state)
            {
                this.canvasGroup.alpha = 1f;
                this.canvasGroup.interactable = true;
                this.canvasGroup.blocksRaycasts = true;

                base.Select(true);
            }
            else
            {
                this.canvasGroup.alpha = 0f;
                this.canvasGroup.interactable = false;
                this.canvasGroup.blocksRaycasts = false;
                if (base.focused)
                {
                    base.Deselect();
                }
            }
        }
    }
}
