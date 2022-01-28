using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlappyBird
{
    public sealed class AudioSystem : AbstractSystem
    {

        /// <summary>
        /// 背景音乐优先级
        /// </summary>
        private int BackgroundPriority = 0;

        /// <summary>
        /// 单通道音效优先级
        /// </summary>
        private int SinglePriority = 10;

        /// <summary>
        /// 多通道音效优先级
        /// </summary>
        private int MultiplePriority = 20;

        /// <summary>
        /// 世界音效优先级
        /// </summary>
        private int WorldPriority = 30;

        private float backgroundVolume = 0.6f;
        /// <summary>
        /// 背景音乐音量
        /// </summary>
        private float BackgroundVolume
        {
            get => backgroundVolume;
            set
            {
                backgroundVolume = value;
                backgroundAudio.volume = value;
            }
        }
        /// <summary>
        /// 音效音量
        /// </summary>
        public float SoundEffectVolume = 1;

        private AudioSource backgroundAudio;
        private AudioSource singleAudio;
        private readonly List<AudioSourcePlayer> multipleAudio = new List<AudioSourcePlayer>();
        private readonly Dictionary<GameObject, AudioSource> worldAudio = new Dictionary<GameObject, AudioSource>();
        private bool isMute;
        private Transform root;
        
        protected override void OnInit()
        {
            backgroundAudio = CreateAudioSource("BackgroundAudio", BackgroundPriority, BackgroundVolume);
            singleAudio = CreateAudioSource("SingleAudio", SinglePriority, SoundEffectVolume);
            root = new GameObject(nameof(AudioSystem)).transform;
        }
        
        private class AudioSourcePlayer : IDisposable
        {
            private AudioSource _audioSource;

            public bool IsPlaying => _audioSource.isPlaying;

            public int Priority
            {
                get => _audioSource.priority;
                set => _audioSource.priority = value;
            }

            public float Volume
            {
                get => _audioSource.volume;
                set => _audioSource.volume = value;
            }

            public bool Mute
            {
                get => _audioSource.mute;
                set => _audioSource.mute = value;
            }

            public AudioSourcePlayer(AudioSource audioSource)
            {
                this._audioSource = audioSource;
            }

            public bool IsSameClip(AudioClip clip)
            {
                return _audioSource.clip == clip;
            }

            public bool IsSameClip(string name)
            {
                return _audioSource.clip.name == name;
            }

            public void SetParams(AudioClip clip, bool isLoop, float speed)
            {
                _audioSource.clip = clip;
                _audioSource.loop = isLoop;
                _audioSource.pitch = speed;
                _audioSource.spatialBlend = 0;
            }

            public void Play()
            {
                _audioSource.Play();
            }

            public void Stop()
            {
                _audioSource.Stop();
            }

            private void Release()
            {
                _audioSource.clip = null;
            }

            public void Dispose()
            {
                Release();
                Object.Destroy(_audioSource.gameObject);
                _audioSource = null;
            }
        }
        
        /// <summary>
        /// 静音
        /// </summary>
        public bool Mute
        {
            get => isMute;
            set
            {
                isMute = value;
                backgroundAudio.mute = value;
                singleAudio.mute = value;
                foreach (var audio in multipleAudio)
                {
                    audio.Mute = value;
                }

                foreach (KeyValuePair<GameObject, AudioSource> audio in worldAudio)
                {
                    audio.Value.mute = value;
                }
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBackgroundMusic(string clipName, bool isLoop = true, float speed = 1)
        {
            var clip = Resources.Load<AudioClip>(clipName);
            if (backgroundAudio.isPlaying)
            {
                backgroundAudio.Stop();
            }

            backgroundAudio.clip = clip;
            backgroundAudio.loop = isLoop;
            backgroundAudio.pitch = speed;
            backgroundAudio.spatialBlend = 0;
            backgroundAudio.Play();
        }

        /// <summary>
        /// 暂停播放背景音乐
        /// </summary>
        public void PauseBackgroundMusic(bool isGradual = true)
        {
            if (!backgroundAudio.isPlaying) return;
            // if (isGradual)
            // {
            //     _backgroundAudio.DOFade(0, 2);
            // }
            // else
            {
                backgroundAudio.volume = 0;
            }
        }

        /// <summary>
        /// 恢复播放背景音乐
        /// </summary>
        public void UnPauseBackgroundMusic(bool isGradual = true)
        {
            if (!backgroundAudio.isPlaying) return;
            // if (isGradual)
            // {
            //     _backgroundAudio.DOFade(BackgroundVolume, 2);
            // }
            // else
            {
                backgroundAudio.volume = BackgroundVolume;
            }
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (backgroundAudio.isPlaying)
            {
                backgroundAudio.Stop();
            }
        }

        /// <summary>
        /// 播放单通道音效
        /// </summary>
        public  void PlaySingleSound(string clipName, bool isLoop = false, float speed = 1)
        {
            var clip = Resources.Load<AudioClip>(clipName);
            if (singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }

            singleAudio.clip = clip;
            singleAudio.loop = isLoop;
            singleAudio.pitch = speed;
            singleAudio.spatialBlend = 0;
            singleAudio.Play();
        }

        /// <summary>
        /// 暂停播放单通道音效
        /// </summary>
        public void PauseSingleSound(bool isGradual = true)
        {
            if (!singleAudio.isPlaying) return;
            // if (isGradual)
            // {
            //     _singleAudio.DOFade(0, 2);
            // }
            // else
            {
                singleAudio.volume = 0;
            }
        }

        /// <summary>
        /// 恢复播放单通道音效
        /// </summary>
        public void UnPauseSingleSound(bool isGradual = true)
        {
            if (!singleAudio.isPlaying) return;
            // if (isGradual)
            // {
            //     _singleAudio.DOFade(SoundEffectVolume, 2);
            // }
            // else
            {
                singleAudio.volume = SoundEffectVolume;
            }
        }

        /// <summary>
        /// 停止播放单通道音效
        /// </summary>
        public void StopSingleSound()
        {
            if (singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }
        }
        
        /// <summary>
        /// 播放世界音效
        /// </summary>
        public void PlayWorldSound(GameObject attachTarget, string clipName, bool isLoop = false, float speed = 1)
        {
            var clip = Resources.Load<AudioClip>(clipName);
            if (worldAudio.ContainsKey(attachTarget))
            {
                AudioSource audio = worldAudio[attachTarget];
                if (audio.isPlaying)
                {
                    audio.Stop();
                }

                audio.clip = clip;
                audio.loop = isLoop;
                audio.pitch = speed;
                audio.spatialBlend = 1;
                audio.Play();
            }
            else
            {
                AudioSource audio = AttachAudioSource(attachTarget, WorldPriority, SoundEffectVolume);
                worldAudio.Add(attachTarget, audio);
                audio.clip = clip;
                audio.loop = isLoop;
                audio.pitch = speed;
                audio.spatialBlend = 1;
                audio.Play();
            }
        }

        /// <summary>
        /// 暂停播放指定的世界音效
        /// </summary>
        public void PauseWorldSound(GameObject attachTarget, bool isGradual = true)
        {
            if (!worldAudio.ContainsKey(attachTarget)) return;
            AudioSource audio = worldAudio[attachTarget];
            if (!audio.isPlaying) return;
            // if (isGradual)
            // {
            //     audio.DOFade(0, 2);
            // }
            // else
            {
                audio.volume = 0;
            }
        }

        /// <summary>
        /// 恢复播放指定的世界音效
        /// </summary>
        public void UnPauseWorldSound(GameObject attachTarget, bool isGradual = true)
        {
            if (!worldAudio.ContainsKey(attachTarget)) return;
            AudioSource audio = worldAudio[attachTarget];
            if (!audio.isPlaying) return;
            // if (isGradual)
            // {
            //     audio.DOFade(SoundEffectVolume, 2);
            // }
            // else
            {
                audio.volume = SoundEffectVolume;
            }
        }

        /// <summary>
        /// 停止播放所有的世界音效
        /// </summary>
        public void StopAllWorldSound()
        {
            foreach (KeyValuePair<GameObject, AudioSource> audio in worldAudio)
            {
                if (audio.Value.isPlaying)
                {
                    audio.Value.Stop();
                }
            }
        }

        /// <summary>
        /// 销毁所有闲置中的世界音效的音源
        /// </summary>
        public void ClearIdleWorldAudioSource()
        {
            HashSet<GameObject> removeSet = new HashSet<GameObject>();
            foreach (KeyValuePair<GameObject, AudioSource> audio in worldAudio)
            {
                if (!audio.Value.isPlaying)
                {
                    removeSet.Add(audio.Key);
                    Object.Destroy(audio.Value);
                }
            }

            foreach (GameObject item in removeSet)
            {
                worldAudio.Remove(item);
            }
        }

        /// <summary>
        /// 创建一个音源
        /// </summary>
        private AudioSource CreateAudioSource(string name, int priority, float volume)
        {
            GameObject audioObj = new GameObject(name);
            audioObj.transform.SetParent(root);
            audioObj.transform.localPosition = Vector3.zero;
            audioObj.transform.localRotation = Quaternion.identity;
            audioObj.transform.localScale = Vector3.one;
            AudioSource audio = audioObj.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.priority = priority;
            audio.volume = volume;
            audio.mute = isMute;
            return audio;
        }

        /// <summary>
        /// 创建一个音源包装类
        /// </summary>
        private AudioSourcePlayer CreateAudioSourcePlayer(string name, int priority, float volume)
        {
            var audio = CreateAudioSource(name, priority, volume);
            return new AudioSourcePlayer(audio);
        }

        /// <summary>
        /// 附加一个音源
        /// </summary>
        private AudioSource AttachAudioSource(GameObject target, int priority, float volume)
        {
            AudioSource audio = target.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.priority = priority;
            audio.volume = volume;
            audio.mute = isMute;
            return audio;
        }

        /// <summary>
        /// 提取闲置中的多通道音源
        /// </summary>
        private AudioSourcePlayer ExtractIdleMultipleAudioSource()
        {
            foreach (var audioSource in multipleAudio)
            {
                if (!audioSource.IsPlaying)
                {
                    return audioSource;
                }
            }

            var audio = CreateAudioSourcePlayer("MultipleAudio", MultiplePriority, SoundEffectVolume);
            multipleAudio.Add(audio);
            return audio;
        }

        public void Dispose()
        {
            StopBackgroundMusic();
            StopSingleSound();
            StopAllWorldSound();
        }
    }
}