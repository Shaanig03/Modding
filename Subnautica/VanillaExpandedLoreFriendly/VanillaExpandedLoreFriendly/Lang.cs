using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaExpandedLoreFriendly
{
    public class Lang
    {
        public string mod_has_loaded = "mod has loaded";


        public string buildable_crate_displayName = "Crate";
        public string buildable_crate_desc = "Low-cost medium storage solution";

        public string buildable_locker_displayName = "Locker";
        public string buildable_locker_desc = "Low-cost small storage solution";

        public string buildable_cabinet_displayName = "Cabinet";
        public string buildable_cabinet_desc = "Medium-cost large storage solution";

        public string buildable_portablegen_displayName = "Portable Power Station";
        public string buildable_portablegen_desc = "Inserted power cells are used to provide power";

        public string buildable_largeSolarPanel_displayName = "(Advanced) Solar Panel";
        public string buildable_largeSolarPanel_desc = "Generates power more efficiently than a regular solar panel";

        public string largeSolarPanel_displayInfo = "(Advanced) Solar Panel (sun: {0}% charge: {1}/{2})";

        public string buildable_alarmSiren_displayName = "Alarm Siren";
        public string buildable_alarmSiren_desc = "AI powered alarm that triggers when your base is under attack, P.S: is used with the tower defense mod";

        public string buildable_rifleturretmk1_displayName = "Blaster MK1";
        public string buildable_rifleturretmk1_desc = "Medium-cost basic defense turret, used by the Degasi survivors";
        public string buildable_plasmaturret_displayName = "Plasma Turret";
        public string buildable_plasmaturret_desc = "High-cost high fire-rate advanced defense turret, used by the Degasi survivors";

        public string item_blasterturretmk1_ammo = "Ammunition Case (Blaster MK1)";
        public string item_plasmaturret_ammo = "Ammunition Case (Plasma Turret)";

        public string ui_turretConfig_title = "Turret Configuration";
        public string ui_turretConfig_label_targetingRange = "Targeting Range:";
        public string ui_turretConfig_label_targets = "Targets:";
        public string ui_turretConfig_label_tipMaxRange = "Max range: 1000";
        public string ui_turretConfig_label_tipTargets = $"(Note: type down a part of the creature's name you want to target, these names are separated by commas, eg: 'reaper, ghost, peep', targets any creature that has the name containing one of those words)";
        public string ui_turretConfig_btn_applyForThisTurret = "Apply (For This Turret)";
        public string ui_turretConfig_btn_applyForNearbyTurrets = "Apply (For Nearby Turrets)";
        public string ui_turretConfig_txtbox_targets_entertext = "Enter text...";

        public string txt_turretConfig_applyChanges_targetingRangeIsNotNumber = "failed to convert targeting range to a number";
        public string txt_turretConfig_applyChanges_turretsApplied = "{0} turrets within 100 radius has been modified";
        public string txt_turretConfig_applyChanges_turretApplied = "turret has been modified";
    }
}
