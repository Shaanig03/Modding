using Nautilus.Json;
using Nautilus.Options.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaExpandedLoreFriendly
{
    [Menu(PluginInfo.PLUGIN_NAME)]
    public class InGameConfig : ConfigFile
    {
        [Slider("Alarm Siren (Volume)", Format = "{0:F1}x", DefaultValue = 0.5f, Min = 0f, Max = 0.8f, Step = 0.001f)]
        public float alarmSirenVolume = 0.5f;

        [Slider("Turret Fire (Volume)", Format = "{0:F1}x", DefaultValue = 0.5f, Min = 0f, Max = 1f, Step = 0.001f)]
        public float turretFireVolume = 0.5f;
    }
}
