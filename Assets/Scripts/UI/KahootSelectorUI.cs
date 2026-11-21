using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KahootSelectorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KahootLoader kahootLoader;
    [SerializeField] private Transform kahootsContent;
    [SerializeField] private KahootListItemUI kahootItemPrefab;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button playButton;
    [SerializeField] private WindowManager windowManager;

    private List<KahootListItemUI> kahootItems = new List<KahootListItemUI>();
    private KahootEntry selectedKahoot;

    //cuestionario elegido actualmente por el usuario
    //(leido por KahootGameUI que es el gestor de la partida)
    public KahootEntry SelectedKahoot { get { return selectedKahoot; }}

    //nombre introducido por el usuario
    public string PlayerName { get { return usernameInput.text.Trim(); } } //Trim = sin espacios sobrantes

    private void Start()
    {
        //suscribimos los controles de interfaz a sus metodos correspondientes
        usernameInput.onValueChanged.AddListener(OnUsernameChanged);
        playButton.onClick.AddListener(StartSelectedKahoot);

        PopulateKahoots(); //limpiar lista y generar botones
        UpdatePlayButton(); //aseguramos el estado inicial del boton al arrancar la escena
    }

    //se encarga de limpiar la lista visual y generar botones por cada kahoot encontrado
    private void PopulateKahoots()
    {
        //recorrer en orden inverso para destruir los hijos de forma segura
        for (int i = kahootsContent.childCount - 1; i >= 0; i--)
        {
            Destroy(kahootsContent.GetChild(i).gameObject);
        }

        kahootItems.Clear();
        selectedKahoot = null;

        //instanciar cada elemento y le pasamos SelectKahoot como callback
        foreach (KahootEntry entry in kahootLoader.LoadedKahoots)
        {
            KahootListItemUI item = Instantiate(kahootItemPrefab, kahootsContent);
            item.Setup(entry, SelectKahoot);
            kahootItems.Add(item);
        }
        //como selectedKahoot se acaba de poner en null
        UpdatePlayButton(); //actualizamos el boton para forzar su desactivacion
    }

    //callback ejecutado cuando el usuario pulsa un elemento de la lista
    private void SelectKahoot(KahootEntry entry)
    {
        selectedKahoot = entry;
        UpdatePlayButton(); //mirar si ya podemos habilitar el boton y jugar --> kahoot seleccionado
    }

    private void OnUsernameChanged(string username)
    {
        UpdatePlayButton(); //mirar si ya podemos habilitar el boton y jugar --> nombre de usuario introducido
    }

    private void UpdatePlayButton()
    {
        //entramos al metodo pero solo habilitamos el boton para jugar si:
        playButton.interactable =
            selectedKahoot != null &&                       //hay un kahoot elegido
            !string.IsNullOrWhiteSpace(usernameInput.text); //hay un nombre valido
    }

    private void StartSelectedKahoot()
    {
        //doble validacion de seguridad antes de cambiar de pantalla
        if (selectedKahoot == null || string.IsNullOrWhiteSpace(PlayerName))
        { return; }
        //si todo esta bien iniciamos el juego
        windowManager.OpenKahootGame();
    }

    //recarga los kahoots disponibles desde el disco y actualiza la lista en pantalla
    public void RefreshKahoots()
    {
        kahootLoader.LoadAllKahoots();
        PopulateKahoots();
    }
}
