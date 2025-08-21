using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class LargeSolarPanel : MonoBehaviour, IHandTarget, IConstructable, IObstacle
    {
        private float GetDepthScalar()
        {
            float time = Mathf.Clamp01((this.maxDepth - Ocean.GetDepthOf(base.gameObject)) / this.maxDepth);
            return this.depthCurve.Evaluate(time);
        }

        // Token: 0x0600222D RID: 8749 RVA: 0x000A6338 File Offset: 0x000A4538
        private float GetSunScalar()
        {
            return DayNightCycle.main.GetLocalLightScalar() * this.biomeSunlightScale;
        }

        // Token: 0x0600222E RID: 8750 RVA: 0x000A634B File Offset: 0x000A454B
        private float GetRechargeScalar()
        {
            return this.GetDepthScalar() * this.GetSunScalar();
        }
        private Constructable _constructable;

        // Token: 0x0600222F RID: 8751 RVA: 0x000A635A File Offset: 0x000A455A
        private void Start()
        {
            powerSource = GetComponent<PowerSource>();
            relay = GetComponent<PowerRelay>();
            _constructable = GetComponent<Constructable>();
            AtmosphereDirector.onVolumeAdded += this.OnAtmosphereVolumeAdded;

            StartCoroutine(IPowerGen());
        }

        // Token: 0x06002230 RID: 8752 RVA: 0x000A636D File Offset: 0x000A456D
        private void OnDestroy()
        {
            AtmosphereDirector.onVolumeAdded -= this.OnAtmosphereVolumeAdded;
        }

        private System.Collections.IEnumerator IPowerGen()
        {
            WaitForSeconds delay = new WaitForSeconds(0.05f);
            while(this != null)
            {
                if (_constructable.constructed)
                {
                    float amount = (this.GetRechargeScalar() * DayNightCycle.main.deltaTime * 1f * 25f);
                    float amountStored;
                    powerSource.AddEnergy(amount, out amountStored);
                }
                yield return delay;
            }
        }

        // Token: 0x06002232 RID: 8754 RVA: 0x000A63CC File Offset: 0x000A45CC
        private void OnAtmosphereVolumeAdded(AtmosphereVolume volume)
        {
            WaterscapeVolume.Settings settings;
            if (volume.Contains(base.transform.position) && WaterBiomeManager.main.GetSettings(volume.overrideBiome, out settings))
            {
                this.biomeSunlightScale = Mathf.Clamp01(settings.sunlightScale);
            }
        }

        // Token: 0x06002233 RID: 8755 RVA: 0x000A6414 File Offset: 0x000A4614
        void IConstructable.OnConstructedChanged(bool constructed)
        {
            WaterscapeVolume.Settings settings;
            if (constructed && WaterBiomeManager.main.GetSettings(base.transform.position, true, out settings))
            {
                this.biomeSunlightScale = Mathf.Clamp01(settings.sunlightScale);
            }
        }

        // Token: 0x06002234 RID: 8756 RVA: 0x0000B942 File Offset: 0x00009B42
        public bool IsDeconstructionObstacle()
        {
            return true;
        }

        // Token: 0x06002235 RID: 8757 RVA: 0x0001D163 File Offset: 0x0001B363
        bool IObstacle.CanDeconstruct(out string reason)
        {
            reason = null;
            return true;
        }

        // Token: 0x06002236 RID: 8758 RVA: 0x000A6450 File Offset: 0x000A4650
        public void OnHandHover(GUIHand hand)
        {
            if (base.gameObject.GetComponent<Constructable>().constructed)
            {
                HandReticle.main.SetText(HandReticle.TextType.Hand, string.Format(Vars.lang.largeSolarPanel_displayInfo, Mathf.RoundToInt(this.GetRechargeScalar() * 100f), Mathf.RoundToInt(this.powerSource.GetPower()), Mathf.RoundToInt(this.powerSource.GetMaxPower())), false, GameInput.Button.None);
                //HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int, int>("SolarPanelStatus", , , , false, GameInput.Button.None);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            }
        }

        // Token: 0x06002237 RID: 8759 RVA: 0x00002319 File Offset: 0x00000519
        public void OnHandClick(GUIHand hand)
        {
        }

        // Token: 0x04001F0D RID: 7949
        public PowerSource powerSource;

        // Token: 0x04001F0E RID: 7950
        [AssertNotNull]
        public PowerRelay relay;

        // Token: 0x04001F0F RID: 7951
        public float maxDepth = 300f;

        // Token: 0x04001F10 RID: 7952
        [AssertNotNull]
        public AnimationCurve depthCurve;

        // Token: 0x04001F11 RID: 7953
        [AssertLocalization(3)]
        private const string solarPanelStatusFormatKey = "SolarPanelStatus";

        // Token: 0x04001F12 RID: 7954
        private const int currentVersion = 1;

        // Token: 0x04001F13 RID: 7955
        [ProtoMember(1)]
        [NonSerialized]
        public int version = 1;

        // Token: 0x04001F14 RID: 7956
        [ProtoMember(2)]
        [NonSerialized]
        public float biomeSunlightScale = 1f;

    }
}
