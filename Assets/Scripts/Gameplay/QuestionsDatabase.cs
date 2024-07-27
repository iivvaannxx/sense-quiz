#region USED NAMESPACES
     
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using SenseQuiz.Gameplay.Definitions;
    using UnityEngine;

#endregion


namespace SenseQuiz.Gameplay {

    /// <summary> Stores all the questions and allows easy acess to them. </summary>
    public static class QuestionsDatabase {

        #region FIELDS

            /// <summary> Stores all the questions by their language. </summary>
            private static Dictionary<VoiceLanguage, List<Question>> _questionsByLanguage
                = new Dictionary<VoiceLanguage, List<Question>>();
            
            /// <summary> Stores all the questions by their categories.. </summary>
            private static Dictionary<QuestionCategories, List<Question>> _questionsByCategories
                = new Dictionary<QuestionCategories, List<Question>> {{
                    
                    // Preregister the mixed category.
                    QuestionCategories.Mixed, new List<Question>() 
                }};

        #endregion


        #region METHODS

            #region PUBLIC

                /**
                 * <summary> Registers a new question with the given language. </summary>
                 * 
                 * <param name = "question"> The question to register. </param>
                 * <param name = "language"> The language of the question. </param>
                */
                
                [PublicAPI] public static void RegisterQuestion (Question question, VoiceLanguage language, QuestionCategories category) {

                    if (QuestionsDatabase._questionsByLanguage.ContainsKey(language))
                        QuestionsDatabase._questionsByLanguage [language].Add(question);
                    
                    else {

                        var list = new List<Question> {question};
                        QuestionsDatabase._questionsByLanguage.Add(language, list);
                    }

                    if (QuestionsDatabase._questionsByCategories.ContainsKey(category))
                        QuestionsDatabase._questionsByCategories[category].Add(question);
                    
                    else {
                        
                        var list = new List<Question> {question};
                        QuestionsDatabase._questionsByCategories.Add(category, list);
                    }
                    
                    // Every question belongs to the mixed category.
                    QuestionsDatabase._questionsByCategories[QuestionCategories.Mixed].Add(question);
                }


                /**
                 * <summary> Gets all the questions with the given categories. </summary>
                 * <param name = "categories"> The categories of the questions to retrieve. </param>
                 *
                 * <returns> An immutable list with all the questions with the given categories. </returns>
                */
                
                [CanBeNull, PublicAPI] public static IList <Question> GetQuestionsOfCategory (QuestionCategories categories) 
                    => QuestionsDatabase._questionsByCategories.ContainsKey(categories) ? QuestionsDatabase._questionsByCategories [categories] : null;

                
                /**
                 * <summary> Gets all the questions with the given language. </summary>
                 * <param name = "language"> The language of the questions to retrieve. </param>
                 *
                 * <returns> An immutable list with all the questions with the given language. </returns>
                */
                
                [CanBeNull, PublicAPI] public static IList <Question> GetQuestionsWithLanguage (VoiceLanguage language) 
                    => QuestionsDatabase._questionsByLanguage.ContainsKey(language) ? QuestionsDatabase._questionsByLanguage [language] : null;

                
                /**
                 * <summary> Gets a random question with the given language (any category). </summary>
                 * <param name = "language"> The language of the question to retrieve. </param>
                 *
                 * <returns> A random question of the given language if exists, otherwise null. </returns>
                */

                [CanBeNull, PublicAPI] public static Question GetRandomQuestionWithLanguage (VoiceLanguage language) {

                    if (QuestionsDatabase._questionsByLanguage.ContainsKey(language)) {

                        if (QuestionsDatabase._questionsByLanguage [language].Count > 0) {
                            
                            // Grab a random position and return the question in it.
                            var randomIndex = Random.Range(0, QuestionsDatabase._questionsByLanguage [language].Count);
                            return QuestionsDatabase._questionsByLanguage[language][randomIndex];
                        }
                    }

                    return null;
                }

            #endregion

        #endregion
    }
}
