#region USED NAMESPACES

    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using SenseQuiz.Gameplay.Feedback;
    using SenseQuiz.Utilities;

#endregion


namespace SenseQuiz.Gameplay.Definitions {

    /// <summary> Represents a question which a defined statement and a correct answer. </summary>
    public class Question {

        #region FIELDS

            /// <summary> The correct answer for this question. </summary>
            private int _correct;

            /// <summary> Contains all the possible answers for this question. </summary>
            private string [ ] _answers = { };

        #endregion


        #region CONSTRUCTORS AND FINALIZERS

            /**
             * <summary> Constructs a new question with the given data. </summary>
             *
             * <param name = "statement">The statement of the question. </param>
             * <param name = "correct">The index of the correct answer. </param>
             * <param name = "answers"> A list with all the possible answers. </param>
            */
            
            [JsonConstructor] public Question (string statement, int correct, string [ ] answers) {

                this.Statement = statement;
                this._correct = correct;
                this._answers = answers;
            }

        #endregion


        #region PROPERTIES AND INDEXERS

            /// <summary> An immutable list which contains a copy of all the possible answers for this question. </summary>
            [PublicAPI] public IReadOnlyList<string> PossibleAnswers => this._answers;

            /// <summary> The statement of the question (the actual question). </summary>
            [PublicAPI] public string Statement { get; private set; }

            /// <summary> The language of the question. </summary>
            [PublicAPI] public VoiceLanguage Language { get; set; } = VoiceLanguage.Catalan;

        #endregion


        #region METHODS

            #region PUBLIC

                /**
                 * <summary> Tells if the answer at the given index is the correct one. </summary>
                 *
                 * <param name = "index"> The index of the answer. </param>
                 * <returns> True if the given answer is the correct one, False otherwise. </returns>
                */
                
                [PublicAPI] public bool IsCorrect (int index) => this._correct == index;
                
                
                /**
                 * <summary> Tells if the given answer is the correct one. </summary>
                 *
                 * <param name = "answer"> The answer to check if is correct or not. </param>
                 * <returns> True if the given answer is the correct one, False otherwise. </returns>
                */
                
                [PublicAPI] public bool IsCorrect (string answer) => this._answers [this._correct] == answer;

            
                /**
                 * <summary> Speaks the question statement with the given rate and pitch. </summary>
                 *
                 * <param name = "rate"> The rate of the voice. </param>
                 * <param name = "pitch"> The pitch of the voice. </param>
                 * <param name = "done"> Callback fired after the text has been speaked. </param>
                */

                [PublicAPI] public void SpeakStatement (float rate = 1.0f, float pitch = 1.0f, [CanBeNull] Action done = null) {

                    var locale = LocaleHelpers.GetPrefixFromLanguage(this.Language);
                    EasySpeech.SpeakWithSettings(this.Statement, locale, rate, pitch, done);
                }


                /**
                 * <summary> Speaks the answer at the given index with the given rate and pitch. </summary>
                 *
                 * <param name = "rate"> The rate of the voice. </param>
                 * <param name = "pitch"> The pitch of the voice. </param>
                 * <param name = "done"> Callback fired after the text has been speaked. </param>
                */

                [PublicAPI] public void SpeakAnswer (int index, float rate = 1.0f, float pitch = 1.0f, [CanBeNull] Action done = null) {
                    
                    if (index >= 0 && index < this._answers.Length) {

                        var locale = LocaleHelpers.GetPrefixFromLanguage(this.Language);
                        var letter = (char) ('A' + index);
                        
                        // Speak the answer with the letter.
                        EasySpeech.SpeakWithSettings(letter.ToString(), locale, rate, pitch, () => TimeHelpers.SetTimeout(0.8f, 
                            () => EasySpeech.SpeakWithSettings(this.PossibleAnswers [index], locale, rate, pitch, done)));
                    }
                }

            #endregion

        #endregion
    }
}
