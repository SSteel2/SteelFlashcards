using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace LanguageLearn2
{
    public class Answer
    {
        public int Order { get; set; }
        public string Word { get; set; }
        public string CorrectAnswer { get; set; }
        public string Guess { get; set; }
        public bool IsCorrect { get; set; }

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
