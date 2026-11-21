using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionEditorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questionNumberText;
    [SerializeField] private TMP_InputField questionInput;
    [SerializeField] private TMP_InputField durationInput;

    [Header("Answer References")]
    [SerializeField] private Transform answersContent;
    [SerializeField] private AnswerEditorUI answerPrefab;
    [SerializeField] private ToggleGroup answersToggleGroup;

    [Header("Buttons")]
    [SerializeField] private Button addAnswerButton;
    [SerializeField] private Button removeQuestionButton;

    private List<AnswerEditorUI> answerItems = new List<AnswerEditorUI>();
    private Action<QuestionEditorUI> onRemove;

    private void Awake()
    {
        addAnswerButton.onClick.AddListener(AddAnswer);
        removeQuestionButton.onClick.AddListener(RemoveQuestion);
    }

    public void Setup(int questionNumber, Action<QuestionEditorUI> removeCallback)
    {
        onRemove = removeCallback;

        answersToggleGroup.allowSwitchOff = false; //evitar que se puedan desmarcar todas las respuestas

        UpdateQuestionNumber(questionNumber);

        CreateAnswer(true); //cada pregunta empezara con dos respuestas
        CreateAnswer(false);
    }

    public void UpdateQuestionNumber(int questionNumber)
    {
        questionNumberText.text = "Question " + questionNumber;
    }

    private void AddAnswer()
    {
        CreateAnswer(false);
    }

    private void CreateAnswer(bool isCorrect)
    {
        //instanciamos una nueva respuesta dentro de la pregunta
        AnswerEditorUI answer = Instantiate(answerPrefab, answersContent);
        answer.Setup(answersToggleGroup, RemoveAnswer, isCorrect);
        answerItems.Add(answer);

        UpdateAnswerButtons();
    }

    private void RemoveAnswer(AnswerEditorUI answer)
    {
        //no permitir eliminar respuestas si solo quedan dos
        if (answerItems.Count <= 2) { return; }

        bool wasCorrect = answer.IsCorrect;

        answerItems.Remove(answer);
        Destroy(answer.gameObject);

        //si se ha eliminado la respuesta correcta 
        if (wasCorrect && answerItems.Count > 0)
        {
            answerItems[0].SetCorrect(true); // --> seleccionamos otra
        }
        UpdateAnswerButtons();
    }

    private void UpdateAnswerButtons()
    {
        //desactivar botones para eliminar cuando solo queden dos respuestas
        bool canRemove = answerItems.Count > 2;

        foreach (AnswerEditorUI answer in answerItems)
        {
            answer.SetRemoveButtonInteractable(canRemove);
        }
    }

    private void RemoveQuestion()
    {
        onRemove?.Invoke(this);
    }

    public bool TryBuildQuestion(out QuestionData question, out string errorMessage)
    {
        question = null;
        errorMessage = "";

        //comprobar enunciado
        if (string.IsNullOrWhiteSpace(questionInput.text))
        {
            errorMessage = "A question cannot have an empty statement";
            return false;
        }

        //comprobar que la duracion sea un numero positivo
        if (!int.TryParse(durationInput.text, out int duration) || duration <= 0)
        {
            errorMessage = "Question duration must be a positive number";
            return false;
        }

        //comprobar que existan como minimo dos respuestas
        if (answerItems.Count < 2)
        {
            errorMessage = "Questions must have at least two answers";
            return false;
        }

        QuestionData newQuestion = new QuestionData();

        newQuestion.questionText = questionInput.text.Trim();
        newQuestion.duration = duration;

        int correctAnswers = 0;

        //recuperamos todas las respuestas de esta pregunta
        foreach (AnswerEditorUI answerItem in answerItems)
        {
            AnswerData answer = answerItem.GetAnswerData();

            if (string.IsNullOrWhiteSpace(answer.answerText))
            {
                errorMessage = "Answer options cannot be empty";
                return false;
            }
            if (answer.isCorrect) { correctAnswers++; }

            newQuestion.answers.Add(answer);
        }

        //debe existir exactamente una respuesta correcta
        if (correctAnswers != 1)
        {
            errorMessage = "A question must have only one correct answer";
            return false;
        }
        question = newQuestion;
        return true;
    }
}
