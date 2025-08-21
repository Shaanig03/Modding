using FMOD;
using FMOD.Studio;
using FMODUnity;
using mset;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VR;
using VanillaExpandedLoreFriendly.Buildables;
using static UnityStandardAssets.ImageEffects.BloomOptimized;

namespace VanillaExpandedLoreFriendly
{
    
    public static class VEMethods
    {
        public static void ModifyItemToAmmoBox(GameObject prefab, int ammunitionCapacity)
        {
            AssetBundle assetBundle = Vars.assetBundle;

            // get transform model
            Transform t_model = prefab.transform.Find("model");

            // hide the real prefab's display object
            foreach (Transform _t in t_model)
            {
                _t.gameObject.SetActive(false);
            }


            // get ammunition case prefab
            GameObject prefab_ammunitionCase = assetBundle.LoadAsset<GameObject>("AmmunitionCase");

            // clone ammunition case
            GameObject ammunitionCase = GameObject.Instantiate(prefab_ammunitionCase);
            ammunitionCase.name = "AmmunitionCase";
            ammunitionCase.transform.SetParent(t_model);
            ammunitionCase.transform.localPosition = Vector3.zero;
            ammunitionCase.transform.localRotation = Quaternion.identity;

            // apply shaders for the ammunition case and add components
            MaterialUtils.ApplySNShaders(ammunitionCase, 4f, 1f, 1f, Array.Empty<MaterialModifier>());

            // set ammunition capacity
            Battery battery = prefab.GetComponent<Battery>();
            battery._capacity = ammunitionCapacity;
            battery.charge = ammunitionCapacity;

            // get vfx fabricating component
            VFXFabricating vfxFabricating = t_model.GetComponent<VFXFabricating>();
            vfxFabricating.posOffset = new Vector3(0.0001f, 0.255f, 0.038f);
        }

        public static void RegisterItem(
                CustomPrefab customPrefab,
                PrefabTemplate prefabTemplate,
                Vector2int itemSize,
                RecipeData recipeData,
                CraftTree.Type craftTreeType,
                string[] stepsToFabricator, // CraftTreeHandler.Paths.FabricatorsBasicMaterials
                TechGroup techGroup,
                TechCategory techCategory,
                TechType unlockTechType,
                int unlockFragmentsRequired
            )
        {
            customPrefab.Info = customPrefab.Info.WithSizeInInventory(itemSize);
            customPrefab.SetGameObject(prefabTemplate);

            if (recipeData != null) { customPrefab.SetRecipe(recipeData).WithFabricatorType(CraftTree.Type.Fabricator).WithStepsToFabricatorTab(stepsToFabricator); }
            customPrefab.SetPdaGroupCategory(techGroup, techCategory);
            customPrefab.SetUnlock(unlockTechType, unlockFragmentsRequired);

            customPrefab.Register();
        }
        public static float SignedAngle(this Quaternion rotationA, Quaternion rotationB)
        {
            // get a "forward vector" for each rotation
            var forwardA = rotationA * Vector3.forward;
            var forwardB = rotationB * Vector3.forward;

            // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
            var angleA = Mathf.Atan2(forwardA.y, forwardA.z) * Mathf.Rad2Deg;
            var angleB = Mathf.Atan2(forwardB.y, forwardB.z) * Mathf.Rad2Deg;

            // get the signed difference in these angles
            var angleDiff = Mathf.DeltaAngle(angleA, angleB);
            return angleDiff;
        }

        public static Texture2D LoadTexture(string filePath, int resoWidth, int resoHeight)
        {
            byte[] data = File.ReadAllBytes(filePath);
            Texture2D returnTexture = new Texture2D(resoWidth, resoHeight);
            returnTexture.LoadImage(data);
            return returnTexture;
        }

        public static System.Collections.IEnumerator ISolarPanelSetDepthCurve(LargeSolarPanel largeSolarPanel)
        {
            yield return new WaitUntil(() => Vars.vanillaSolarPanel != null);

            largeSolarPanel.depthCurve = Vars.vanillaSolarPanel.GetComponent<SolarPanel>().depthCurve;
        }

        public static System.Collections.IEnumerator GetVanillaPrefabs()
        {
            // get vanilla solar panel
            TaskResult<GameObject> currentResult_solarPanel = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel, false, currentResult_solarPanel);
            Vars.vanillaSolarPanel = currentResult_solarPanel.Get();
        }

        public static System.Collections.IEnumerator IRegisterPowerThing(this GameObject prefab)
        {
            // wait until vanilla solar panel is retrieved
            yield return new WaitUntil(() => Vars.vanillaSolarPanel != null);

            GameObject powerFX_attachPoint = new GameObject("powerFX_AttachPoint");
            powerFX_attachPoint.transform.SetParent(prefab.transform);
            powerFX_attachPoint.transform.localPosition = Vector3.zero;


            GameObject vanillaSolarPanel = Vars.vanillaSolarPanel;

            PowerFX vanillaPowerFX = vanillaSolarPanel.GetComponent<PowerFX>();
            PowerRelay vanillaPowerRelay = vanillaSolarPanel.GetComponent<PowerRelay>();

            PowerFX powerFX = prefab.EnsureComponent<PowerFX>();
            powerFX.attachPoint = powerFX_attachPoint.transform;
            powerFX.vfxPrefab = vanillaPowerFX.vfxPrefab;
            vanillaPowerFX.vfxVisible = true;


            PowerRelay powerRelay = prefab.EnsureComponent<PowerRelay>();
            powerRelay.powerSystemPreviewPrefab = vanillaPowerRelay.powerSystemPreviewPrefab;
            powerRelay.constructable = prefab.GetComponent<Constructable>();
            powerRelay.internalPowerSource = prefab.GetComponent<PowerSource>();
            powerRelay.maxOutboundDistance = 15;
            powerRelay.lastCanConnect = true;
            powerRelay.powerFX = powerFX;
        }

        public static void RegisterPowerThing(this GameObject prefab, float maxPower)
        {
            // add power source component
            PowerSource powerSource = prefab.EnsureComponent<PowerSource>();
            powerSource.maxPower = maxPower;

            Plugin.Get.StartCoroutine(IRegisterPowerThing(prefab));
        }

        public static void PlayFMODSound(string soundName, Transform position, float soundRadiusObsolete = 20)
        {
            var asset = Nautilus.Utility.AudioUtils.GetFmodAsset(soundName);
            Utils.PlayFMODAsset(asset, position, soundRadiusObsolete);
        }

        public static void RegisterStorage(this GameObject prefab, string rootName,string rootClassID, int width, int height, string openSound, string closeSound, bool animated = true)
        {
            PrefabUtils.AddStorageContainer(prefab, rootName, rootClassID, width, height, true);
            if (animated) {
                StorageAnimator storageAnimator = prefab.AddComponent<StorageAnimator>();
                storageAnimator.animator = prefab.GetComponentInChildren<Animator>();
                storageAnimator.sound_open = openSound;
                storageAnimator.sound_close = closeSound;
            }
        }

        public static void DefaultSettings(this Constructable constructable, bool inside = true, bool ceilingOnly = false)
        {
            if (inside)
            {
                constructable.allowedInBase = true;
                constructable.allowedInSub = true;

                constructable.allowedOutside = false;
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;

                constructable.allowedOnCeiling = ceilingOnly;

            }
            else
            {
                constructable.allowedOnGround = true;
                constructable.allowedOutside = true;

                constructable.allowedOnWall = false;
                constructable.allowedInSub = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedInBase = false;
            }
            constructable.rotationEnabled = true;
        }
        public static CustomPrefab RegisterBuildableObject
            (
                string className,
                string displayName,
                string desc,
                Sprite icon,
                GameObject prefab,
                TechGroup techGroup,
                TechCategory techCategory,
                RecipeData recipes,
                TechType unlockTechType,
                int fragmentsToScan,
                out Constructable constructable
            )
        {
            

            // add basic components
            PrefabUtils.AddBasicComponents(prefab, className, TechType.None, LargeWorldEntity.CellLevel.Near);

            // handle renderers & materials
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            prefab.GetComponent<SkyApplier>().renderers = renderers;


            // get subnautica object shader
            Shader shader = Shader.Find("MarmosetUBER");

            foreach (Renderer _renderer in renderers)
            {
                // ignore particle system renderer
                if(!(_renderer is ParticleSystemRenderer))
                {
                    Material[] sharedMats = _renderer.sharedMaterials;
                    for(int i = 0; i < sharedMats.Length; i++)
                    {
                        sharedMats[i].shader = shader;
                        sharedMats[i].EnableKeyword("MARMO_EMISSION");
                        sharedMats[i].EnableKeyword("MARMO_SPECMAP");
                    }
                }
            }

            constructable = prefab.AddComponent<Constructable>();
            constructable.model = prefab.transform.GetChild(0).gameObject;
            constructable.rotationEnabled = true;

            // destroy physics components
            Rigidbody rigi = prefab.GetComponent<Rigidbody>();
            WorldForces worldForces = prefab.GetComponent<WorldForces>();
            if(rigi != null) { UnityEngine.Object.DestroyImmediate(rigi); }
            if (worldForces != null) { UnityEngine.Object.DestroyImmediate(worldForces); }


            // set constructable bounds
            ConstructableBounds ctBounds = prefab.AddComponent<ConstructableBounds>();
            Transform t_ctBounds = prefab.FindChild("ConstructionBounds").transform;
            BoxCollider boundBoxCol = t_ctBounds.GetComponent<BoxCollider>();

            ctBounds.bounds.position = boundBoxCol.center;
            ctBounds.bounds.size = boundBoxCol.size;
            GameObject.Destroy(t_ctBounds.gameObject);

            // ensure tech tag component
            prefab.EnsureComponent<TechTag>();

            // register buildable object
            CustomPrefab customPrefab = new CustomPrefab(className, displayName, desc, icon);
            GadgetExtensions.SetPdaGroupCategory(customPrefab, techGroup, techCategory).SetBuildable(true); ;
            GadgetExtensions.SetRecipe(customPrefab, recipes);
            GadgetExtensions.SetUnlock(customPrefab, unlockTechType, 1);

            customPrefab.SetGameObject(prefab);
            customPrefab.Register();

            return customPrefab;
        }
    }

    public static class FModExtensions
    {

        // method whipped up by @Metious
        public static void SetVolume(this FMOD_CustomEmitter emitter, float amount)
        {
            if (emitter.evt.isValid()) // vanilla event, set volume normally
            {
                emitter.evt.setVolume(amount);
                return;
            }

            // custom event, set volume via channel
            if (CustomSoundHandler.TryGetCustomSoundChannel(emitter.GetInstanceID(), out var channel))
            {
                channel.setVolume(amount);
                return;
            }

            // event is neither vanilla nor custom, do error handling here
        }
    }

}
