#region USED NAMESPACES

    using System;
    using DG.Tweening;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using UnityEngine;
    using UnityEngine.UI;

#endregion


namespace SenseQuiz.UI {

    /// <summary> Handles almost everything related to the UI. </summary>
    public class UserInterfaceManager : MonoBehaviour {

        #region NON-SERIALIZED FIELDS

            /// <summary> Global access instance variable. </summary>
            public static UserInterfaceManager Instance = null;

        #endregion
        
        
        #region SERIALIZED FIELDS

            /// <summary> The black overlay of the screen. </summary>
            [SerializeField, Tooltip("The black overlay of the screen."), Space]
            private RawImage _overlay;
        
        #endregion
        


        #region SERIALIZED PROPERTIES

            /// <summary> The text in the header of the screen. </summary>
            [field: SerializeField, Tooltip("The text at the header of the screen."), Space] [PublicAPI]
            public FadingText HeaderText { get; private set; } = null;
            
            /// <summary> The text at the center of the screen. </summary>
            [field: SerializeField, Tooltip("The text at the center of the screen."), Space] [PublicAPI]
            public FadingText CentralText { get; private set; } = null;
            
            /// <summary> The text of the current monologue. </summary>
            [field: SerializeField, Tooltip("The text of the current monologue."), Space] [PublicAPI]
            public FadingText MonologueText { get; private set; } = null;
            
            /// <summary> The subtext of the current monologue. </summary>
            [field: SerializeField, Tooltip("The subtext of the current monologue."), Space] [PublicAPI]
            public FadingText MonologueSubText { get; private set; } = null;
            
            /// <summary> The reference to all the answer fading text component. </summary>
            [field: SerializeField, Tooltip("The reference to all the fading text answers."), Space] [PublicAPI]
            public List<FadingText> AnswerTexts { get; private set; }
            
        #endregion


        #region UNITY EVENTS

            private void Awake () => UserInterfaceManager.Instance = this;

        #endregion


        #region METHODS

            #region PUBLIC
            
                /**
                 * <summary> Fades in the screen in the given duration. </summary>
                 *
                 * <param name = "duration"> The duration of the fade. </param>
                 * <param name = "complete"> Callback fired when the fade finishes. </param>
                */

                [PublicAPI] public void FadeInScreen (float duration, Color? color = null, [CanBeNull] Action complete = null) {

                    // Shorthand variables.
                    var alpha = 1.0f;
                    color ??= Color.black;
                    
                    // Override the initial color and start the fade.
                    this._overlay.color = new Color(color.Value.r, color.Value.g, color.Value.b, alpha);
                    var tween = DOTween.To(() => alpha, (value) => {
                        
                        alpha = value;
                        this._overlay.color = new Color(color.Value.r, color.Value.g, color.Value.b, alpha);

                    }, 0.0f, duration);

                    tween.onComplete += () => complete?.Invoke();
                }
                
                
                /**
                 * <summary> Fades out the screen in the given duration. </summary>
                 *
                 * <param name = "duration"> The duration of the fade. </param>
                 * <param name = "complete"> Callback fired when the fade finishes. </param>
                */

                [PublicAPI] public void FadeOutScreen (float duration, Color? color = null, [CanBeNull] Action complete = null) {

                    // Shorthand variables.
                    var alpha = 0.0f;
                    color ??= Color.black;
                    
                    // Override the initial color and start the fade.
                    this._overlay.color = new Color(color.Value.r, color.Value.g, color.Value.b, alpha);
                    var tween = DOTween.To(() => alpha, (value) => {
                        
                        alpha = value;
                        this._overlay.color = new Color(color.Value.r, color.Value.g, color.Value.b, alpha);

                    }, 1.0f, duration);

                    tween.onComplete += () => complete?.Invoke();
                }
            

                /**
                 * <summary> Sets the text of the central component. </summary>
                 * <param name = "text"> The new text of the central component. </param>
                */
                
                [PublicAPI] public void SetCenter (string text) => this.CentralText.Text = text;


                /**
                 * <summary> Sets the text of the header. </summary>
                 * <param name = "text"> The new text of the header. </param>
                */
                
                [PublicAPI] public void SetHeader (string text) => this.HeaderText.Text = text;
                
                
                /**
                 * <summary> Sets the text of the monologue. </summary>
                 * <param name = "text"> The new text of the monologue. </param>
                */
                
                [PublicAPI] public void SetMonologue (string text) => this.MonologueText.Text = text;
                
                
                /**
                 * <summary> Sets the sub text of the monologue. </summary>
                 * <param name = "text"> The new text of the sub monologue. </param>
                */
                
                [PublicAPI] public void SetSubMonologue (string text) => this.MonologueSubText.Text = text;

            #endregion

        #endregion
    }
}
