#region USED NAMESPACES

    using System;
    using System.Collections;
    using System.Collections.Generic;
    
    using JetBrains.Annotations;
    
    using UnityEngine;
    using Newtonsoft.Json;
    using SenseQuiz.Gameplay.Definitions;
    using SenseQuiz.UI;
    using SenseQuiz.Utilities;
    using Random = UnityEngine.Random;

#endregion



namespace SenseQuiz.Gameplay.Managers {

    /// <summary> Handles everything related to the questions. </summary>
    public class QuestionManager : MonoBehaviour {

        #region NON-SERIALIZED FIELDS

            /// <summary> Global access instance variable. </summary>
            public static QuestionManager Instance = null;

        #endregion
        
        
        #region SERIALIZED PROPERTIES

            /// <summary> A list with all the files with questions. </summary>
            [field: SerializeField, Tooltip("A list with all the files with questions."), Space] [PublicAPI]
            private List <QuestionManager.QuestionFile> _files;
            
            /// <summary> The voice which speaks the questions. </summary>
            [field: SerializeField, Tooltip("The voice which speaks the questions."), Space] [PublicAPI]
            public VoiceSettings VoiceSettings { get; set; }

        #endregion
        
        
        #region NON-SERIALIZED PROPERTIES
        
            /// <summary> The questions of the current game session. </summary>
            public List <Question> Questions { get; private set; } = null;

        #endregion


        #region UNITY EVENTS

            private void Start () {

                QuestionManager.Instance = this;
                this.ReadQuestions();
            }
            
        #endregion
        
        
        #region METHODS
            
            #region PUBLIC

                /**
                 * <summary> Sets the questions to be spoken in the current quiz session. </summary>
                 * <param name = "questions"> The questions of the current quiz. </param>
                */
                
                [PublicAPI] public void SetQuestionsForQuiz (List <Question> questions) => this.Questions = questions;
            
            
                /**
                 * <summary> Speaks the given question and it's answers. </summary>
                 *
                 * <param name = "done"> Callback fired when the question has been spoken. </param>
                 * <param name = "speakAnswers"> Should the answers be spoken? </param>
                 * <param name = "remove"> Should the question be removed from the current quiz session? </param>
                */
                
                [PublicAPI] public Question SpeakRandomQuestion ([CanBeNull] Action done = null, bool speakAnswers = true, bool remove = true) {
                    
                    if (this.Questions.Count > 0) {
                        
                        // Select the question to speak.
                        var randomIndex = Random.Range(0, this.Questions.Count);
                        var question = this.Questions [randomIndex];
                        
                        QuestionManager.PrepareQuestion(question, () => question?.SpeakStatement(this.VoiceSettings.Rate, this.VoiceSettings.Pitch, () => {
                            
                            // Remove the selected question.
                            if (remove) 
                                this.Questions.Remove(question);
                            
                            if (speakAnswers)
                                this.SpeakAllQuestionAnswers(question, done);
                            
                            else
                                done?.Invoke();
                        }));

                        return question;
                    }

                    return null;
                }
                

                /**
                 * <summary> Speaks all the answers of a questions. </summary>
                 *
                 * <param name = "question"> The question to be spoken. </param>
                 * <param name = "done"> Callback fired when all the answers have been read. </param>
                */
                
                private void SpeakAllQuestionAnswers ([NotNull] Question question, [CanBeNull] Action done = null) {
                    
                    var currentFinished = false;
                    var currentIndex = 0;

                    IEnumerator AnswerCoroutine () {

                        // Speak the current answer index.
                        var isLastQuestion = currentIndex == question.PossibleAnswers.Count - 1;
                        question.SpeakAnswer(currentIndex, this.VoiceSettings.Rate, this.VoiceSettings.Pitch, () => {
                            
                            // Invoke the done callback.
                            if (isLastQuestion)
                                done?.Invoke();
                            
                            currentFinished = true;
                        });
                        
                        // Wait until finished.
                        yield return new WaitUntil(() => currentFinished);
                        
                        // Go to the next question and speak it.
                        if (++currentIndex < question.PossibleAnswers.Count) {

                            currentFinished = false;
                            this.StartCoroutine(AnswerCoroutine());
                        }
                    }

                    this.StartCoroutine(AnswerCoroutine());
                }


                /**
                 * <summary> Sets the current language of the question speaker. </summary>
                 * <param name = "languageIndex"> The index of the language to set in the <see cref = "VoiceLanguage"/> enum. </param>
                */
                
                [PublicAPI] public void SetCurrentLanguage (int languageIndex) 
                    => this.VoiceSettings.CurrentLanguage = (VoiceLanguage) languageIndex;
            
            #endregion
        
        
            #region PRIVATE 
                
                /// <summary> Reads all the questions of all the provided files. </summary>
                private void ReadQuestions () {
                    
                    this._files.ForEach((current) => {

                        var data = JsonConvert.DeserializeObject<QuestionManager.QuestionFileWrapper>(current.QuestionsJson.text);
                        var language = LocaleHelpers.GetLanguageFromPrefix(current.Language);
                        
                        // Iterate all the categories.
                        foreach (var category in data.Categories) {

                            // Then all the questions.
                            foreach (var question in category.Questions) {

                                question.Language = language;
                                QuestionsDatabase.RegisterQuestion(question, language, category.Category);
                            }
                        }
                    });
                }
                
                
                /**
                 * <summary> Prepares a question to be read. </summary>
                 *
                 * <param name = "question"> The question to prepare on the screen. </param>
                 * <param name = "prepared"> Callback fired after the question has been prepared. </param>
                */
                
                private static void PrepareQuestion ([CanBeNull] Question question, Action prepared) {
                    
                    // Set the answers.
                    if (question?.PossibleAnswers != null) {
                        
                        var startAnswerLetter = (char) ('A' - 1);
                        
                        foreach (var answer in question.PossibleAnswers) {

                            var answerText = $"{++startAnswerLetter}) {answer}";
                            var currentIndex = startAnswerLetter - 'A';

                            UserInterfaceManager.Instance.AnswerTexts[currentIndex].Text = answerText;
                        }
                    }

                    // Set the current text and then read.
                    UserInterfaceManager.Instance.CentralText.SetText(question?.Statement, prepared);
                }
            
            #endregion
        
        #endregion
        
        
        #region NESTED TYPES

            /// <summary> Wraps data used to serialize a Question File in the inspector. </summary>
            [Serializable] private class QuestionFile {

                #region NON-SERIALIZED PROPERTIES

                    /// <summary> The language of the question file (also the key). </summary>
                    [field: SerializeField, Tooltip("The language of the question file (also the key)."), Space]
                    public string Language { get; private set; } = string.Empty;

                    /// <summary> The JSON file with all the questions. </summary>
                    [field: SerializeField, Tooltip("The JSON file with all the questions."), Space]
                    public TextAsset QuestionsJson { get; private set; } = null;

                #endregion
            }


            /// <summary> Wraps the global questions JSON structure into a single type. </summary>
            private sealed class QuestionFileWrapper {

                #region CONSTRUCTORS AND FINALIZERS

                    /**
                     * <summary> Constructs a new instance of the file wrapper with the given data. </summary>
                     * <param name = "categories"> The categories with all the questions. </param>
                    */
                    
                    [JsonConstructor] public QuestionFileWrapper (CategoryQuestions [ ] categories) {
                        
                        this.Categories = categories;
                    }

                #endregion


                #region PROPERTIES AND INDEXERS

                    /// <summary> Contains </summary>
                    [PublicAPI] public CategoryQuestions [ ] Categories { get; set; }

                #endregion
            }

        #endregion
    }
}
