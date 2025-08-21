using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using VanillaExpandedLoreFriendly.Items.Equipment;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace VanillaExpandedLoreFriendly.Buildables
{
    public class AlarmSiren : MonoBehaviour
    {
        private bool _alarming;
        public bool Alarming
        {
            get { return _alarming; }
            set { PlayOrStopAlarm(value); }
        }

        private MeshRenderer[] _renderers;
        private Light[] _lights;
        private Constructable _constructable;
        private FMOD_CustomEmitter _soundEmitter;
        private Transform t_rotator;

        private const float _lightIntensity = 3;


        private InGameConfig _inGameConfig;

        //private float _last_configVolume = -1;

        private bool _initialized = false;
        void Start()
        {
            _inGameConfig = Plugin.ingameConfig;

            // get components
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _lights = GetComponentsInChildren<Light>();
            _constructable = GetComponent<Constructable>();
            _soundEmitter = GetComponent<FMOD_CustomEmitter>();

            // find light rotator (transform)
            t_rotator = transform.Find("Rotator");
            foreach (Light _light in _lights) { _light.enabled = true; }


            // stop alarm
            PlayOrStopAlarm(false);
            StartCoroutine(_ISoundPlayer());

            _initialized = true;
        }

        
        private System.Collections.IEnumerator _ISoundPlayer()
        {
            float soundDuration = Registries.alarmSirenAudioClip.length - 0.1f;
            WaitForSeconds delay = new WaitForSeconds(0.1f);


            
            while(this != null)
            {
                // if alarming & sound is not playing
                if(Alarming)
                {
                    if (!_soundEmitter._playing)
                    {
                        // play sound & reset sound
                        _soundEmitter.Play();
                        _coroutine_resetSound = StartCoroutine(ResetSound(soundDuration));
                    }
                    _soundEmitter.SetVolume(_inGameConfig.alarmSirenVolume);
                }
                yield return delay;
            }
        }

        private Coroutine _coroutine_resetSound;
        private System.Collections.IEnumerator ResetSound(float duration)
        {
            yield return new WaitForSeconds(duration);

            if (this != null && Alarming && _soundEmitter.playing)
            {
                _soundEmitter.Stop();
            }
        }
        
        void OnDisable()
        {
            if (!_initialized) { return; }

            PlayOrStopAlarm(false, true);
        }

        public void PlayOrStopAlarm(bool playOrStop = true, bool forceLightsOff = false)
        {
            
            float glowStrength = (playOrStop) ? 35 : 0;
            foreach (MeshRenderer _renderer in _renderers)
            {
                Material[] mats = _renderer.materials;
                mats[1].SetFloat("_GlowStrength", glowStrength);
                mats[1].SetFloat("_GlowStrengthNight", glowStrength);
                mats[1].SetFloat("_EmissionLM", glowStrength);
                mats[1].SetFloat("_EmissionLMNight", glowStrength);
                _renderer.materials = mats;
            }

            if (!playOrStop)
            {
                _soundEmitter.Stop();
                if(_coroutine_resetSound != null)
                {
                    StopCoroutine(_coroutine_resetSound);
                    _coroutine_resetSound = null;
                }
            }
            if (forceLightsOff)
            {
                foreach (Light _light in _lights) { _light.intensity = 0; }
            }
            _alarming = playOrStop;
        }



        void Update()
        {
            
            // rotate lights 
            if (Alarming)
            {
                // if buildable is constructing
                if (!_constructable.constructed)
                {
                    // set alarming to false
                    Alarming = false;
                    return;
                }

                t_rotator.rotation *= Quaternion.Euler(0, 0, 350 * Time.deltaTime);
                
            }
            // lerp light intensity
            float intensityTarget = (_alarming) ? _lightIntensity : 0;

            foreach (Light _light in _lights)
            {
                _light.intensity = Mathf.Lerp(_light.intensity, intensityTarget, 4 * Time.deltaTime);
            }
        }

        /*

        private bool _alarming;

        /// <summary>is alarming?</summary>
        public bool Alarming
        {
            get { return _alarming; }
            set { PlayOrStopAlarm(value); }
        }

        private MeshRenderer[] _renderers;
        private Light[] _lights;
        private Light[] _lights_outer;
        private Constructable _constructable;
        private FMOD_CustomEmitter _soundEmitter;
        private Transform t_rotator;

        private const float intensity = 3;

        void Start()
        {
            // get components
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _lights = GetComponentsInChildren<Light>();
            _constructable = GetComponent<Constructable>();
            _soundEmitter = GetComponent<FMOD_CustomEmitter>();

            // get light rotator (transform)
            t_rotator = transform.Find("Rotator");

            List<Light> _lights_o = new List<Light>();

            // set light properties
            foreach (Light _light in _lights)
            {
                // set intensity & range
                float intensity = 10;

                // if its a spot light facing outside
                if (_light.gameObject.name.StartsWith("l_o"))
                {
                    _light.range = 9;
                    intensity = 0;
                    _lights_o.Add(_light);
                }
                else if (_light.gameObject.name.StartsWith("l_po"))
                {
                    // if its a point light
                    _lights_o.Add(_light);
                }
                _light.intensity = intensity;
            }
            _lights_outer = _lights_o.ToArray();
        }

        /// <summary>
        /// plays or stops an alarm
        /// </summary>
        /// <param name="play">play?</param>
        public void PlayOrStopAlarm(bool play = true)
        {
            // reset light intensity on stop
            if (!play)
            {
                foreach (Light _light in _lights_outer)
                {
                    _light.intensity = 0;
                }
            }

            // enable lights
            foreach (Light _light in _lights) { _light.enabled = play; }


            // change textures
            Texture2D texture = (Alarming) ? Vars.texture_alarm_bright_red : Vars.texture_alarm_dark_red;
            foreach (MeshRenderer _renderer in _renderers)
            {
                Material[] mats = _renderer.materials;
                mats[1].SetTexture("_MainTex", texture);
                _renderer.materials = mats;
            }
            _alarming = play;
        }
        */
        /*
        //public FMODAsset soundAsset;
        public void Start()
        {
            // get components
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _lights = GetComponentsInChildren<Light>();
            _constructable = GetComponent<Constructable>();
            _soundEmitter = GetComponent<FMOD_CustomEmitter>();

            // get light rotator (transform)
            t_rotator = transform.Find("Rotator");

            List<Light> _lights_o = new List<Light>();

            // set light properties
            foreach (Light _light in _lights)
            {
                // set intensity & range
                float intensity = 10;

                // if its a spot light facing outside
                if (_light.gameObject.name.StartsWith("l_o"))
                {
                    _light.range = 9;
                    intensity = 0;
                    _lights_o.Add(_light);
                } else if (_light.gameObject.name.StartsWith("l_po"))
                {
                    // if its a point light
                    _lights_o.Add(_light);
                }
                _light.intensity = intensity;
            }
            _lights_outer = _lights_o.ToArray();

            StartCoroutine(ISoundPlayer());
        }

            
        System.Collections.IEnumerator ISoundPlayer()
        {
            WaitForSeconds delay = new WaitForSeconds(1);

            while (this != null)
            {
                if(Alarming && !_soundEmitter._playing)
                {
                    _soundEmitter.Play();
                }

                yield return delay;
            }

        }


        /// <summary>
        /// plays or stops an alarm
        /// </summary>
        /// <param name="play">play?</param>
        public void PlayOrStopAlarm(bool play = true)
        {
            // reset light intensity on stop
            if (!play)
            {
                foreach (Light _light in _lights_outer)
                {
                    _light.intensity = 0;
                }
            }

            // enable lights
            foreach (Light _light in _lights) { _light.enabled = play; }


            // change textures
            Texture2D texture = (Alarming) ? Vars.texture_alarm_bright_red : Vars.texture_alarm_dark_red;
            foreach (MeshRenderer _renderer in _renderers)
            {
                Material[] mats = _renderer.materials;
                mats[1].SetTexture("_MainTex", texture);
                _renderer.materials = mats;
            }
            _alarming = play;
        }

      

        public void Toggle()
        {
            if (_alarming)
            {
                Alarming = false;
            }
            else { Alarming = true; }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Toggle();
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                _soundEmitter.SetVolume(0.1f);
            }
        }
        */
        /*
        
        private bool _alarming;
        public bool Alarming
        {
            get { return _alarming; } 
            set { PlayOrStopAlarm(_alarming); }
        }

        private FMOD_CustomEmitter _soundEmitter;
        private MeshRenderer[] _renderers;
        private Light[] lights;
        private Transform rotator;
        private Constructable _constructable;


        private InGameConfig _inGameConfig;
        private Coroutine _soundLoopCoroutine;

        void Start()
        {
            // ref to ingame config
            _inGameConfig = Plugin.ingameConfig;

            // get components and transforms
            _renderers = GetComponentsInChildren<MeshRenderer>();
            lights = GetComponentsInChildren<Light>();
            _soundEmitter = GetComponent<FMOD_CustomEmitter>();
            _constructable = GetComponent<Constructable>();
            rotator = transform.Find("Rotator");

            // set light properties
            foreach (Light _light in lights)
            {
                float intensity = 10;

                if (_light.gameObject.name.StartsWith("l_o"))
                {
                    _light.range = 9;
                    intensity = 3;
                }
                _light.intensity = intensity;
            }

            // update volume
            _UpdateVolume();

            StartCoroutine(IVolumeUpdater());
            StartCoroutine(IAlarmSoundPlayer());
        }

        // updates volume to config volume
        System.Collections.IEnumerator IVolumeUpdater()
        {
            WaitForSeconds delay = new WaitForSeconds(1);
            while(this != null)
            {
                _UpdateVolume();
                yield return delay;
            }
        }



        private float _last_updatedVolume = -1;
        private void _UpdateVolume()
        {
            // get config volume
            float configVolume = _inGameConfig.alarmSirenVolume;

            // update volume to config volume
            if(_last_updatedVolume != configVolume)
            {
                _last_updatedVolume = configVolume;
                _soundEmitter.evt.setVolume(configVolume);
                ErrorMessage.AddMessage($"temp volume set to {configVolume}");
            }
        }


        public void PlayOrStopAlarm(bool play = true)
        {
            // exit if is going to play while buildable is constructing
           if(play && !_constructable.constructed) { return; }

            // change textures
            Texture2D texture = (Alarming) ? Vars.texture_alarm_bright_red : Vars.texture_alarm_dark_red;
            foreach (MeshRenderer _renderer in _renderers)
            {
                Material[] mats = _renderer.materials;
                mats[1].SetTexture("_MainTex", texture);
                _renderer.materials = mats;
            }

            // play/stop sound
            if (!play) { _soundEmitter.Stop(); }

            // enable/disable lights
            foreach (Light _light in lights) { _light.enabled = play; }

            _alarming = play;
        }
        
        System.Collections.IEnumerator IAlarmSoundPlayer()
        {
            WaitForSeconds delay = new WaitForSeconds(0.1f);

            while(this != null) 
            {
                bool constructed = _constructable.constructed;
                // play sound
                if (Alarming && constructed && !_soundEmitter.playing)
                {
                    _soundEmitter.Play();
                    _soundEmitter.evt.setVolume(0.05f);
                    _soundEmitter.UpdateEventAttributes();
                }else if (!constructed)
                {
                    _soundEmitter.Stop();
                }

                yield return delay;
            }
        }

        
        void Update()
        {
            if (Alarming)
            {
                rotator.rotation *= Quaternion.Euler(0, 0, 200 * Time.deltaTime);
            }

            //#temp
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Alarming)
                {
                    PlayOrStopAlarm(false);
                }
                else
                {
                    PlayOrStopAlarm(true);
                }
            }
        }
        */
        /*
        

        // is alarming?
        private bool _isAlarming;
        public bool isAlarming
        {
            get { return _isAlarming; }
            set { _isAlarming = value; _SetAlarming(value); }
        }

        private MeshRenderer[] _renderers;
        private Light[] lights;
        private Transform t_rotator;
        private Constructable _constructable;
        private FMOD_CustomEmitter _soundEmitter;



        private float _last_alarmSirenVolume = -1;

        void Start()
        {
            lights = GetComponentsInChildren<Light>();
            t_rotator = transform.Find("Rotator");
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _constructable = GetComponent<Constructable>();
            StartCoroutine(SoundPlayer());
            _soundEmitter = GetComponent<FMOD_CustomEmitter>();
        }


        System.Collections.IEnumerator SoundPlayer()
        {

            // delay & condition
            WaitForSeconds delay = new WaitForSeconds(9.1f);
            WaitUntil waitUntil_alarming = new WaitUntil(() => isAlarming && _constructable.constructed);

            // while alarm siren exists
            while(this != null)
            {
                // wait for alarm
                yield return waitUntil_alarming;

                // play sound emitter
                _soundEmitter.Play();

                // wait for 9.1s
                yield return delay;
            }

          
        }


        void Update()
        {
            if (isAlarming)
            {
                t_rotator.rotation *= Quaternion.Euler(0, 0, 200 * Time.deltaTime);
            }




            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_isAlarming)
                {
                    isAlarming = false;
                } 
                else
                {
                    isAlarming = true;
                }
            }

        }

        private void _SetAlarming(bool alarm = true)
        {
            Texture2D texture = (alarm) ? Vars.texture_alarm_bright_red : Vars.texture_alarm_dark_red;
            
            // if alarming
            if (alarm)
            {

                // enable lights & set intensity

                foreach (Light _light in lights) {

                    _light.enabled = true;

                    float intensity = 10;
                    if (_light.gameObject.name.StartsWith("l_o"))
                    {
                        _light.range = 9;
                        intensity = 3;
                    }

                    _light.intensity = intensity;
                
                }
            }
            else
            {
                foreach (Light _light in lights) { _light.enabled = false; _light.intensity = 0; }
            }


            // change texture
            foreach (MeshRenderer _renderer in _renderers)
            {
                Material[] mats = _renderer.materials;
                mats[1].SetTexture("_MainTex", texture);
                _renderer.materials = mats;
            }

        }
        */
    }
}
