using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private KahootLoader kahootLoader;
    [SerializeField] private LeaderboardManager leaderboardManager;

    [Header("UI References")]
    [SerializeField] private TMP_Dropdown kahootDropdown;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private LeaderboardItemUI leaderboardItemPrefab;
    [SerializeField] private TMP_Text messageText;

    private List<KahootEntry> availableKahoots = new List<KahootEntry>();
    private KahootEntry requestedKahoot;

    private void OnEnable() //suscribirse al cambio de opcion del Dropdown y cargar lista inicial
    {
        kahootDropdown.onValueChanged.AddListener(OnKahootSelected);
        PopulateKahoots();
    }

    private void OnDisable() //limpiar suscripciones al cerrar la ventana para evitar llamadas duplicadas
    {
        kahootDropdown.onValueChanged.RemoveListener(OnKahootSelected);
    }

    public void ShowLeaderboardFor(KahootEntry kahoot)
    {
        requestedKahoot = kahoot; //guardar el kahoot que se quiere consultar

        if (isActiveAndEnabled) //si la ventana esta activa actualizamos el contenido
        {
            PopulateKahoots();
        }
    }

    private void PopulateKahoots()
    {
        availableKahoots = kahootLoader.LoadedKahoots;

        kahootDropdown.ClearOptions();

        List<string> options = new List<string>();

        //generamos una opcion para cada kahoot disponible
        foreach (KahootEntry entry in availableKahoots)
        {
            string source = entry.isExternal ? " (External)" : " (Default)"; //indicar si es externo
            options.Add(entry.displayName + source);
        }

        kahootDropdown.AddOptions(options);

        //si no hay kahoots bloqueamos el desplegable y mostramos un aviso
        if (availableKahoots.Count == 0)
        {
            kahootDropdown.interactable = false;
            ClearLeaderboard();
            ShowMessage("No Kahoots available");
            return;
        }

        kahootDropdown.interactable = true;

        //buscar si existe un kahoot solicitado previamente
        int selectedIndex = 0;

        if (requestedKahoot != null)
        {
            int requestedIndex = availableKahoots.IndexOf(requestedKahoot);
            if (requestedIndex >= 0) { selectedIndex = requestedIndex; }
        }

        //actualizar el Dropdown sin ejecutar dos veces el evento
        kahootDropdown.SetValueWithoutNotify(selectedIndex);
        kahootDropdown.RefreshShownValue();

        requestedKahoot = null;

        LoadLeaderboard(selectedIndex);
    }

    private void OnKahootSelected(int index)
    {
        LoadLeaderboard(index);
    }

    private void LoadLeaderboard(int index)
    {
        //evitar errores si el indice seleccionado esta fuera de los limites
        if (index < 0 || index >= availableKahoots.Count) { return; }

        ClearLeaderboard();

        try
        {
            //recuperar los resultados del kahoot seleccionado
            List<LeaderboardResult> results = leaderboardManager.LoadResults(availableKahoots[index]);

            if (results.Count == 0) //avisar con un mensaje si el kahoot no tiene resultados
            {
                ShowMessage("No results available for this Kahoot");
                return;
            }

            messageText.gameObject.SetActive(false);

            for (int i = 0; i < results.Count; i++) //generar una fila por cada resultado
            {
                LeaderboardItemUI item =
                    Instantiate(leaderboardItemPrefab, leaderboardContent);

                item.Setup(i + 1, results[i]); //i + 1 convierte el indice 0 en posicion #1
            }
        }
        catch (Exception exception) //capturar errores de lectura si el archivo xml esta mal formado
        {
            Debug.LogError("Could not load leaderboard: " + exception.Message);
            ShowMessage("Could not load leaderboard");
        }
    }

    private void ClearLeaderboard() //eliminamos todas las filas anteriores
    {
        //recorremos los hijos en orden inverso para destruirlos sin alterar los indices
        for (int i = leaderboardContent.childCount - 1; i >= 0; i--)
        {
            Destroy(leaderboardContent.GetChild(i).gameObject);
        }
    }

    private void ShowMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }
}
