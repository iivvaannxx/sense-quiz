#region USED NAMESPACES

    using System;
    using SenseQuiz.Gameplay.Definitions;
    using SenseQuiz.Gameplay.Feedback;
    using SenseQuiz.Gameplay.Managers;
    using SenseQuiz.UI;
    using UnityEngine;

#endregion


namespace SenseQuiz.Gameplay {

    /// <summary> The class which handles the setup of the game. </summary>
    public class Welcome : MonoBehaviour {

        #region NON-SERIALIZED FIELDS

            /// <summary> The welcome text of the game. </summary>
            private static readonly Monologue _welcomeText = new Monologue (
                
                new Dialog ("\n\nWELCOME TO SENSE QUIZ. A VIDEOGAME MADE TO IMPROVE YOUR GENERAL KNOWLEDGE."),
                new Dialog ("YOU WILL HAVE TO ANSWER TEN QUESTIONS OF THE SELECTED CATEGORY, AND A SCORE WILL BE CALCULATED BASED ON THE PERCENTAGE OF CORRECT ANSWERS.", 1.4f),
                new Dialog ("CONTROLS ARE PRETTY BASIC. WHEN YOU HAVE TO CHOOSE SOMETHING, A NUMBER IS INDICATED BEFORE, SO YOU JUST HAVE TO TOUCH ANYWHERE IN THE SCREEN AS MANY TIMES AS THE NUMBER INDICATES."),
                new Dialog ("FOR EXAMPLE, IF YOU HAVE 4 AVAILABLE ANSWERS, AND YOU WANT TO CHOOSE THE LAST ONE, YOU WILL HAVE TO TOUCH THE SCREEN 4 TIMES.", 1.4f),
                new Dialog ("NOW THAT WE KNOW HOW IT WORKS, PRESS ANYWHERE ON THE SCREEN TO START THE GAME. \n\nGOOD LUCK!", 0f)
            );


            /// <summary> Is the setup currently accepting input of any kind? </summary>
            private bool _acceptsInput = false;
            
            /// <summary> The current callback fired when a single touch is received. </summary>
            private Action _currentSingleTouchCallback;
            
        #endregion

        
        #region UNITY EVENTS

            private void Start () {
                
                this.OnGameStart();
            }
            
            
            private void Update () {

                if (this._acceptsInput) {
                    
                    // Invoke the current single touch event.
                    if (AndroidTouchManager.ScreenTouchedLastFrame)
                        this._currentSingleTouchCallback?.Invoke();
                }

                AndroidTouchManager.EnableTouchVibration = this._acceptsInput;
            }
            
        #endregion


        #region METHODS

            #region PUBLIC
        
                /// <summary> Spawns the game configurator which configures a game session. </summary>
                public static void SpawnConfigurator () {

                    var configurator = new GameObject("Game Configurator");
                    configurator.AddComponent<GameConfigurator>();
                }
                
            #endregion
            
            
            #region PRIVATE
            

                /// <summary> Fired as soon as the game starts. </summary>
                private void OnGameStart () {

                    Welcome.FadeInTitle(() => {
                        this._acceptsInput = true;
                        AndroidVibrator.Vibrate(100);
                        
                        this._currentSingleTouchCallback = () => {

                            this._acceptsInput = false;
                            AudioManager.Instance.PlaySound("Start");
                            AndroidTouchManager.TouchVibrationDuration = 200;
                            
                            UserInterfaceManager.Instance.MonologueSubText.FadeOut();
                            UserInterfaceManager.Instance.MonologueText.SetText(Welcome._welcomeText.FullText, this.WelcomePlayer);
                        };
                    });
                }


                /// <summary> Welcomes the player by speaking the welcome monologue. </summary>
                private void WelcomePlayer () { 
                    
                    // Speak the initial monologue.
                    EasySpeech.SpeakMonologue(Welcome._welcomeText, VoiceLanguage.AmericanEnglish, 1f, 1f, () => {

                        this._acceptsInput = true;
                        AndroidVibrator.Vibrate(100);
                        AndroidTouchManager.TouchVibrationDuration = 50;

                        this._currentSingleTouchCallback = () => {

                            this._acceptsInput = false;
                            AudioManager.Instance.PlaySound("Confirm");
                            
                            UserInterfaceManager.Instance.MonologueText.FadeOut(Welcome.SpawnConfigurator);
                        };
                    });
                }


                /**
                 * <summary> Fades in the title screen. </summary>
                 * <param name = "complete"> Callback fired when the fade completes. </param>
                */
                
                private static void FadeInTitle (Action complete) { 
                    
                    // Fade in the menu screen.
                    UserInterfaceManager.Instance.FadeInScreen(1.0f, Color.black, () => {

                        // Fade the Game Title in.
                        UserInterfaceManager.Instance.MonologueText.FadeIn(() => {
                            
                            // Present the game and tell how to continue.
                            EasySpeech.SpeakWithSettings("¡SENSE QUIZ!", "en-US", 1.0f, 1.0f, () => {
                                
                                UserInterfaceManager.Instance.MonologueSubText.FadeIn(
                                    () => { EasySpeech.SpeakWithSettings("Press anywhere to start.", "en-US", 1f, 1f, complete); });
                            });
                        });
                    });
                }

            #endregion

        #endregion
    }
}
