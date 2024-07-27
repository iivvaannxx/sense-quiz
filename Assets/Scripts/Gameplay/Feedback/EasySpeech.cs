#region USED NAMESPACES

    using System;
    using TextSpeech;
    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Definitions;
    using SenseQuiz.Utilities;
using UnityEngine;

#endregion


namespace SenseQuiz.Gameplay.Feedback {

    /// <summary> Wraps some utility methods to easily speak messages. </summary>
    [PublicAPI] public static class EasySpeech {

        #region FIELDS
        
            /// <summary> The current callback of the voice. </summary>
            private static Action _currentCallback = null;

            /// <summary> The current pitch used by the voice. </summary>
            private static float _currentPitch = 1.0f;

            /// <summary> The current rate used by the voice. </summary>
            private static float _currentRate = 1.0f;

            /// <summary> The current local spoke by the voice. </summary>
            private static string _currentLocale = "en-US";

            /// <summary> The current message spoke by the voice. </summary>
            private static string _currentMessage = string.Empty;

        #endregion


        #region METHODS

            #region PUBLIC

                /// <summary> Resumes the last message being spoken by the voice. </summary>
                public static void ResumeSpeak () {
                    
                    // Resume the last message.
                    if (EasySpeech._currentMessage != string.Empty) {

                        EasySpeech.SpeakWithSettings(EasySpeech._currentMessage, EasySpeech._currentLocale,
                            EasySpeech._currentRate, EasySpeech._currentPitch, EasySpeech._currentCallback);
                    }
                }


                /**
                 * <summary> Speaks the given monologue with the given rate and pitch. </summary>
                 * 
                 * <param name = "monologue"> The monologue to speak. </param>
                 * <param name = "locale"> The locale of the voice. </param>
                 * <param name = "rate"> The rate of the voice. </param>
                 * <param name = "pitch"> The pitch of the voice. </param>
                 * <param name = "done"> Callback fired after the text has been speaked. </param>
                */
                
                public static Action SpeakMonologue ([NotNull] Monologue monologue, VoiceLanguage language, float rate = 1.0f, float pitch = 1.0f, [CanBeNull] Action done = null) {

                    var locale = LocaleHelpers.GetPrefixFromLanguage(language);
                    return monologue.Speak(locale, rate, pitch, done);
                }


                /**
                 * <summary> Speaks the given message with the given rate and pitch. </summary>
                 * 
                 * <param name = "message"> The message to speak. </param>
                 * <param name = "locale"> The locale of the voice. </param>
                 * <param name = "rate"> The rate of the voice. </param>
                 * <param name = "pitch"> The pitch of the voice. </param>
                 * <param name = "done"> Callback fired after the text has been speaked. </param>
                */
                
                public static void SpeakWithSettings (string message, string locale = "en-US", float rate = 1.0f,
                    float pitch = 1.0f, [CanBeNull] Action done = null) {

                        if (Application.isEditor) {
                            done?.Invoke();
                            return;
                        }

                        // Save the current settings.
                        var previousPitch = TextToSpeech.instance.pitch;
                        var previousRate = TextToSpeech.instance.rate;

                        EasySpeech._currentMessage = message;
                        EasySpeech._currentLocale = locale;
                        EasySpeech._currentRate = rate;
                        EasySpeech._currentPitch = pitch;
                        EasySpeech._currentCallback = done;

                        // Set the done callback.
                        void OnDone () {

                            TextToSpeech.instance.pitch = previousPitch;
                            TextToSpeech.instance.rate = previousRate;

                            // Erase all the settings.
                            EasySpeech._currentMessage = string.Empty;
                            EasySpeech._currentLocale = "en-US";
                            EasySpeech._currentRate = 1.0f;
                            EasySpeech._currentPitch = 1.0f;
                            EasySpeech._currentCallback = null;
                                
                            done?.Invoke();
                            TextToSpeech.instance.onDoneCallback -= OnDone;
                        }

                        // Override with the given settings.
                        TextToSpeech.instance.Setting(locale, pitch, rate);
                        TextToSpeech.instance.pitch = pitch;
                        TextToSpeech.instance.rate = rate;

                        // Speak and set the callback.
                        TextToSpeech.instance.onDoneCallback += OnDone;
                        TextToSpeech.instance.StartSpeak(message);
                    }

            #endregion

        #endregion
    }
}
