#region USED NAMESPACES

    using System;

    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Definitions;

#endregion


namespace SenseQuiz.Utilities {

    
    /// <summary> Contains some utility methods for locales. </summary>
    public static class LocaleHelpers {

        #region METHODS
        
            #region PUBLIC

                /**
                 * <summary> Given a prefix, returns the equivalent language. </summary>
                 *
                 * <param name = "prefix"> The prefix of the language to obtain. </param>
                 * <returns> The equivalent language to the given prefix. </returns>
                */
                        
                [PublicAPI] public static VoiceLanguage GetLanguageFromPrefix ([NotNull] string prefix) {

                    return prefix switch {

                        "ca-ES" => VoiceLanguage.Catalan,
                        "es-ES" => VoiceLanguage.Spanish,
                        "en-GB" => VoiceLanguage.BritishEnglish,
                        "en-US" => VoiceLanguage.AmericanEnglish,

                        _ => throw new ArgumentOutOfRangeException (nameof (prefix), prefix, "Unknown prefix was given.")
                    };
                }
                        
                /**
                 * <summary> Given a language, returns the equivalent prefix. </summary>
                 *
                 * <param name = "language"> The language which prefix should be returned. </param>
                 * <returns> The equivalent prefix for the given language. </returns>
                */
                        
                [PublicAPI, NotNull] public static string GetPrefixFromLanguage (VoiceLanguage language) {

                    return language switch {

                        VoiceLanguage.Catalan => "ca-ES",
                        VoiceLanguage.Spanish => "es-ES",
                        VoiceLanguage.BritishEnglish => "en-GB",
                        VoiceLanguage.AmericanEnglish => "en-US",

                        _ => throw new ArgumentOutOfRangeException(nameof (language), language, "Unknown language was given.")
                    };
                }
                
            #endregion
        
        #endregion
    }
}
