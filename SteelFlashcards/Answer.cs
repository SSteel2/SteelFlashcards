using System;

namespace LanguageLearn2
{
    public class Answer
    {
        public string Word { get; set; }
        public string Guess { get; set; }
        public bool IsCorrect { get; set; }
        public DateTimeOffset AttemptDateTime { get; set; }
    }

    public class LearnPageAnswer : Answer
    {
        public int Order { get; set; }
        public string CorrectAnswer { get; set; }

        private static int s_lastOrder = 0;

        public LearnPageAnswer(string word, string correctAnswer, string guess) 
            : this(word, correctAnswer, guess, correctAnswer == guess) { }

        public LearnPageAnswer(string word, string correctAnswer, string guess, bool isCorrect)
        {
            Order = ++s_lastOrder;
            AttemptDateTime = DateTimeOffset.Now;
            Word = word;
            CorrectAnswer = correctAnswer;
            Guess = guess;
            IsCorrect = isCorrect;
        }

        public static void Reset()
        {
            s_lastOrder = 0;
        }
    }
}
