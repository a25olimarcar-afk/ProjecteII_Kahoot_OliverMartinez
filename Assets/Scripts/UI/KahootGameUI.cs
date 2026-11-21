using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KahootGameUI : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private KahootSelectorUI kahootSelector; //de aqui recuperamos el kahoot elegido por el jugador
    [SerializeField] private WindowManager windowManager; //para poder cambiar de ventana al terminar
    [SerializeField] private LeaderboardManager leaderboardManager;
    [SerializeField] private LeaderboardUI leaderboardUI;
    [SerializeField] private QuestionAudioController questionAudioController;
    [SerializeField] private KahootGameController gameController;

    [Header("Game Header")]
    [SerializeField] private TMP_Text kahootTitleText;
    [SerializeField] private TMP_Text questionProgressText;
    [SerializeField] private TMP_Text timerText;

    [Header("Question UI")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Transform answerButtonsContent; //contenedor donde se instancian los botones de respuesta
    [SerializeField] private AnswerButtonUI answerButtonPrefab;

    [Header("Feedback UI")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text correctAnswerText;
    [SerializeField] private Image feedbackBackgroundImage;
    [SerializeField] private Color32 correctFeedbackColor = new Color32(0, 255, 0, 100); //verde transparente
    [SerializeField] private Color32 incorrectFeedbackColor = new Color32(255, 0, 0, 100); //rojo transparente
    [SerializeField] private Button nextQuestionButton;
    [SerializeField] private TMP_Text nextQuestionButtonText;

    [Header("Results UI")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalRankText;
    [SerializeField] private Button viewLeaderboardButton;
    [SerializeField] private GameObject backButton;

    private KahootEntry currentKahoot; //kahoot que se juega
    private QuestionData currentQuestion; //pregunta que se esta mostrando

    private List<AnswerButtonUI> answerButtons = new List<AnswerButtonUI>(); //botones de la pregunta actual

    private void OnEnable()
    {
        //enlazamos los botones por codigo cada vez que se abre la ventana
        nextQuestionButton.onClick.AddListener(NextQuestion);
        viewLeaderboardButton.onClick.AddListener(OpenLeaderboard);

        //nos suscribimos a los eventos del controller (KahootGameController)
        gameController.TimerUpdated += UpdateTimerText;
        gameController.AnswerResolved += ShowAnswerFeedback;

        StartGame();
    }

    private void OnDisable()
    {
        //quitar listeners para evitar duplicarlos al volver a abrir la ventana
        nextQuestionButton.onClick.RemoveListener(NextQuestion);
        viewLeaderboardButton.onClick.RemoveListener(OpenLeaderboard);

        //dejamos de escuchar los eventos del controlador
        gameController.TimerUpdated -= UpdateTimerText;
        gameController.AnswerResolved -= ShowAnswerFeedback;

        //detener partida y recuperar la musica idle
        gameController.StopGame();
        questionAudioController.ResumeIdleMusic();
    }

    //reinicia todos los valores de la partida y carga la primera pregunta
    private void StartGame()
    {
        //recuperamos el kahoot seleccionado
        currentKahoot = kahootSelector.SelectedKahoot;

        //iniciamos la partida desde el controlador
        if (!gameController.StartGame(currentKahoot))
        { Debug.LogError("No Kahoot has been selected or it is invalid!!!"); return; }

        //titulo del kahoot en la cabecera del panel
        kahootTitleText.text = currentKahoot.displayName;

        //el boton Back se oculta durante la partida
        backButton.SetActive(false); //asi no se puede salir hasta terminar

        LoadQuestion();
    }

    //prepara la interfaz con la pregunta y genera sus botones de respuesta
    private void LoadQuestion()
    {
        ClearAnswerButtons();

        //recuperamos la pregunta actual desde el controlador
        currentQuestion = gameController.CurrentQuestion;

        if (currentQuestion == null)
        { Debug.LogError("Could not load current question"); return; }

        questionText.text = currentQuestion.questionText;

        //mostrar el progreso de la partida
        questionProgressText.text =
        (gameController.CurrentQuestionIndex + 1) +"/"+ gameController.QuestionCount;

        //asegurar que solo se muestra el panel de preguntas
        questionPanel.SetActive(true);
        feedbackPanel.SetActive(false);
        resultsPanel.SetActive(false);

        //generar un boton por cada respuesta encontrada
        for (int i = 0; i < currentQuestion.answers.Count; i++)
        {
            int answerIndex = i;

            AnswerData answer = currentQuestion.answers[i];

            AnswerButtonUI answerButton = 
                Instantiate(answerButtonPrefab, answerButtonsContent);

            //enviamos el indice de la respuesta al controlador
            answerButton.Setup(answer.answerText,
                () => gameController.SubmitAnswer(answerIndex));

            answerButtons.Add(answerButton);
        }
        //seleccionar la primera respuesta de la nueva pregunta
        if (answerButtons.Count > 0)
        {
            Selectable firstAnswer =
                answerButtons[0].GetComponentInChildren<Selectable>();

            if (firstAnswer != null && firstAnswer.IsInteractable())
            {
                SelectUIElement(firstAnswer.gameObject);
            }
        }

        //reproducimos la musica correspondiente a la duracion
        questionAudioController.PlayQuestionMusic(currentQuestion.duration);
    }

    //metodo que comprueba la respuesta elegida, calcula la puntuacion y muestra el feedback
    //answerIndex pasa a valer -1 cuando se acaba el tiempo sin responder
    private void ShowAnswerFeedback(bool isCorrect, bool timedOut, string correctAnswer)
    {
        //desactivar todos los botones para evitar respuestas adicionales
        foreach (AnswerButtonUI button in answerButtons)
        { button.SetInteractable(false); }

        //mostramos el feedback de la pregunta
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);

        //reproducimos el sonido gong al mostrar el feedback
        questionAudioController.StopQuestionMusic(); //detener musica de pregunta
        questionAudioController.PlayFeedbackSound();

        //elegimos el mensaje segun como termino la pregunta
        if (timedOut)
        { feedbackText.text = "Time's up!"; }
        else if (isCorrect)
        { feedbackText.text = "Correct!"; }
        else
        { feedbackText.text = "Incorrect!"; }

        //cambiamos el color del fondo segun el resultado
        feedbackBackgroundImage.color =
            isCorrect ? correctFeedbackColor : incorrectFeedbackColor;

        //mostramos la respuesta correcta
        correctAnswerText.text = "Correct answer: \n" + correctAnswer;

        //cambiamos el texto del boton segun la pregunta
        bool isLastQuestion =
            gameController.CurrentQuestionIndex >= gameController.QuestionCount - 1;

        nextQuestionButtonText.text = isLastQuestion ? "Results" : "Next";

        //seleccionamos Next o Results para continuar
        SelectUIElement(nextQuestionButton.gameObject);
    }

    private void NextQuestion() //avanza a la siguiente pregunta
    {
        //intentamos avanzar a la siguiente pregunta
        if (gameController.AdvanceQuestion())
        {
            LoadQuestion();
        }
        else
        {
            ShowResults(); //muestra los resultados si era la ultima
        }
    }

    //muestra el panel final con la puntuacion total y los aciertos
    private void ShowResults()
    {
        gameController.StopGame();

        //dejamos visible solo el panel de resultados
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        resultsPanel.SetActive(true);

        finalScoreText.text = "Final Score: " + gameController.TotalScore;

        finalRankText.text =
            "Correct Answers: " + gameController.CorrectAnswers +
            " / " + gameController.QuestionCount +
            "\nCompletion Time: " +
            gameController.TotalElapsedTime.ToString("F1") + "s";

        //recuperamos los datos finales de la partida
        LeaderboardResult result = gameController.CreateLeaderboardResult(kahootSelector.PlayerName);

        try //guardamos el resultado en el xml correspondiente
        {
            leaderboardManager.SaveResult(currentKahoot, result);
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not save leaderboard result: " + exception.Message);
        }
        //mostramos Back unicamente al terminar la partida
        backButton.SetActive(true);
        //seleccionamos el boton para consultar el leaderboard
        SelectUIElement(viewLeaderboardButton.gameObject);
        //recuperamos la musica idle
        questionAudioController.ResumeIdleMusic();
    }

    private void UpdateTimerText(float timeRemaining)
    {
        //CeilToInt hace que se vea 1 hasta el ultimo instante y no 0 antes de tiempo
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    //destruir botones de la pregunta anterior y vaciar la lista
    private void ClearAnswerButtons()
    {
        //recorremos al reves para no alterar los indices al destruir
        for (int i = answerButtonsContent.childCount - 1; i >= 0; i--)
        {
            Destroy(answerButtonsContent.GetChild(i).gameObject);
        }
        answerButtons.Clear();
    }

    //abre ventana de puntuaciones desde el panel de resultados
    private void OpenLeaderboard()
    {
        //seleccionar el kahoot que acabamos de jugar
        leaderboardUI.ShowLeaderboardFor(currentKahoot);
        windowManager.OpenHighscores();
    }

    private void SelectUIElement(GameObject selectedObject)
    {
        //comprobamos que exista el EventSystem y el objeto seleccionado
        if (EventSystem.current == null || selectedObject == null)
        { return; }
        //limpiamos la seleccion anterior
        EventSystem.current.SetSelectedGameObject(null);
        //seleccionamos el nuevo elemento
        EventSystem.current.SetSelectedGameObject(selectedObject);
    }
}