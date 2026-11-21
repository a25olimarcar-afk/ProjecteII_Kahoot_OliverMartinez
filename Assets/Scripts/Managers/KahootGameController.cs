using System;
using UnityEngine;

public class KahootGameController : MonoBehaviour
{
    private KahootEntry currentKahoot; //kahoot que se juega
    private int currentQuestionIndex; //pregunta que se esta mostrando

    private int totalScore; //puntos acumulados en la partida
    private int correctAnswers; //numero de aciertos

    private float timeRemaining; //segundos que tiene el usuario para responder
    private float totalElapsedTime; //variable de tiempo total

    private bool questionActive;
    private bool questionAnswered;
    private bool gameActive;

    //eventos para comunicar los cambios a la interfaz
    public event Action<float> TimerUpdated;
    public event Action<bool, bool, string> AnswerResolved;

    //propiedades para consultar el estado actual de la partida
    public QuestionData CurrentQuestion
    {
        get
        {
            if (currentKahoot == null || currentKahoot.data == null ||
                currentQuestionIndex >= currentKahoot.data.questions.Count)
            { return null; }

            return currentKahoot.data.questions[currentQuestionIndex];
        }
    }
    public int CurrentQuestionIndex { get { return currentQuestionIndex; } }
    public int QuestionCount
    {
        get
        {
            if (currentKahoot == null || currentKahoot.data == null)
            { return 0; }

            return currentKahoot.data.questions.Count;
        }
    }
    public int TotalScore { get { return totalScore; } }
    public int CorrectAnswers { get { return correctAnswers; } }
    public float TotalElapsedTime { get { return totalElapsedTime; } }
    public float TimeRemaining { get { return timeRemaining; } }

    private void Update()
    {
        //si no hay pregunta activa
        if (!questionActive) { return; } //no actualizamos el temporizador

        timeRemaining -= Time.deltaTime; //actualizamos el tiempo restante

        //cuando se acaba el tiempo resolvemos la pregunta como no respondida
        if (timeRemaining <= 0)
        {
            timeRemaining = 0; //evitar mostrar numeros negativos
            TimerUpdated?.Invoke(timeRemaining);

            ResolveAnswer(-1); //-1 --> no se eligio ninguna respuesta
            return;
        }
        TimerUpdated?.Invoke(timeRemaining);
    }

    public bool StartGame(KahootEntry kahoot)
    {
        StopGame();

        //comprobar que el kahoot tenga preguntas disponibles
        if (kahoot == null ||
            kahoot.data == null ||
            kahoot.data.questions == null ||
            kahoot.data.questions.Count == 0)
        {
            Debug.LogError("Invalid Kahoot!!!"); return false;
        }

        currentKahoot = kahoot;

        //reiniciamos los valores de la partida
        currentQuestionIndex = 0;
        totalScore = 0;
        correctAnswers = 0;
        totalElapsedTime = 0;

        gameActive = true;
        BeginCurrentQuestion();
        return true;
    }

    private void BeginCurrentQuestion()
    {
        //iniciar el temporizador de la pregunta actual
        timeRemaining = CurrentQuestion.duration;

        questionAnswered = false;
        questionActive = true;

        TimerUpdated?.Invoke(timeRemaining);
    }

    public bool AdvanceQuestion()
    {
        //solo podemos avanzar si la pregunta actual ya se ha resuelto
        if (!gameActive || questionActive || !questionAnswered)
        { return false; }

        if (currentQuestionIndex >= QuestionCount - 1)
        { return false; }

        currentQuestionIndex++;
        BeginCurrentQuestion();
        return true;
    }

    public void SubmitAnswer(int answerIndex)
    {
        ResolveAnswer(answerIndex); //recibe la respuesta seleccionada por el jugador
    }

    private void ResolveAnswer(int answerIndex)
    {
        //evitar resolver una pregunta mas de una vez
        if (!gameActive || questionAnswered || !questionActive)
        { return; }

        //comprobar que el indice recibido sea valido
        if (answerIndex < -1 || answerIndex >= CurrentQuestion.answers.Count)
        { return; }

        //sumar tiempo utilizado en la pregunta actual
        totalElapsedTime += CurrentQuestion.duration - timeRemaining;

        questionAnswered = true;
        questionActive = false;

        bool timedOut = answerIndex == -1;

        //comprobar si la respuesta seleccionada era la correcta
        bool isCorrect = !timedOut && CurrentQuestion.answers[answerIndex].isCorrect;

        if (isCorrect)
        {
            correctAnswers++;
            //cuanto mas tiempo quede, mas puntos consigue el jugador
            float timePercentage = timeRemaining / CurrentQuestion.duration;
            int questionScore = Mathf.RoundToInt(1000f * timePercentage);

            totalScore += questionScore;
        }

        string correctAnswer = ""; //obtener texto de la respuesta correcta

        foreach (AnswerData answer in CurrentQuestion.answers)
        {
            if (answer.isCorrect)
            {
                correctAnswer = answer.answerText;
                break;
            }
        }
        //avisar a la interfaz de que la pregunta ha terminado
        AnswerResolved?.Invoke(isCorrect, timedOut, correctAnswer);
    }

    public LeaderboardResult CreateLeaderboardResult(string playerName)
    {
        //generar resultado final de la partida
        LeaderboardResult result = new LeaderboardResult();

        result.playerName = playerName;
        result.score = totalScore;
        result.correctAnswers = correctAnswers;
        result.totalQuestions = QuestionCount;
        result.completionTime = totalElapsedTime;
        result.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        return result;
    }

    public void StopGame()
    {
        //detener temporizador y la partida
        questionActive = false;
        gameActive = false;
    }
}
