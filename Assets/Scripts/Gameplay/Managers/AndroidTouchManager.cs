#region USED NAMESPACES

    using System;
    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Feedback;
    using UnityEngine;
    using UnityEngine.InputSystem;

#endregion


namespace SenseQuiz.Gameplay.Managers {

    /// <summary> Exposes some events to easily handle touches in an Android device. </summary>
    public static class AndroidTouchManager {

        #region DELEGATES AND EVENTS

            /// <summary> Event fired when the screen gets touched multiple times. Receives the number of times. </summary>
            public static event Action <int> OnScreenMultipleTouch;

            /// <summary> Event fired when the screen gets touched. </summary>
            public static event Action OnScreenTouch;

        #endregion


        #region PROPERTIES AND INDEXERS

            /// <summary> Should the screen vibrate when it receives a touch. </summary>
            [PublicAPI] public static bool EnableTouchVibration { get; set; } = true;

            /// <summary> Tells if the screen was touched the last frame. </summary>
            [PublicAPI] public static bool ScreenTouchedLastFrame = false;
            
            /// <summary> The amount of consecutive touches which were registered the last frame. </summary>
            [PublicAPI] public static int LastFrameConsecutiveTouches { get; set; } = 0;
            
            /// <summary> The duration in milliseconds for the touch vibration duration. </summary>
            [PublicAPI] public static long TouchVibrationDuration { get; set; } = 50;


        #endregion


        #region METHODS

            #region PRIVATE

                /// <summary> Initialize the touch manager class. </summary>
                [RuntimeInitializeOnLoadMethod] private static void Initialize () {

                    // Only avaiable for Android platforms.
                    if (Application.platform == RuntimePlatform.Android) {

                        var listener = new GameObject("Touch Listener");
                        listener.AddComponent<TouchListener>();
                    }
                }

            #endregion

        #endregion


        #region NESTED TYPES

            /// <summary> Internal mono-behaviour which handles the touch check. </summary>
            [DefaultExecutionOrder(int.MaxValue)] private sealed class TouchListener : MonoBehaviour {

                #region NON-SERIALIZED FIELDS

                    /// <summary> Holds the current tap count of the primary touch. Used to handle the state of the touches. </summary>
                    private int _tapCount;

                #endregion


                #region UNITY EVENTS

                    private void Update () {

                        // Reset the touch flag.
                        if (ScreenTouchedLastFrame)
                            ScreenTouchedLastFrame = false;

                        // Reset the tap count registry.
                        if (LastFrameConsecutiveTouches > 0)
                            LastFrameConsecutiveTouches = 0;
                        
                        // Shorthand for the touch variable.
                        var touch = Touchscreen.current?.primaryTouch;

                        // If it's valid.
                        if (touch != null) {

                            var tapCount = touch.tapCount.ReadValue();
                            
                            if (this._tapCount != tapCount) {
                                
                                // The count got reset.
                                if (tapCount == 0) {

                                    LastFrameConsecutiveTouches = this._tapCount;
                                    OnScreenMultipleTouch?.Invoke(this._tapCount);
                                }
                                    
                                // The screen was pressed.
                                if (tapCount > 0) {

                                    // Vibrate for the given amount of milliseconds.
                                    if (EnableTouchVibration)
                                        AndroidVibrator.Vibrate(TouchVibrationDuration);    
                                    
                                    OnScreenTouch?.Invoke();
                                    ScreenTouchedLastFrame = true;
                                }
                                    
                                // Save the current tap count.
                                this._tapCount = tapCount;
                            }
                        }
                    }

                #endregion
            }

        #endregion
    }
}
