#region USED NAMESPACES

    using System;
    using System.Collections;
    using System.Collections.Generic;
    
    using JetBrains.Annotations;
    using DG.Tweening;
    using UnityEngine;

#endregion


namespace SenseQuiz.Gameplay.Managers {

    /// <summary> Exposes a simple API to handle audios at runtime. </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour {
        
        #region NON-SERIALIZED FIELDS

            /// <summary> Global access instance variable. </summary>
            public static AudioManager Instance = null;
            
            /// <summary> The dictionary with all the active audio sources. </summary>
            private Dictionary<string, AudioSource> _activeSources = new Dictionary<string, AudioSource>();

        #endregion
        

        #region SERIALIZED FIELDS

            /// <summary> A list with all the files with audios. </summary>
            [field: SerializeField, Tooltip("A list with all the files with audios."), Space] [PublicAPI]
            private List <AudioFile> _clips;

        #endregion
        
        
        #region UNITY EVENTS

            private void Awake () => AudioManager.Instance = this;

        #endregion

        
        #region METHODS
        
            #region PUBLIC

                /**
                 * <summary> Fades in the sound with the given key in the given duration. </summary>
                 *
                 * <param name = "key"> The key of the sound to fade. </param>
                 * <param name = "duration"> The duration of the fade. </param>
                 *
                 * <returns> The audio source of the sound (valid while only while is playing). </returns>
                */
                
                [CanBeNull, PublicAPI] public AudioSource FadeInLooped (string key, float duration, [CanBeNull] Action complete = null) {
                    
                    // Find and assign the clip.
                    var clip = this._clips.Find(current => current.Key == key);

                    // If the clip is valid and not already playing.
                    if (clip != null) {
                        
                        AudioSource source;
                        
                        if (!(this._activeSources.ContainsKey(key))) {
                            
                            // Add a source for this audio.
                            source = this.gameObject.AddComponent<AudioSource>();
                            this._activeSources.Add(key, source);

                            source.loop = true;
                            source.playOnAwake = false;
                            source.clip = clip.AudioClip;
                            source.volume = 0f;
                            
                            source.Play();
                        }

                        else {

                            source = this._activeSources [key];
                            source.volume = 0f;
                        }
                        
                        var tween = DOTween.To(() => source.volume, (value) => source.volume = value, 1.0f, duration);
                        tween.onComplete += () => complete?.Invoke();
                        
                        return source;
                    }

                    return null;
                }


                /**
                 * <summary> Fades the given sound out in the given duration. </summary>
                 *
                 * <param name = "key"> The key of the sound to fade out. </param>
                 * <param name = "duration"> The duration of the fade. </param>
                 * <param name = "complete"> Callback fired when the fade ends. </param>
                 * <param name = "destroy"> Should the audio clip be destroyed after the fade? </param>
                */
                
                [PublicAPI] public void FadeOut ([NotNull] string key, float duration, [CanBeNull] Action complete = null, bool destroy = false) {
                    
                    // If it's actually playing:
                    if (this._activeSources.ContainsKey(key)) {

                        var source = this._activeSources [key];

                        if (source != null) {
                            
                            var tween = DOTween.To(() => source.volume, (value) => source.volume = value, 0.0f, duration);
                            tween.onComplete += () => {

                                if (destroy) {
                                    
                                    source.Stop();
                                    Destroy(source);

                                    this._activeSources.Remove(key);
                                }
                                
                                complete?.Invoke();
                            };
                        }
                    }
                }
            

                /**
                 * <summary> Tries to play, if exists, the sound with the given key. </summary>
                 * <param name = "key"> The key of the sound to play. </param>
                 *
                 * <returns> The audio source of the sound (valid while only while is playing). </returns>
                */
                
                [PublicAPI, CanBeNull] public AudioSource PlaySound (string key, [CanBeNull] Action complete = null) {

                    // Find and assign the clip.
                    var clip = this._clips.Find(current => current.Key == key);

                    // If the clip is valid and not already playing.
                    if (clip != null && !(this._activeSources.ContainsKey(key))) {
                        
                        // Add a source for this audio.
                        var source = this.gameObject.AddComponent<AudioSource>();
                        this._activeSources.Add(key, source);
                        
                        // Configure it.
                        source.playOnAwake = false;
                        source.clip = clip.AudioClip;
                        source.Play();

                        // Callback fired after the clip ends.
                        IEnumerator EndCallback () {

                            yield return new WaitForSeconds(source.clip.length);
                            this._activeSources.Remove(key);
                            
                            // Destroy when the audio clip ends.
                            Destroy(source);
                            complete?.Invoke();
                        }

                        this.StartCoroutine(EndCallback());
                        return source;
                    }

                    return null;
                }


                /**
                 * <summary> Plays the given sound looped. </summary>
                 * <param name = "key"> The key of the sound to play. </param>
                 *
                 * <returns> The audio source of the sound (valid while only while is playing). </returns>
                */
                
                [PublicAPI,  CanBeNull] public AudioSource PlaySoundLooped (string key) {

                    var source = this.PlaySound(key);
                    
                    if (source != null) {

                        source.loop = true;
                        return source;
                    }

                    return null;
                }
                
                
                /**
                 * <summary> Stops the audio with the given key only if it's playing. </summary>
                 * <param name = "key"> The key of the sound to stop. </param>
                 *
                */
                
                [PublicAPI] public void StopAudio ([NotNull] string key) {

                    // If it's actually playing:
                    if (this._activeSources.ContainsKey(key)) {

                        var source = this._activeSources [key];

                        if (source != null) {
                            
                            source.Stop();
                            Destroy(source);
                            
                            this._activeSources.Remove(key);
                        }
                    }
                }
                
            #endregion
        
        #endregion
        
        
        #region NESTED TYPES

            /// <summary> Wraps data used to serialize an Audio File in the inspector. </summary>
            [Serializable] private class AudioFile {

                #region NON-SERIALIZED PROPERTIES

                    /// <summary> The key of the sound object. </summary>
                    [field: SerializeField, Tooltip("The key of the sound object."), Space]
                    public string Key { get; private set; } = string.Empty;
                    
                    /// <summary> The audio clip with the sound. </summary>
                    [field: SerializeField, Tooltip("The audio clip with the sound."), Space]
                    public AudioClip AudioClip { get; private set; } = null;

                #endregion
            }

        #endregion
    }
}
