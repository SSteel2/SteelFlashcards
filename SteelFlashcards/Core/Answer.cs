using System;
using System.Diagnostics.CodeAnalysis;

namespace SteelFlashcards;

public class Answer
{
    public required string Word { get; set; }
    public required string Guess { get; set; }
    public bool IsCorrect { get; set; }
    public DateTimeOffset AttemptDateTime { get; set; }
}

public class LearnPageAnswer : Answer
{
    public int Order { get; set; }
    public string CorrectAnswer { get; set; }

    private static int s_lastOrder = 0;

    [SetsRequiredMembers]
    public LearnPageAnswer(string word, string correctAnswer, string guess) 
        : this(word, correctAnswer, guess, correctAnswer == guess) { }

    [SetsRequiredMembers]
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
