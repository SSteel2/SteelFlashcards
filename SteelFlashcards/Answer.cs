using System.Text.Json.Serialization;

namespace LanguageLearn2
{
    public class JsonAnswer
    {
        public string Word { get; set; }
        public string Guess { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class Answer : JsonAnswer
    {
        public int Order { get; set; }
        public string CorrectAnswer { get; set; }

        private static int s_lastOrder = 0;

        public Answer(string word, string correctAnswer, string guess) 
            : this(word, correctAnswer, guess, correctAnswer == guess) { }

        public Answer(string word, string correctAnswer, string guess, bool isCorrect)
        {
            Order = ++s_lastOrder;
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
