#region USED NAMESPACES

    using System;
    using Newtonsoft.Json;
    using JetBrains.Annotations;
    using System.Collections.Generic;

#endregion


namespace SenseQuiz.Gameplay.Definitions {

    /// <summary> Groups some questions in a single category. </summary>
    public class CategoryQuestions {

        #region FIELDS

            /// <summary> The questions of the category. </summary>
            private Question [ ] _questions;

        #endregion


        #region CONSTRUCTORS AND FINALIZERS

            /**
             * <summary> Constructs a new category of questions from the given data. </summary>
             *
             * <param name = "name"> The name of the category. </param>
             * <param name = "questions"> The questions of the category. </param>
            */
            
            [JsonConstructor] public CategoryQuestions ([NotNull] string category, [NotNull] Question [ ] questions) {

                this.Category = (QuestionCategories) Enum.Parse(typeof(QuestionCategories), category);
                this._questions = questions;
            }

        #endregion


        #region PROPERTIES AND INDEXERS

            /// <summary> The name of the category. </summary>
            public QuestionCategories Category { get; private set; }
            
            /// <summary> The questions of the category. </summary>
            public IReadOnlyCollection <Question> Questions => this._questions;

        #endregion
    }
}
