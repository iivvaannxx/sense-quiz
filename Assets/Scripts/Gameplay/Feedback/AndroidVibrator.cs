#region USED NAMESPACES

    using System;

    using UnityEngine;
    using JetBrains.Annotations;
    using SenseQuiz.Utilities;

#endregion


namespace SenseQuiz.Gameplay.Feedback {

    /// <summary> Exposes simple methods to easily vibrate an Android device. </summary>
    public static class AndroidVibrator {

        #region FIELDS

            /// <summary> The reference to the vibrator system service. </summary>
            private static AndroidJavaObject _vibrator;

        #endregion


        #region METHODS

            #region PUBLIC

                /**
                 * <summary> Vibrates the Android device for the given amount of time. </summary>
                 * <param name = "milliseconds"> The amount of time the device should vibrate. </param>
                */
                
                [PublicAPI] public static void Vibrate (long milliseconds = 250) {
                    
                    // Turn on the vibrator for the specified amount of time.
                    if (Application.platform == RuntimePlatform.Android)
                        AndroidVibrator._vibrator.Call("vibrate", milliseconds);
                }


                /// <summary> Cancels the current vibration. </summary>
                [PublicAPI] public static void Cancel () {

                    if (Application.platform == RuntimePlatform.Android)
                        AndroidVibrator._vibrator.Call("cancel");
                }


                /**
                 * <summary> Vibrates the device at a delayed interval with the given duration and the given amount of times. </summary>
                 *
                 * <param name = "intervalMilliseconds"> The delay between calls. </param>
                 * <param name = "vibrationDuration"> The duration of the vibration (should be smaller than the interval time). </param>
                 * <param name = "count"> The amount of times to vibrate. -1 Means vibrate infinitely until cancelled. </param>
                 *
                 * <returns> A callback which allows to cancel the interval. </returns>
                */
                
                [PublicAPI, NotNull] public static Action IntervalVibrate (long intervalMilliseconds, long vibrationDuration, int count = -1) {

                    // Create an interval to do the vibration and the callback to cancel it.
                    Action cancelInterval = null;
                    cancelInterval = TimeHelpers.SetInterval(intervalMilliseconds / 1000f, () => {
                        
                        // Start the vibration.
                        AndroidVibrator.Vibrate(vibrationDuration);

                        // If different from infinite:
                        if (count != -1) {
                            
                            // No more vibrations.
                            if (--count == 0)  
                                cancelInterval?.Invoke();
                        }
                    });

                    return cancelInterval;
                }

            #endregion
            

            #region PRIVATE

                /// <summary> Initializes the vibrator class. </summary>
                [RuntimeInitializeOnLoadMethod] private static void Initialize () {

                    if (Application.platform == RuntimePlatform.Android) {

                        // The reference to the UnityPlayer class inside an Android device.
                        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        // Find the vibrator system service.
                        AndroidVibrator._vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    }
                }

            #endregion

        #endregion
    }
}
