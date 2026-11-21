using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerEditorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Toggle correctToggle;
    [SerializeField] private Button removeButton;

    private Action<AnswerEditorUI> onRemove;

    public bool IsCorrect { get { return correctToggle.isOn; } } //obtener si es correcta o no

    private void Awake()
    {
        removeButton.onClick.AddListener(RemoveAnswer);
    }

    public void Setup(ToggleGroup toggleGroup, Action<AnswerEditorUI> removeCallback, bool isCorrect)
    {
        correctToggle.group = toggleGroup; //asignar el grupo para permitir una unica respuesta correcta
        correctToggle.isOn = isCorrect; //marcar la casilla segun el valor recibido

        onRemove = removeCallback;
        answerInput.text = "";
    }

    public AnswerData GetAnswerData()
    {
        AnswerData answer = new AnswerData(); //crear objeto AnswerData con la informacion introducida

        answer.answerText = answerInput.text.Trim();
        answer.isCorrect = correctToggle.isOn;

        return answer;
    }

    public void SetCorrect(bool isCorrect)
    {
        correctToggle.isOn = isCorrect;
    }

    public void SetRemoveButtonInteractable(bool interactable)
    {
        removeButton.interactable = interactable;
    }

    private void RemoveAnswer()
    {
        onRemove?.Invoke(this);
    }
}
