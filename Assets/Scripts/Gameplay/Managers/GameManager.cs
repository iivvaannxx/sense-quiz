#region USED NAMESPACES

    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using TextSpeech;
    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Definitions;
    using SenseQuiz.Gameplay.Feedback;
    using SenseQuiz.UI;
    using SenseQuiz.Utilities;
    using UnityEngine;
    using Random = UnityEngine.Random;

#endregion


namespace SenseQuiz.Gameplay.Managers {

    /// <summary> Handles generic logic of the game. </summary>
    public class GameManager : MonoBehaviour {

        #region NON-SERIALIZED FIELDS

            /// <summary> Global access instance variable. </summary>
            public static GameManager Instance = null;

            /// <summary> Is the game manager currently accepting input of any kind? </summary>
            private bool _acceptsInput = false;

            /// <summary> The callback to cancel the current question interval. </summary>
            private Action _currentQuestionCancel;
            
        #endregion
        
        
        #region NON-SERIALIZED PROPERTIES
        
            /// <summary> The time to answer the questions of the quiz. </summary>
            public float QuestionTime { get; private set; }
            
            /// <summary> The current question of the quiz. </summary>
            public Question CurrentQuestion { get; private set; }

            
            /// <summary> The current score of the game. </summary>
            public int CurrentScore { get; private set; } = 0;

            /// <summary> Has the quiz been finished? </summary>
            public bool QuizFinished { get; private set; } = false;
            
        #endregion
        

        
        #region UNITY EVENTS

            // Save the instance.
            private void Awake () => GameManager.Instance = this;
            

            private void Update () {

                if (this._acceptsInput) {

                    if (!(this.QuizFinished)) {
                        
                        // If any touch is received, cancel the interval to avoid delayed decisions.
                        if (AndroidTouchManager.ScreenTouchedLastFrame) {
                            
                            AudioManager.Instance.StopAudio("Time Is Running Out");
                            this._currentQuestionCancel?.Invoke();
                        }
                        
                        if (AndroidTouchManager.LastFrameConsecutiveTouches > 0) {

                            // Clamp the count to avoid out of range accesses.
                            var count = Mathf.Clamp(AndroidTouchManager.LastFrameConsecutiveTouches, 1, 4);
                            this._acceptsInput = false;

                            if (this.CurrentQuestion.IsCorrect(count - 1)) {

                                this.CurrentScore++;
                                
                                // Play the sound and go to the next question.
                                AudioManager.Instance.PlaySound("Correct", () => TimeHelpers.SetTimeout(1.0f, this.NextQuestion));
                                AndroidVibrator.Vibrate(200);
                            }

                            else {
                                
                                // Play the sound and go to the next question.
                                AudioManager.Instance.PlaySound("Incorrect", () => TimeHelpers.SetTimeout(1.0f, this.NextQuestion));
                                AndroidVibrator.Vibrate(500);
                            }
                        }
                    }

                    else if (AndroidTouchManager.ScreenTouchedLastFrame) {

                        this._acceptsInput = false;
                        AudioManager.Instance.PlaySound("Confirm");
                        
                        UserInterfaceManager.Instance.FadeOutScreen(1.0f, Color.black, () => {
                            
                            UserInterfaceManager.Instance.MonologueText.FadeOut();
                            UserInterfaceManager.Instance.MonologueSubText.FadeOut();
                            
                            AudioManager.Instance.FadeOut("Background", 1.5f, () => {

                                // Spawn the configurator again.
                                UserInterfaceManager.Instance.FadeInScreen(1.0f, Color.black, Welcome.SpawnConfigurator);
                                
                            }, true);
                        });
                    }
                }

                // Vibrate on touch.
                AndroidTouchManager.EnableTouchVibration = this._acceptsInput;
            }
            
            
            /// <summary> Handle the focus toggle for the voice. </summary>
            private void OnApplicationFocus (bool hasFocus) {

                if (hasFocus)
                    EasySpeech.ResumeSpeak();

                else {

                    TextToSpeech.instance.onDoneCallback = null;
                    TextToSpeech.instance.StopSpeak();
                }
            }


            /// <summary> Remove the voice when the game is closed. </summary>
            private void OnApplicationQuit () {

                TextToSpeech.instance.onDoneCallback = null;
                TextToSpeech.instance.StopSpeak();
            }

        #endregion


        #region METHODS

            #region PUBLIC

                /**
                 * <summary> Starts the quiz game with the given parameters. </summary>
                 *
                 * <param name = "time"> The time the user will have to answer each question. </param>
                 * <param name = "categories"> The categories of the questions. </param>
                 * <param name = "configurator"> The configurator instance which configurated the game. </param>
                */
                
                [PublicAPI] public void StartGame (float time, QuestionCategories categories, GameConfigurator configurator) {
                    
                    // Destroy the configurator instance.
                    Destroy (configurator);
                    this.CurrentScore = 0;
                    this.QuizFinished = false;
                    
                    // Fade out the monologue.
                    UserInterfaceManager.Instance.MonologueText.FadeOut(() => {

                        if (QuestionsDatabase.GetQuestionsOfCategory(categories) is List<Question> questions) {
                            
                            var current = questions.Count;

                            // Shuffle the questions.
                            while (current-- > 1) {  
                                
                                var otherIndex = Random.Range(0, current + 1);  
                                (questions[otherIndex], questions[current]) = (questions[current], questions[otherIndex]);
                            }  
                            
                            QuestionManager.Instance.SetQuestionsForQuiz(questions.Take(10).ToList());
                        }

                        this.QuestionTime = time;
                        this.NextQuestion();
                    });
                }


                /// <summary> Skips to next question of the quiz. </summary>
                private void NextQuestion () {
                    
                    // There are still questions to answer.
                    if (QuestionManager.Instance.Questions.Count > 0) {

                        // The current question.
                        var questionIndex = (10 - QuestionManager.Instance.Questions.Count) + 1;
                        var questionHeader = $"Question {questionIndex} - Remaining Time: {this.QuestionTime}";
                        
                        // Reduce the fade effect duration of the counter. 
                        UserInterfaceManager.Instance.HeaderText.FadeInDuration = 0.25f;
                        UserInterfaceManager.Instance.HeaderText.FadeOutDuration = 0.25f;
                        UserInterfaceManager.Instance.SetHeader(questionHeader);
                        
                        // Speak a random question.
                        this.CurrentQuestion = QuestionManager.Instance.SpeakRandomQuestion(() => {
                            
                            AndroidVibrator.Vibrate(100);
                            
                            this._currentQuestionCancel = TimeHelpers.SetCountdown((uint) this.QuestionTime, (remaining) => {

                                if (remaining <= 5) {

                                    AudioManager.Instance.PlaySoundLooped("Time Is Running Out");
                                    EasySpeech.SpeakWithSettings(remaining.ToString());
                                }

                                else if (remaining < this.QuestionTime)
                                    AudioManager.Instance.PlaySound("Clock Tick");
                                
                                questionHeader = $"Question {questionIndex} - Remaining Time: {remaining}";
                                UserInterfaceManager.Instance.HeaderText.SetTextWithoutFade(questionHeader);
                                
                                this._acceptsInput = true;

                            }, () => {

                                this._acceptsInput = false;
                                questionHeader = $"Question {questionIndex} - Remaining Time: {0}";
                                UserInterfaceManager.Instance.HeaderText.SetTextWithoutFade(questionHeader);

                                // Play the sound and go to the next question.
                                AudioManager.Instance.StopAudio("Time Is Running Out");
                                AudioManager.Instance.PlaySound("Incorrect", () => TimeHelpers.SetTimeout(1.0f, this.NextQuestion));
                                AndroidVibrator.Vibrate(500);
                            });
                        });
                    }

                    else {

                        this._acceptsInput = false;
                        this.FinishQuiz();
                    }
                }


                /// <summary> Triggers the end of the quiz. </summary>
                private void FinishQuiz () {

                    this.QuizFinished = true;
                    
                    // Hide all the interface.
                    UserInterfaceManager.Instance.AnswerTexts.ForEach(current => current.FadeOut());
                    UserInterfaceManager.Instance.HeaderText.FadeOut();
                    
                    UserInterfaceManager.Instance.CentralText.FadeOut(() => {
                        
                        // Test finished.
                        AudioManager.Instance.FadeInLooped("Background", 1.0f, () => {
                            
                            this.WriteAndSpeakMonologue("You have finished the test! Let's see your results!", this.DisplayResults);
                        });
                    });
                }


                /// <summary> Displays the results to the player. </summary>
                private void DisplayResults () {

                    var text = this.CurrentScore switch {

                        var w when (w < 5) => "Sorry, but you haven't passed the test. For sure you will make it better next time!",
                        var x when (x >= 5 && x < 7) => "Congratulations! You passed the test. But you could always do it better!",
                        var y when (y > 7 && y < 9) => "Great! You did it pretty well! Keep working hard!",
                        var z when (z > 9) => "You are amazing! Congratulations on this incredible mark!",
                        
                        _ => throw new ArgumentOutOfRangeException(nameof(this.CurrentScore), this.CurrentScore, "Invalid score!")
                    };

                    var sound = this.CurrentScore < 5 ? "Not Approved" : "Approved";
                    var finalText = $"Your score is {this.CurrentScore} points out of 10. {text}";

                    TimeHelpers.SetTimeout(1.0f, () => AudioManager.Instance.PlaySound(sound, () => {
                        
                        this.WriteAndSpeakMonologue(finalText, () => {

                            // Return to the configuration phase.
                            this.WriteAndSpeakSubMonologue("Thanks for playing! If you want to start again, press anywhere on the screen.", () => {

                                this._acceptsInput = true;
                                AndroidVibrator.Vibrate(100);
                            });
                        });
                    }));
                }


                /**
                 * <summary> Writes the given text to the fading text references. After it has been written speaks it. </summary>
                 * 
                 * <param name = "textRef"> The reference to the text ref were the message will be written. </param>
                 * <param name = "text"> The text to write and speak. </param>
                 * <param name = "done"> Callback fired after the text has been spoken. </param>
                 * <param name = "speakDelay"> The delay between the end of the write and the start of the speak. </param>
                */
                
                [PublicAPI] public void WriteAndSpeak ([NotNull] FadingText textRef, string text, [CanBeNull] Action done = null, float speakDelay = 0.4f) {

                    textRef.SetText(text, () => { TimeHelpers.SetTimeout(speakDelay, 
                        () => { EasySpeech.SpeakWithSettings(text, "en-US", 1f, 1f, done); }); });
                }
                
                
                /**
                 * <summary> Writes the given text to the monologue text. After it has been written speaks it. </summary>
                 * 
                 * <param name = "text"> The text to write and speak. </param>
                 * <param name = "done"> Callback fired after the text has been spoken. </param>
                 * <param name = "speakDelay"> The delay between the end of the write and the start of the speak. </param>
                */
                
                [PublicAPI] public void WriteAndSpeakMonologue (string text, [CanBeNull] Action done = null, float speakDelay = 0.4f) {

                    UserInterfaceManager.Instance.MonologueText.SetText(text, () => { TimeHelpers.SetTimeout(speakDelay, 
                        () => { EasySpeech.SpeakWithSettings(text, "en-US", 1f, 1f, done); }); });
                }
                
                
                /**
                 * <summary> Writes the given text to the monologue sub text. After it has been written speaks it. </summary>
                 * 
                 * <param name = "text"> The text to write and speak. </param>
                 * <param name = "done"> Callback fired after the text has been spoken. </param>
                 * <param name = "speakDelay"> The delay between the end of the write and the start of the speak. </param>
                */
                
                [PublicAPI] public void WriteAndSpeakSubMonologue (string text, [CanBeNull] Action done = null, float speakDelay = 0.4f) {

                    UserInterfaceManager.Instance.MonologueSubText.SetText(text, () => { TimeHelpers.SetTimeout(speakDelay, 
                        () => { EasySpeech.SpeakWithSettings(text, "en-US", 1f, 1f, done); }); });
                }

            #endregion

        #endregion
    }
}
