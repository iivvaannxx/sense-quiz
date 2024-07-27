namespace SenseQuiz.Gameplay.Definitions {

    /// <summary> Defines the different categories of a question. </summary>
    [System.Flags, JetBrains.Annotations.PublicAPI] public enum QuestionCategories {

        /// <summary> Null equivalent for QuestionCategories. </summary>
        None = 0,

        /// <summary> Art related questions. </summary>
        Art = 1 << 0,

        /// <summary> Sports related questions. </summary>
        Sports = 1 << 1,

        /// <summary> History related questions. </summary>
        History = 1 << 2,
        
        /// <summary> Science related questions. </summary>
        Science = 1 << 3,

        /// <summary> Geography related questions. </summary>
        Geography = 1 << 4,

        /// <summary> Entertainment related questions. </summary>
        Entertainment = 1 << 5,

        /// <summary> All the categories. </summary>
        Mixed = Art | Sports | History | Science | Geography | Entertainment,
    }

}
