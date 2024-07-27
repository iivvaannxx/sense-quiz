#region USED NAMESPACES

    using System;
    using System.Collections;
    using JetBrains.Annotations;

    using UnityEngine;

#endregion


namespace SenseQuiz.Utilities {

    /// <summary> Wraps some utility methods related to the time. </summary>
    public static class TimeHelpers {

        #region FIELDS

            /// <summary> The instance of the coroutine runner. </summary>
            private static CoroutineRunner _coroutineRunner;

        #endregion


        #region METHODS

            #region PUBLIC
                
                /**
                 * <summary> Sets a timeout action to run after the given time. </summary>
                 *
                 * <param name = "time"> The time before the action should run. </param>
                 * <param name = "complete"> The callback to execute on the timeout. </param>
                 *
                 * <returns> A callback to cancel the timeout. </returns>
                */
                
                [PublicAPI, NotNull] public static Action SetTimeout (float time, Action complete) {
                    
                    // Flag to cancel the timeout.
                    var canceled = false;

                    // The coroutine enumerator method.
                    IEnumerator Wait () {
                        
                        yield return new WaitForSeconds(time);
                        
                        // If it has not been canceled.
                        if (!(canceled))
                            complete?.Invoke();
                    }

                    // Start the coroutine.
                    var coroutine = TimeHelpers._coroutineRunner.StartCoroutine(Wait());
                    
                    // Return a method to cancel the timeout.
                    return () => {

                        canceled = true;
                        TimeHelpers._coroutineRunner.StopCoroutine(coroutine);
                    };
                }


                /**
                 * <summary> Sets an inteval action to continuously run after the given time. </summary>
                 *
                 * <param name = "time"> The time between each call. </param>
                 * <param name = "complete"> The callback to execute on each tick of the interval. </param>
                 *
                 * <returns> A callback to cancel the interval. </returns>
                */
                
                [PublicAPI, NotNull] public static Action SetInterval (float time, Action tick) {
                    
                    // Flag to cancel the timeout and a reference to the coroutine.
                    var canceled = false;
                    Coroutine coroutine;
                    
                    // The coroutine enumerator method.
                    IEnumerator Wait () {
                        
                        yield return new WaitForSeconds(time);
                        
                        // If it has not been canceled.
                        if (!(canceled)) {
                            
                            // Invoke and start again after end.
                            tick?.Invoke();
                            coroutine = TimeHelpers._coroutineRunner.StartCoroutine(Wait());
                        }
                    }

                    // Start the coroutine.
                    coroutine = TimeHelpers._coroutineRunner.StartCoroutine(Wait());
                    
                    // Return a method to cancel the timeout.
                    return () => {

                        canceled = true;
                        
                        if (coroutine != null)
                            TimeHelpers._coroutineRunner.StopCoroutine(coroutine);
                    };
                }
                
                
                /**
                 * <summary> Sets a countdown with the given amount of seconds. </summary>
                 *
                 * <param name = "seconds"> The duration of the countdown. </param>
                 * <param name = "tick"> On each second tick, this callback gets fired and receives the remaining time. </param>
                 * <param name = "complete"> When the countdown ends, this callback gets fired. </param>
                 *
                 * <returns> A callback to cancel the countdown. </returns>
                */
                
                [PublicAPI, NotNull] public static Action SetCountdown (uint seconds, [CanBeNull] Action <uint> tick = null, [CanBeNull] Action complete = null) {

                    // Setup the countdown variables. 
                    var initialCountdown = seconds + 1;
                    var canceled = false;

                    // Create a second by second interval.
                    Action cancelInterval = null;
                    
                    // The callback executed on each tick.
                    void Callback () {
                        
                        if (!(canceled)) {

                            // The callback ended.
                            if (--initialCountdown == 0) {
                                
                                complete?.Invoke();
                                cancelInterval?.Invoke();
                            }
                            
                            else
                                tick?.Invoke(initialCountdown);
                        }
                    }
                    
                    Callback();
                    cancelInterval = SetInterval(1.0f, Callback);

                    // Cancels the interval.
                    return () => {

                        canceled = true;
                        cancelInterval?.Invoke();
                    };
                }

            #endregion


            #region PRIVATE

                /// <summary> Lazily initialize the class and create the runner instance. </summary>
                [RuntimeInitializeOnLoadMethod] private static void Initialize () {

                    // Add the component and grab the instance.
                    var runner = new GameObject("Coroutine Runner");
                    TimeHelpers._coroutineRunner = runner.AddComponent<CoroutineRunner>();
                }

            #endregion

        #endregion


        #region NESTED TYPES

            /// <summary> Internal class which only runs coroutines. </summary>
            private sealed class CoroutineRunner : MonoBehaviour { }

        #endregion
    }
}
