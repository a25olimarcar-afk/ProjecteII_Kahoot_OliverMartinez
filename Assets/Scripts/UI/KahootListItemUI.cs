using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KahootListItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;

    private Action<KahootEntry> onSelected;

    //los datos del kahoot asociados a este elemento de la lista
    public KahootEntry Entry { get; private set; }

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }
 
    //inicializa el elemento con sus datos y define que ocurre al seleccionarlo
    public void Setup(KahootEntry entry, Action<KahootEntry> selectCallback) 
    {
        Entry = entry; //objeto con la informacion del cuestionario
        onSelected = selectCallback; //metodo que recibe el entry seleccionado al hacer clic

        titleText.text = entry.displayName; //titulo del kahoot saldra como texto en el boton
    }

    private void HandleClick()
    {
        //notifica a KahootSelectorUI que elemento especifico fue pulsado
        onSelected?.Invoke(Entry);
    }
}
