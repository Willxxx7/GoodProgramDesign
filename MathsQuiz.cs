using System;

namespace MathsQuiz
{
    /// <summary>
    /// Entry point for the Maths Quiz application.
    /// 
    /// DESIGN PRINCIPLE — SEPARATION OF CONCERNS:
    /// Main() does almost nothing. It just wires things together and starts
    /// the program. All the real work lives in dedicated classes, each with
    /// a single responsibility. This makes each piece easy to test, reason
    /// about, and replace without breaking the rest.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // The quiz needs three collaborators:
            //   - a source of questions
            //   - a thing that runs the quiz
            //   - a thing that reports the result
            //
            // By constructing them here (and nowhere else), we make the
            // dependencies of the quiz explicit. If we ever want to swap
            // a component out (e.g. read questions from a file), this is
            // the only place we need to change.
            var questionSource = new RandomQuestionSource();
            var reportWriter = new ConsoleReportWriter();
            var quiz = new Quiz(questionSource, reportWriter);

            quiz.Run();
        }
    }

    // ==================================================================
    //  DATA MODEL
    // ==================================================================

    /// <summary>
    /// A single quiz question. Immutable — once created, it cannot change.
    ///
    /// DESIGN PRINCIPLE — IMMUTABILITY:
    /// Making this a simple, read-only record makes it impossible to
    /// accidentally corrupt a question part-way through the quiz. Bugs
    /// that can't happen are better than bugs that are caught.
    /// </summary>
    public class Question
    {
        public int LeftNumber { get; }
        public int RightNumber { get; }
        public int CorrectAnswer => LeftNumber * RightNumber;

        public Question(int left, int right)
        {
            LeftNumber = left;
            RightNumber = right;
        }

        /// <summary>
        /// Returns a human-readable form of the question.
        /// Kept here (not in the UI class) because the question knows
        /// best how to describe itself.
        /// </summary>
        public override string ToString() => $"{LeftNumber} x {RightNumber}";
    }

    /// <summary>
    /// The outcome of a single answered question.
    ///
    /// DESIGN PRINCIPLE — MODEL THE DOMAIN:
    /// Rather than passing around loose bools and ints, we give the
    /// concept of "a result" a name and a type. This makes the code
    /// read like the problem, not like the implementation.
    /// </summary>
    public class AnswerResult
    {
        public Question Question { get; }
        public int GivenAnswer { get; }
        public bool WasCorrect { get; }

        public AnswerResult(Question question, int givenAnswer, bool wasCorrect)
        {
            Question = question;
            GivenAnswer = givenAnswer;
            WasCorrect = wasCorrect;
        }
    }

    // ==================================================================
    //  ABSTRACTIONS — the "ports" of the application
    // ==================================================================

    /// <summary>
    /// Anything that can produce questions for the quiz.
    ///
    /// DESIGN PRINCIPLE — PROGRAM TO AN INTERFACE, NOT A CONCRETE TYPE:
    /// The Quiz doesn't know or care *how* questions are produced. Today
    /// it's random numbers; tomorrow it could be read from a file, a
    /// database, or an API. The Quiz class doesn't change.
    /// This is called "dependency inversion" and it's what makes code
    /// flexible without being complicated.
    /// </summary>
    public interface IQuestionSource
    {
        Question Next();
    }

    /// <summary>
    /// Anything that can display the quiz and collect answers.
    ///
    /// DESIGN PRINCIPLE — SEPARATE LOGIC FROM PRESENTATION:
    /// The quiz logic (are you right? is it over?) has nothing to do
    /// with whether we're writing to a console, a window, or a web page.
    /// Splitting them means we can change the UI without touching the
    /// rules, and test the rules without a UI at all.
    /// </summary>
    public interface IReportWriter
    {
        void ShowWelcome(string name, int totalQuestions);
        void ShowQuestion(int number, int total, Question question, int currentScore);
        void ShowCorrectAnswer();
        void ShowWrongAnswer(int correct);
        void ShowInvalidInput();
        void ShowFinalReport(string name, int score, int total);
    }

    // ==================================================================
    //  IMPLEMENTATIONS
    // ==================================================================

    /// <summary>
    /// Produces random multiplication questions.
    ///
    /// DESIGN PRINCIPLE — INJECT DEPENDENCIES, DON'T CREATE THEM:
    /// The Random object is passed in rather than created inside.
    /// This lets us (a) reuse the same Random across sources if we want,
    /// and (b) pass a *fake* Random in tests to get predictable results.
    /// </summary>
    public class RandomQuestionSource : IQuestionSource
    {
        private readonly Random _random;
        private readonly int _min;
        private readonly int _max;

        public RandomQuestionSource(int min = 1, int max = 10)
            : this(new Random(), min, max) { }

        // This overload is used by tests to inject a seeded Random.
        public RandomQuestionSource(Random random, int min = 1, int max = 10)
        {
            _random = random;
            _min = min;
            _max = max;
        }

        public Question Next()
        {
            // NOTE: Random.Next's upper bound is *exclusive*, so we add 1
            // to make the range inclusive. Easy bug to write by accident,
            // and easier to catch when it's written down.
            int left = _random.Next(_min, _max + 1);
            int right = _random.Next(_min, _max + 1);
            return new Question(left, right);
        }
    }

    /// <summary>
    /// Writes quiz output to the console.
    ///
    /// DESIGN PRINCIPLE — SMALL, FOCUSED METHODS:
    /// Each method does exactly one thing. This makes the console code
    /// easy to change (e.g. to add colour) without affecting any other
    /// part of the program, and easy to read at a glance.
    /// </summary>
    public class ConsoleReportWriter : IReportWriter
    {
        public void ShowWelcome(string name, int totalQuestions)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("        MATHS QUIZ — ARE YOU READY?");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine($"Hello {name}! You'll get {totalQuestions} questions.");
            Console.WriteLine("Type your answer and press Enter.");
            Console.WriteLine();
            Console.WriteLine("Press any key to begin...");
            Console.ReadKey();
        }

        public void ShowQuestion(int number, int total, Question question, int currentScore)
        {
            Console.Clear();
            Console.WriteLine($"Question {number} of {total}");
            Console.WriteLine($"Score so far: {currentScore}");
            Console.WriteLine();
            Console.Write($"{question} = ");
        }

        public void ShowCorrectAnswer()
        {
            Console.WriteLine();
            Console.WriteLine("Correct! Well done.");
        }

        public void ShowWrongAnswer(int correct)
        {
            Console.WriteLine();
            Console.WriteLine($"Not quite. The answer is {correct}.");
        }

        public void ShowInvalidInput()
        {
            Console.WriteLine();
            Console.WriteLine("That wasn't a whole number. Please try again.");
        }

        public void ShowFinalReport(string name, int score, int total)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("             QUIZ COMPLETE");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine($"Well done {name} — you scored {score} out of {total}.");
            Console.WriteLine();

            // DESIGN PRINCIPLE — PUSH DETAILS INTO DEDICATED METHODS:
            // The "what to say about the score" logic has a name. If we
            // ever want to change the wording, this is the only place
            // we look.
            Console.WriteLine(GetEncouragement(score, total));
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // Private because it's an implementation detail. No caller
        // outside this class needs to know how encouragement is worded.
        private string GetEncouragement(int score, int total)
        {
            double percentage = (double)score / total * 100;

            if (percentage == 100) return "Perfect score! Outstanding work.";
            if (percentage >= 80)  return "Excellent work — you've got this.";
            if (percentage >= 60)  return "Good job. Keep practising and you'll be great.";
            if (percentage >= 40)  return "Nice try. Practice makes progress.";
            return "Good effort. Keep going — every attempt helps.";
        }
    }

    // ==================================================================
    //  CORE LOGIC
    // ==================================================================

    /// <summary>
    /// Runs a quiz: asks questions, collects answers, tracks the score.
    ///
    /// DESIGN PRINCIPLE — THE CORE IS UI-AGNOSTIC:
    /// Notice that this class never calls Console.WriteLine directly.
    /// It goes through IReportWriter. That means:
    ///   - the same quiz can drive a console, a GUI, or a web page
    ///   - we can test the logic with a *fake* writer that just counts calls
    /// This is what people mean when they say "keep logic out of the UI".
    /// </summary>
    public class Quiz
    {
        // A named constant, not a magic number buried in code.
        // If we want 20 questions instead of 10, this is the only change.
        private const int TotalQuestions = 10;

        private readonly IQuestionSource _questionSource;
        private readonly IReportWriter _writer;

        public Quiz(IQuestionSource questionSource, IReportWriter writer)
        {
            _questionSource = questionSource;
            _writer = writer;
        }

        public void Run()
        {
            string name = AskForName();
            _writer.ShowWelcome(name, TotalQuestions);

            int score = 0;

            // We loop exactly TotalQuestions times. A `for` loop is the
            // clearest expression of "do this N times" — better than a
            // while loop with a counter, because the intent is obvious.
            for (int i = 1; i <= TotalQuestions; i++)
            {
                Question question = _questionSource.Next();
                int answer = AskQuestion(i, question, score);

                if (answer == question.CorrectAnswer)
                {
                    score++;
                    _writer.ShowCorrectAnswer();
                }
                else
                {
                    _writer.ShowWrongAnswer(question.CorrectAnswer);
                }

                WaitForUserToContinue();
            }

            _writer.ShowFinalReport(name, score, TotalQuestions);
        }

        /// <summary>
        /// Asks a single question and returns a valid answer.
        /// Keeps asking until the user types a number — so invalid
        /// input is handled here and not by the caller.
        /// </summary>
        private int AskQuestion(int number, Question question, int score)
        {
            while (true) // loop until we get a valid answer
            {
                _writer.ShowQuestion(number, TotalQuestions, question, score);

                string input = Console.ReadLine();

                // Guard clauses: handle bad input up front, then fall
                // through to the "happy path" at the end. This keeps the
                // valid path unindented and easy to read.
                if (string.IsNullOrWhiteSpace(input))
                {
                    _writer.ShowInvalidInput();
                    continue;
                }

                if (!int.TryParse(input, out int answer))
                {
                    _writer.ShowInvalidInput();
                    continue;
                }

                return answer;
            }
        }

        private string AskForName()
        {
            Console.Write("What's your name? ");
            string? name = Console.ReadLine();
            return string.IsNullOrWhiteSpace(name) ? "Friend" : name.Trim();
        }

        private void WaitForUserToContinue()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key for the next question...");
            Console.ReadKey();
        }
    }
}
