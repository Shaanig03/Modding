using FMOD;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaExpandedLoreFriendly.Buildables;

namespace VanillaExpandedLoreFriendly
{
    

    public static class Registries
    {
        public const MODE k3DSoundModes = MODE.DEFAULT | MODE._3D | MODE.ACCURATETIME | MODE._3D_LINEARSQUAREROLLOFF;
        public const MODE k2DSoundModes = MODE.DEFAULT | MODE._2D | MODE.ACCURATETIME;
        public const MODE kStreamSoundModes = k2DSoundModes | MODE.CREATESTREAM;


        // sounds
        //-----------
        public static string sound_scificrate_open;
        public static string sound_scificrate_close;

        public static string sound_locker_open;
        public static string sound_locker_close;

        public static string sound_cabinet_open;
        public static string sound_cabinet_close;

        public static string sound_alarmSirenSound;

        public static string sound_turretFire_rifleMK1;
        //-----------

        // uis
        public static GameObject prefab_ui_turretConfiguration;

        // indicators
        public static GameObject prefab_indicatorSphere;
        public static System.Collections.IEnumerator RegisterAll()
        {
            yield return new WaitUntil(() => SaveLoadManager.main != null);

            RegisterSounds();
            RegisterPoolObjects();
            RegisterBuildables();
            RegisterItems();
            RegisterUIs();
            RegisterIndicators();
        }

        private static void RegisterIndicators()
        {
            AssetBundle assetBundle = Vars.assetBundle;

            prefab_indicatorSphere = assetBundle.LoadAsset<GameObject>("VEIndicatorSphere");
        }
        private static void RegisterUIs()
        {
            AssetBundle assetBundle = Vars.assetBundle;

            prefab_ui_turretConfiguration = assetBundle.LoadAsset<GameObject>("CanvasTurretConfiguration");
            prefab_ui_turretConfiguration.AddComponent<UI.UITurretConfiguration>();
        }
        private static void RegisterItems()
        {
            AssetBundle assetBundle = Vars.assetBundle;

            var customPrefab_blasterTurretMK1Ammo = new CustomPrefab(
                    "turretblastermk1_ammo",
                    Vars.lang.item_blasterturretmk1_ammo,
                    "...",
                    assetBundle.LoadAsset<UnityEngine.Sprite>("icon_ammunitionCase")
            );

            // blaster turret mk1 ammo  
            //--------------------------------------------
            VEMethods.RegisterItem(customPrefab_blasterTurretMK1Ammo,
                new CloneTemplate(customPrefab_blasterTurretMK1Ammo.Info, TechType.Battery)
                {
                    ModifyPrefab = delegate (GameObject prefab)
                    {
                        VEMethods.ModifyItemToAmmoBox(prefab, 500);
                    }
                }, new Vector2int(1, 1),
                new Nautilus.Crafting.RecipeData
                {
                    Ingredients =
                    {
                        new Ingredient(TechType.Titanium, 1),
                        new Ingredient(TechType.CreepvineSeedCluster, 2)
                    },
                    craftAmount = 4
                }
                ,
                CraftTree.Type.Fabricator,
                CraftTreeHandler.Paths.FabricatorsBasicMaterials,
                TechGroup.Resources,
                TechCategory.BasicMaterials,
                TechType.None,
                0);
            //--------------------------------------------
        }

        private static void RegisterPoolObjects()
        {
            
            AssetBundle assetBundle = Vars.assetBundle;

            // create pools
            PoolDefs.CreatePool("fx_muzzlesmoke", assetBundle.LoadAsset<GameObject>("FxMuzzleSmoke"), Vars.config.poolSize_fxMuzzleSmoke, 1.5f);
            VETurretMethods.CreateBulletPool("bullet_blastermk1", assetBundle.LoadAsset<GameObject>("BlasterMK1Projectile"), Vars.config.poolSize_bullet_blastermk1, 5f, 10, 8000);

            
        }

        public static AudioClip alarmSirenAudioClip;

        private static void RegisterSounds()
        {
            AssetBundle assetBundle = Vars.assetBundle;

            sound_scificrate_open = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("sci-fi_crate_open"), "ve_crate_open");
            sound_scificrate_close = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("sci-fi_crate_close"), "ve_crate_close");

            sound_locker_open = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("locker_open"), "ve_locker_open");
            sound_locker_close = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("locker_close"), "ve_locker_close");

            sound_cabinet_open = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("cabinet_open"), "ve_cabinet_open");
            sound_cabinet_close = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("cabinet_close"), "ve_cabinet_close");

            sound_turretFire_rifleMK1 = RegisterSoundEffect(assetBundle.LoadAsset<AudioClip>("rifleTurretFireSound"), "ve_rifleTurretFireSound");



            alarmSirenAudioClip = Vars.assetBundle.LoadAsset<AudioClip>("alarm_siren_loop");
            RegisterAlarmSirenSound();

        }


        private static void RegisterAlarmSirenSound()
        {
            sound_alarmSirenSound = RegisterSoundEffect(alarmSirenAudioClip, "ve_alarm_siren_loop", 1, 500, null, Plugin.ingameConfig.alarmSirenVolume);
        }

        private static void RegisterBuildables()
        {

            AssetBundle assetBundle = Vars.assetBundle;
            Plugin plugin = Plugin.Get;

            // crate
            //--------------------------------------------
            GameObject prefab_crate = assetBundle.LoadAsset<GameObject>("Sci-fi_Crate");
            Constructable constructable_crate;
            VEMethods.RegisterBuildableObject
            (
                "ve_scificrate",
                Vars.lang.buildable_crate_displayName,
                Vars.lang.buildable_crate_desc,
                assetBundle.LoadAsset<Sprite>("icon_sci-fi_crate"),
                prefab_crate,
                TechGroup.InteriorModules,
                TechCategory.InteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.Titanium, 2) }),
                TechType.None,
                0,
                out constructable_crate
            );

            VEMethods.RegisterStorage(prefab_crate, "scifi_crate", "scifi_crate", 7, 5, Registries.sound_scificrate_open, Registries.sound_scificrate_open, true);
            //PrefabUtils.AddStorageContainer(prefab_crate, "crate", "crate", 7, 6, true);
            constructable_crate.DefaultSettings(true);
            //--------------------------------------------


            // locker
            //--------------------------------------------
            GameObject prefab_locker = assetBundle.LoadAsset<GameObject>("Locker");
            Constructable constructable_locker;
            VEMethods.RegisterBuildableObject
            (
                "ve_locker",
                Vars.lang.buildable_locker_displayName,
                Vars.lang.buildable_locker_desc,
                assetBundle.LoadAsset<Sprite>("icon_locker"),
                prefab_locker,
                TechGroup.InteriorModules,
                TechCategory.InteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.Titanium, 3) }),
                TechType.None,
                0,
                out constructable_locker
            );

            VEMethods.RegisterStorage(prefab_locker, "locker", "locker", 5, 8, Registries.sound_locker_open, Registries.sound_locker_close, true);
            constructable_locker.DefaultSettings(true);
            //--------------------------------------------

            
            // cabinet
            //--------------------------------------------
            GameObject prefab_cabinet = assetBundle.LoadAsset<GameObject>("Cabinet");
            Constructable constructable_cabinet;
            VEMethods.RegisterBuildableObject
            (
                "ve_cabinet",
                Vars.lang.buildable_cabinet_displayName,
                Vars.lang.buildable_cabinet_desc,
                assetBundle.LoadAsset<Sprite>("icon_cabinet"),
                prefab_cabinet,
                TechGroup.InteriorModules,
                TechCategory.InteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.Titanium, 6) }),
                TechType.None,
                0,
                out constructable_cabinet
            );

            VEMethods.RegisterStorage(prefab_cabinet, "cabinet", "cabinet", 9, 9, Registries.sound_cabinet_open, Registries.sound_cabinet_close, true);
            constructable_cabinet.DefaultSettings(true);    
            //--------------------------------------------
            
            // power gen
            //--------------------------------------------
            GameObject prefab_powergen = assetBundle.LoadAsset<GameObject>("PortableGen");
            Constructable constructable_powergen;
            VEMethods.RegisterBuildableObject
            (
                "ve_portablegen",
                Vars.lang.buildable_portablegen_displayName,
                Vars.lang.buildable_portablegen_desc,
                assetBundle.LoadAsset<Sprite>("icon_powergen"),
                prefab_powergen,
                TechGroup.InteriorModules,
                TechCategory.InteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.Titanium, 2), new Ingredient(TechType.Copper, 2) }),
                TechType.None,
                0,
                out constructable_powergen
            );

            VEMethods.RegisterStorage(prefab_powergen, "portablegen", "portablegen", 1, 2, "", "", false);
            constructable_powergen.DefaultSettings(true);
            prefab_powergen.AddComponent<PortableGen>();
            VEMethods.RegisterPowerThing(prefab_powergen, 800);
            //--------------------------------------------


            // (advanced) solar panel
            //--------------------------------------------
            GameObject prefab_largeSolarPanel = assetBundle.LoadAsset<GameObject>("LargeSolarPanel");
            Constructable constructable_largeSolarPanel;
            VEMethods.RegisterBuildableObject
            (
                "ve_largesolarpanel",
                Vars.lang.buildable_largeSolarPanel_displayName,
                Vars.lang.buildable_largeSolarPanel_desc,
                assetBundle.LoadAsset<Sprite>("icon_largesolarpanel"),
                prefab_largeSolarPanel,
                TechGroup.ExteriorModules,
                TechCategory.ExteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.TitaniumIngot, 2), new Ingredient(TechType.Copper, 12), new Ingredient(TechType.Quartz, 12) }),
                TechType.None,
                0,
                out constructable_largeSolarPanel
            );

            //VEMethods.RegisterStorage(prefab_largeSolarPanel, "portablegen", "portablegen", 1, 2, "", "", false);
            constructable_largeSolarPanel.DefaultSettings(false);
            constructable_largeSolarPanel.placeMaxDistance = 20;
            LargeSolarPanel largeSolarPanel = prefab_largeSolarPanel.AddComponent<LargeSolarPanel>();
            plugin.StartCoroutine(VEMethods.ISolarPanelSetDepthCurve(largeSolarPanel));
            VEMethods.RegisterPowerThing(prefab_largeSolarPanel, 1275);
            //--------------------------------------------


            // alarm siren
            //--------------------------------------------
            GameObject prefab_alarmSiren = assetBundle.LoadAsset<GameObject>("AlarmSiren");
            Constructable constructable_alarmSiren;
            VEMethods.RegisterBuildableObject
            (
                "ve_alarmsiren",
                Vars.lang.buildable_alarmSiren_displayName,
                Vars.lang.buildable_alarmSiren_desc,
                assetBundle.LoadAsset<Sprite>("icon_alarmsiren"),
                prefab_alarmSiren,
                TechGroup.InteriorModules,
                TechCategory.InteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Copper, 1), new Ingredient(TechType.Quartz, 1) }),
                TechType.None,
                0,
                out constructable_alarmSiren
            );


            constructable_alarmSiren.DefaultSettings(true, true);
            constructable_alarmSiren.placeMaxDistance = 10;
            AlarmSiren alarmSiren = prefab_alarmSiren.AddComponent<AlarmSiren>();
            FMOD_CustomEmitter soundEmitter_alarmSiren = prefab_alarmSiren.AddComponent<FMOD_CustomEmitter>();
            soundEmitter_alarmSiren.asset = AudioUtils.GetFmodAsset(sound_alarmSirenSound);
            soundEmitter_alarmSiren.restartOnPlay = true;
            //--------------------------------------------

            // other
            //--------------------------------------------
            //--------------------------------------------

            // (advanced) rifle turret mk1
            //--------------------------------------------
            GameObject prefab_rifleTurretMK1 = assetBundle.LoadAsset<GameObject>("BlasterMK1");
            Constructable constructable_rifleTurretMK1;
            VEMethods.RegisterBuildableObject
            (
                "ve_rifleturretmk1",
                Vars.lang.buildable_rifleturretmk1_displayName,
                Vars.lang.buildable_rifleturretmk1_desc,
                assetBundle.LoadAsset<Sprite>("icon_rifleturretmk1"),
                prefab_rifleTurretMK1,
                TechGroup.ExteriorModules,
                TechCategory.ExteriorModule,
                new Nautilus.Crafting.RecipeData(new Ingredient[] { new Ingredient(TechType.TitaniumIngot, 1), new Ingredient(TechType.ComputerChip, 1)}),
                TechType.None,
                0,
                out constructable_rifleTurretMK1
            );

            // add turret rifle mk1 component
            TurretRifleMK1 turretRifleMK1 = prefab_rifleTurretMK1.AddComponent<TurretRifleMK1>();
            turretRifleMK1.fireDelay = 0.7f;
            Transform t_rifleTurretMK1_turret = prefab_rifleTurretMK1.transform.Find("Model/0/Armature.003/TurretHolder 1/TurretCore 1/turret");

            // set fire sound
            turretRifleMK1.fire_soundName = sound_turretFire_rifleMK1;

            // set rotators
            turretRifleMK1.rotators = new GameObject[]
            {
                turretRifleMK1.transform.Find("Model/0/Armature.003/TurretHolder 1/TurretCore 1").gameObject,
                turretRifleMK1.transform.Find("Model/0/Armature.003/TurretHolder 1/TurretCore 1/turret").gameObject
            };

            // set fire fx muzzles
            turretRifleMK1.fire_fx_muzzles = new ParticleSystem[]
            {
                t_rifleTurretMK1_turret.Find("fx_muzzle").GetComponent<ParticleSystem>(),
                t_rifleTurretMK1_turret.Find("fx_muzzle (1)").GetComponent<ParticleSystem>(),
            };

            // register turret
            VETurretMethods.RegisterTurret(prefab_rifleTurretMK1, "blastermk1", turretRifleMK1, 250);
            prefab_rifleTurretMK1.GetComponent<TargetingCore>().visionObject = prefab_rifleTurretMK1.transform.Find("Model/0/Armature.003/TurretHolder 1/TurretCore 1").gameObject;


            // construction settings
            constructable_rifleTurretMK1.DefaultSettings(false);
            constructable_rifleTurretMK1.placeMaxDistance = 10;
            //--------------------------------------------

        }

        private static string RegisterSoundEffect(AudioClip clip, string soundPath, float minDistance = 1f, float maxDistance = 100f, string overrideBus = null, float volume = -1)
        {
            var sound = AudioUtils.CreateSound(clip, k3DSoundModes);
            
            if (maxDistance > 0f)
            {
                sound.set3DMinMaxDistance(minDistance, maxDistance);
            }
            CustomSoundHandler.RegisterCustomSound(soundPath, sound, string.IsNullOrEmpty(overrideBus) ? AudioUtils.BusPaths.PlayerSFXs : overrideBus);
            return soundPath;
        }
    }

}
