#region USED NAMESPACES

    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Definitions;
    using UnityEngine;

#endregion


namespace SenseQuiz {

    /// <summary> Contains the settings of a single synthethic voice. </summary>
    [CreateAssetMenu(fileName = "Voice", menuName = "Scriptable Objects/Voice")]
    public class VoiceSettings : ScriptableObject {

        #region SERIALIZED PROPERTIES

            /// <summary> The current language used by the question speaker. </summary>
            [field: SerializeField, Tooltip("The current language used by the question speaker."), Space] [PublicAPI]
            public VoiceLanguage CurrentLanguage { get; set; } = VoiceLanguage.Catalan;
            
            /// <summary> The rate of the voice which speaks all the questions. </summary>
            [field: SerializeField, Tooltip("The rate of the voice."), Range(0.1f, 3f), Space] [PublicAPI]
            public float Rate { get; set; } = 1.0f;
            
            /// <summary> The pitch of the voice which speaks all the questions. </summary>
            [field: SerializeField, Tooltip("The pitch of the voice."), Range(0.1f, 3f), Space] [PublicAPI]
            public float Pitch { get; set; } = 1.0f;

        #endregion
    }
}
