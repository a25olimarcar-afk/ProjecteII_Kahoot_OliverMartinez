using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text answerText;

    private Action onClick;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Setup(string text, Action callback) //actualiza el texto del boton con el texto de la respuesta
    {
        answerText.text = text; //texto de respuesta que se mostrara en el boton
        onClick = callback; //metodo que se invocara cuando el jugdor elija la opcion
    }

    private void HandleClick()
    {
        onClick?.Invoke(); //invoca el callback solo si ha sido asignado previamente en Setup
        //permite reutilizar el mismo boton para distintas preguntas
    }

    public void SetInteractable(bool interactable) //habilita/deshabilita la interaccion del boton 
    {
        button.interactable = interactable; //util porque evita multiples clics o interactuar cuando no es visible
    }
}
