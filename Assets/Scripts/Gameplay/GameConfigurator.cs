#region USED NAMESPACES

    using System;
    using System.Collections.Generic;
    
    using SenseQuiz.Gameplay.Definitions;
    using SenseQuiz.Gameplay.Feedback;
    using SenseQuiz.Gameplay.Managers;
    
    using SenseQuiz.UI;
    using SenseQuiz.Utilities;
    
    using UnityEngine;

#endregion


namespace SenseQuiz.Gameplay {

    /// <summary> Handles the configuration of the game. </summary>
    public class GameConfigurator : MonoBehaviour {

        #region NON-SERIALIZED FIELDS

            /// <summary> The monologue displayed to select the quiz category. </summary>
            private static readonly Monologue _categoryMonologue = new Monologue(
                
                new Dialog ("\n\n¡Lets configure our game!", 1f),
                new Dialog ("Please, select one of the following categories.\n", 1.4f),
                
                new Dialog ("1. Art.", 0.8f),
                new Dialog ("2. Mixed.", 0.8f),
                new Dialog ("3. Sports.", 0.8f),
                new Dialog ("4. History.", 0.8f),
                new Dialog ("5. Science.", 0.8f),
                new Dialog ("6. Geography.", 0.8f),
                new Dialog ("7. Entertainment.", 0.8f)
            );
            
            
            /// <summary> The monologue displayed to select the quiz question time. </summary>
            private static readonly Monologue _timeMonologue = new Monologue(
                
                new Dialog ("\n\nNow, you will choose how many time you will have to answer each question.\n", 1.4f),
                
                new Dialog ("1. 5 Seconds.", 0.8f),
                new Dialog ("2. 10 Seconds.", 0.8f),
                new Dialog ("3. 20 Seconds.", 0.8f),
                new Dialog ("4. 30 Seconds.", 0.8f)
            );
            
            
            /// <summary> Maps the necessary number of touches needed to select a category to it's respective category. </summary>
            private static readonly Dictionary <int, QuestionCategories> _categories = new Dictionary<int, QuestionCategories> {
                
                { 0, QuestionCategories.None      },  { 1, QuestionCategories.Art },     
                { 2, QuestionCategories.Mixed     },  { 3, QuestionCategories.Sports },  
                { 4, QuestionCategories.History   },  { 5, QuestionCategories.Science }, 
                { 6, QuestionCategories.Geography },  { 7, QuestionCategories.Entertainment }
            };


            /// <summary> Is the configurator currently accepting input of any kind? </summary>
            private bool _acceptsInput = false;
            
            /// <summary> The current callback fired when multiple touches are received. </summary>
            private Action <int> _currentMultipleTouchCallback;


            /// <summary> The selected time between questions. </summary>
            private float _questionTime = 10f;
                
            /// <summary> The selected categories for the game. </summary>
            private QuestionCategories _selectedCategories = QuestionCategories.Mixed;
            
        #endregion


        #region UNITY EVENTS

            private void Start () {

                TimeHelpers.SetTimeout(1f, () => {
                    AudioManager.Instance.FadeInLooped("Background", 1.5f, this.ChooseCategory);
                });
            }


            private void Update () {

                if (this._acceptsInput) {
                    
                    if (AndroidTouchManager.LastFrameConsecutiveTouches > 0)
                        this._currentMultipleTouchCallback?.Invoke(AndroidTouchManager.LastFrameConsecutiveTouches);
                }
                
                AndroidTouchManager.EnableTouchVibration = this._acceptsInput;
            }

        #endregion


        #region METHODS

            #region PRIVATE

                /// <summary> Handles the configuration of the category. </summary>
                private void ChooseCategory () {

                    UserInterfaceManager.Instance.MonologueText.SetTextWithoutFade(GameConfigurator._categoryMonologue.FullText);
                    UserInterfaceManager.Instance.MonologueText.FadeIn(() => {
                        
                        // Speak the selection monologue.
                        EasySpeech.SpeakMonologue(GameConfigurator._categoryMonologue, VoiceLanguage.BritishEnglish, 1f, 1f, () => {

                            this._acceptsInput = true;
                            AndroidVibrator.Vibrate(100);
                            
                            this._currentMultipleTouchCallback = (count) => {

                                AudioManager.Instance.PlaySound("Confirm");
                                this._acceptsInput = false;
                                
                                var index = Mathf.Clamp(count, 0, GameConfigurator._categories.Count - 1);
                                var selectedCategory = GameConfigurator._categories [index];
                                this.OnCategorySelected(selectedCategory);
                            };
                        });
                    });
                }
                
                
                /// <summary> Handles the configuration of the time. </summary>
                private void ChooseTime () {
                    
                    GameManager.Instance.WriteAndSpeakMonologue(GameConfigurator._timeMonologue.FullText, () => {

                        this._acceptsInput = true;
                        AndroidVibrator.Vibrate(100);

                        this._currentMultipleTouchCallback = (count) => {

                            AudioManager.Instance.PlaySound("Confirm");

                            this._acceptsInput = false;
                            var index = Mathf.Clamp(count, 1, 4);
                            
                            var selectedTime = index switch {

                                1 => 5f,
                                2 => 10f,
                                3 => 20f,
                                4 => 30f,
                                
                                // Should never get here.
                                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index too high.")
                            };

                            this.OnTimeSelected(selectedTime);
                        };
                    });
                }
                

                /**
                 * <summary> Forces the user to confirm the selection of the category. </summary>
                 * <param name = "selectedCategory"> The selected category on the previous phase. </param>
                */

                private void OnCategorySelected (QuestionCategories selectedCategory) {

                    // The two texts to read after the selection.
                    var selectedCategoryText = $"Your quiz will consist of {selectedCategory.ToString()} questions.";
                    var confirmText = "Are you sure? Press one time to confirm or press two times to choose again.";

                    GameManager.Instance.WriteAndSpeakMonologue(selectedCategoryText, () => {
                        
                        UserInterfaceManager.Instance.MonologueSubText.SetTextWithoutFade(confirmText);
                        GameManager.Instance.WriteAndSpeakSubMonologue(confirmText, () => {

                            this._acceptsInput = true;
                            AndroidVibrator.Vibrate(100);
                            
                            // Handle the selection confirm.
                            this._currentMultipleTouchCallback = (count) => {
                                
                                if (count == 1) {

                                    this._acceptsInput = false;
                                    this._selectedCategories = selectedCategory;

                                    AudioManager.Instance.PlaySound("Confirm");
                                    UserInterfaceManager.Instance.MonologueSubText.FadeOut(this.ChooseTime);
                                }

                                else {

                                    this._acceptsInput = false;
                                    AudioManager.Instance.PlaySound("Cancel");

                                    UserInterfaceManager.Instance.MonologueText.FadeOut();
                                    UserInterfaceManager.Instance.MonologueSubText.FadeOut(this.ChooseCategory);
                                }
                            };
                        });
                    });
                }
                
                /**
                 * <summary> Forces the user to confirm the selection of the time. </summary>
                 * <param name = "selectedTime"> The selected time on the previous phase. </param>
                */
                
                private void OnTimeSelected (float selectedTime) {
                    
                    // The two texts to read after the selection.
                    var selectedCategoryText = $"You will have {selectedTime} seconds to answer each question.";
                    var confirmText = "Are you sure? Press one time to confirm or press two times to choose again.";

                    GameManager.Instance.WriteAndSpeakMonologue(selectedCategoryText, () => {
                        
                        UserInterfaceManager.Instance.MonologueSubText.SetTextWithoutFade(confirmText);
                        GameManager.Instance.WriteAndSpeakSubMonologue(confirmText, () => {

                            this._acceptsInput = true;
                            AndroidVibrator.Vibrate(100);

                            // Handle the selection confirm.
                            this._currentMultipleTouchCallback = (count) => {

                                
                                if (count == 1) {

                                    this._acceptsInput = false;
                                    this._questionTime = selectedTime;
                                    
                                    AudioManager.Instance.PlaySound("Confirm");
                                    this.StartGame();
                                }

                                else {

                                    this._acceptsInput = false;
                                    AudioManager.Instance.PlaySound("Cancel");

                                    UserInterfaceManager.Instance.MonologueText.FadeOut();
                                    UserInterfaceManager.Instance.MonologueSubText.FadeOut(this.ChooseTime);
                                }
                            };
                        });
                    });
                }


                /// <summary> Ends the configuration process and starts the game. </summary>
                private void StartGame () {
                 
                    UserInterfaceManager.Instance.MonologueSubText.FadeOut();
                    GameManager.Instance.WriteAndSpeakMonologue("Configuration is done! The quiz will start in...", () => {

                        UserInterfaceManager.Instance.MonologueText.FadeInDuration = 0.3f;
                        UserInterfaceManager.Instance.MonologueText.FadeOutDuration = 0.3f;
                        
                        AudioManager.Instance.FadeOut("Background", 1.0f, () => {
                            
                            // Set a 3 second countdown.
                            TimeHelpers.SetCountdown(3, (remaining) => { GameManager.Instance.WriteAndSpeakMonologue(remaining.ToString()); }, () => {
                                    
                                GameManager.Instance.WriteAndSpeakMonologue("¡Now!", () => {
                                    
                                    UserInterfaceManager.Instance.MonologueText.FadeInDuration = 1f;
                                    UserInterfaceManager.Instance.MonologueText.FadeOutDuration = 1f;
                                    GameManager.Instance.StartGame(this._questionTime, this._selectedCategories, this);
                                    
                                }, 0.5f);
                            });
                        });
                        
                    }, 0f);
                }

            #endregion

        #endregion
    }
}
