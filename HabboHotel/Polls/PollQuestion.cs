using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Polls.Enums;

namespace Plus.HabboHotel.Polls
{
    /// <summary>
    ///     Class PollQuestion.
    /// </summary>
    public class PollQuestion
    {
        /// <summary>
        ///     The answers
        /// </summary>
        internal string[] Answers;

        /// <summary>
        ///     a type
        /// </summary>
        internal PollAnswerType AType;

        /// <summary>
        ///     The correct answer
        /// </summary>
        internal string[] CorrectAnswers;

        /// <summary>
        ///     The index
        /// </summary>
        internal int Index;

        /// <summary>
        ///     The question
        /// </summary>
        internal string Question;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PollQuestion" /> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="question">The question.</param>
        /// <param name="aType">a type.</param>
        /// <param name="answers">The answers.</param>
        /// <param name="correctAnswer">The correct answer.</param>
        internal PollQuestion(int index, string question, int aType, string answers, string correctAnswer)
        {
            Index = index;
            Question = question;
            AType = (PollAnswerType) aType;
            Answers = answers.Split(',');
            CorrectAnswers = correctAnswer.Split(',');
        }
    }
}