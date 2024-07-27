#region USED NAMESPACES

    using System;
    using DG.Tweening;
    using JetBrains.Annotations;
    
    using TMPro;
    using UnityEngine;

#endregion


namespace SenseQuiz.UI {

    /// <summary> A text component which fades out before being updated. </summary>
    [RequireComponent(typeof (TextMeshProUGUI), typeof (CanvasGroup))]
    public class FadingText : MonoBehaviour {

        #region NON-SERIALIZED FIELDS

            /// <summary> The reference to the canvas group (easily fade text). </summary>
            private CanvasGroup _canvasGroup;
            
            /// <summary> The reference to the text component. </summary>
            private TextMeshProUGUI _textReference;

        #endregion
        
        
        #region SERIALIZED PROPERTIES
        
            /// <summary> The duration of the fade when fading in. </summary>
            [field: SerializeField, Tooltip("The duration of the fade when fading in."), Space] [PublicAPI]
            public float FadeInDuration { get; set; } = 1.0f;

            /// <summary> The duration of the fade when fading out. </summary>
            [field: SerializeField, Tooltip("The duration of the fade when fading out."), Space] [PublicAPI]
            public float FadeOutDuration { get; set; } = 1.0f;
            
            
            /// <summary> Should the text fade in on start? </summary>
            [field: SerializeField, Tooltip("Should the text fade in on start?"), Space] [PublicAPI]
            public bool FadeInOnStart { get; set; } = false;

        #endregion



        #region NON-SERIALIZED PROPERTIES

            /// <summary> The current value of the alpha. </summary>
            [PublicAPI] public float CurrentAlpha { get; private set; }


            /// <summary> A direct reference to the text variable. </summary>
            [PublicAPI] public string Text {

                get => this._textReference.text;
                set => this.SetText(value);
            }
            
        #endregion


        #region UNITY EVENTS

            private void Awake () {

                this._canvasGroup = this.GetComponent<CanvasGroup>();
                this._textReference = this.GetComponent<TextMeshProUGUI>();
                
                if (this.FadeInOnStart)
                    this.FadeIn();
            }
            
            private void Update () => this._canvasGroup.alpha = this.CurrentAlpha;

        #endregion


        #region METHODS

            #region PUBLIC

                /**
                 * <summary> Fades in the text component. </summary>
                 * <param name = "complete"> Callback fired when the text ends the fade. </param>
                */
                
                [PublicAPI] public void FadeIn ([CanBeNull] Action complete = null) {
                    
                    var tween = DOTween.To(() => this.CurrentAlpha, value => this.CurrentAlpha = value, 1.0f, this.FadeInDuration);
                    tween.onComplete += () => complete?.Invoke();
                }


                /**
                 * <summary> Fades out the text component. </summary>
                 * <param name = "complete"> Callback fired when the text ends the fade. </param>
                */
                
                [PublicAPI] public void FadeOut ([CanBeNull] Action complete = null) {
                    
                    var tween = DOTween.To(() => this.CurrentAlpha, value => this.CurrentAlpha = value, 0.0f, this.FadeOutDuration);
                    tween.onComplete += () => complete?.Invoke();
                }

                
                /**
                 * <summary> Fades out the text component, updates it with the given text, and then fades in. </summary>
                 * 
                 * <param name = "text"> The new text to apply to the text component. </param>
                 * <param name = "complete"> Callback fired when the text ends the fade. </param>
                */

                [PublicAPI] public void SetText (string text, [CanBeNull] Action onComplete = null) {

                    this.FadeOut(() => {

                        this._textReference.text = text;
                        this.FadeIn(onComplete);
                    });
                }
                
                
                /**
                 * <summary> Updates the text without fading. </summary>
                 * <param name = "text"> The new text to apply to the text component. </param>
                */

                [PublicAPI] public void SetTextWithoutFade (string text) {

                    this._textReference.text = text;
                }

            #endregion

        #endregion
    }
}
