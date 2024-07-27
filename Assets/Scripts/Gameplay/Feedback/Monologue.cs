#region USED NAMESPACES

    using System;
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Feedback;
    using SenseQuiz.Utilities;

#endregion


namespace SenseQuiz.Gameplay.Definitions {
    
    /// <summary> Represents a simple speechable line with a bound delay after it's end. </summary>
    [PublicAPI] public readonly struct Dialog {

        #region CONSTRUCTORS AND FINALIZERS

            /**
             * <summary> Builds a simple dialog with the given data. </summary>
             *
             * <param name = "text"> The text of the dialog. </param>
             * <param name = "afterDelay"> The delay used to wait after the text has been spoken. </param>
            */
            
            public Dialog (string text, float afterDelay = 0.5f) {

                this.Line = text;
                this.AfterDelay = afterDelay;
            }

        #endregion


        #region PROPERTIES AND INDEXERS

            /// <summary> After speaking the dialog line, this delay will be applied before reading anything else. </summary>
            public float AfterDelay { get; }

            /// <summary> The text to be read. </summary>
            public string Line { get; }

        #endregion
    }

    
    /// <summary> Exposes a simple API to speak a large monlogue (collection of dialogs). </summary>
    public class Monologue {

        #region FIELDS

            /// <summary> An internal copy of the dialogs used to speak them one by one. </summary>
            private List <Dialog> _dialogsCopy;

        #endregion


        #region CONSTRUCTORS AND FINALIZERS

            /**
             * <summary> Construcs a simple monologue with the given dialogs. </summary>
             * <param name = "dialogs"> The initial dialogs of the monologue. </param>
            */
            
            public Monologue ([NotNull] params Dialog [ ] dialogs) {
                
                this.Dialogs = dialogs.ToList();

                // Build the text,
                var builder = new StringBuilder();
                this.Dialogs.ForEach(current => builder.AppendLine(current.Line + "\n"));
                this.FullText = builder.ToString();
            }

        #endregion


        #region PROPERTIES AND INDEXERS

            /// <summary> The dialogs to be spoken. </summary>
            public List <Dialog> Dialogs { get; private set; }
            
            /// <summary> Is the monologue currently being spoken? </summary>
            public bool IsBeingSpoken { get; private set; }
            
            /// <summary> Returns all the text of the monologue by appending all the dialogue lines. </summary>
            public string FullText { get; private set; }

        #endregion


        #region METHODS

            #region PUBLIC

                /**
                 * <summary> Speaks the monologue with the given rate and pitch. </summary>
                 * 
                 * <param name = "locale"> The locale of the voice. </param>
                 * <param name = "rate"> The rate of the voice. </param>
                 * <param name = "pitch"> The pitch of the voice. </param>
                 * <param name = "done"> Callback fired after the text has been speaked. </param>
                */
                
                [PublicAPI] public void Speak (string locale = "es-ES", float rate = 1.0f, float pitch = 1.0f, [CanBeNull] Action done = null) {

                    if (this.Dialogs.Count > 0) {

                        // Create the copy in order to start the talk,
                        this._dialogsCopy = this.Dialogs.ToList();
                        this.IsBeingSpoken = true;
                        
                        // Start the speak.
                        this.InternalSpeak(locale, rate, pitch, () => {

                            this.IsBeingSpoken = false;
                            done?.Invoke();
                        });
                    }
                }

            #endregion


            #region PRIVATE

                /**
                 * <summary> Recursively speaks all the dialogues of the monologue. </summary>
                 * 
                 * <param name = "locale"> The locale of the voice. </param>
                 * <param name = "rate"> The rate of the voice. </param>
                 * <param name = "pitch"> The pitch of the voice. </param>
                 * <param name = "done"> Callback fired after the text has been speaked. </param>
                */
                
                private void InternalSpeak (string locale = "es-ES", float rate = 1.0f, float pitch = 1.0f, [CanBeNull] Action done = null) {

                    if (this._dialogsCopy.Count > 0) {

                        // Recursively speak all the dialogs until finished.
                        EasySpeech.SpeakWithSettings(this._dialogsCopy [0].Line, locale, rate, pitch, () => TimeHelpers.SetTimeout(
                            this._dialogsCopy [0].AfterDelay, () => {
                                
                                // Was the last dialog.
                                if (this._dialogsCopy.Count == 1) {

                                    this._dialogsCopy.Clear();
                                    done?.Invoke();
                                }

                                else {

                                    // There are still pending dialogs.
                                    this._dialogsCopy.RemoveAt(0);
                                    this.InternalSpeak(locale, rate, pitch, done);
                                }
                            })
                        );
                    }

                    else
                        this.IsBeingSpoken = false;
                }

            #endregion

        #endregion
    }
}
