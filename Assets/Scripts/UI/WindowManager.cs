using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    public enum WindowType //tipos de ventanas disponibles
    {
        MainMenu,
        KahootSelector,
        KahootGame,
        Highscores,
        ReportSelector,
        About,
        ReportViewer,
        KahootCreator
    }

    [Header("Window References")]
    [SerializeField] private RectTransform windowContainer;
    [SerializeField] private RectTransform[] panels; //todos los paneles de nuestras ventanas

    [Header("Navigation References")]
    [SerializeField] private GameObject[] firstSelectedObjects;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.0f; //duracion de la transicion entre ventanas
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); //curva que controla la animacion de transicion

    private CanvasGroup[] canvasGroups; //almacenar los canvas group de cada panel 
    private int currentWindow = 0; //ventana actualmente mostrada
    private bool isTransitioning = false; //variable que evita iniciar varias transiciones a la vez

    private void Awake()
    {
        canvasGroups = new CanvasGroup[panels.Length]; //crear array de CanvasGroup

        for (int i = 0; i < panels.Length; i++)
        {
            //obtener CanvasGroup del panel
            canvasGroups[i] = panels[i].GetComponent<CanvasGroup>();

            //colocar todos los paneles en posicion inicial
            panels[i].anchoredPosition = Vector2.zero;

            //comprobar si es la ventana inicial y activarla
            bool isInitialWindow = i == (int)WindowType.MainMenu;
            panels[i].gameObject.SetActive(isInitialWindow);

            //configurar la visibilidad y interaccion inicial
            canvasGroups[i].alpha = isInitialWindow ? 1 : 0; //si es true pone 1, sino 0
            canvasGroups[i].interactable = isInitialWindow;
            canvasGroups[i].blocksRaycasts = isInitialWindow;
        }
    }

    private void Start()
    {
        //seleccionamos el primer elemento de la ventana inicial
        SelectWindowElement(currentWindow);
    }

    //metodos publicos para enlazar con los botones
    //--> utilizamos el evento OnClick() por el inspector
    public void OpenMainMenu() { OpenWindow(WindowType.MainMenu); }
    public void OpenKahootSelector() { OpenWindow(WindowType.KahootSelector); }
    public void OpenKahootGame() { OpenWindow(WindowType.KahootGame); }
    public void OpenHighscores() { OpenWindow(WindowType.Highscores); }
    public void OpenReportSelector() { OpenWindow(WindowType.ReportSelector); }
    public void OpenAbout() { OpenWindow(WindowType.About); }
    public void OpenReportViewer() { OpenWindow(WindowType.ReportViewer); }
    public void OpenKahootCreator() { OpenWindow(WindowType.KahootCreator); }

    //logica de transiciones entre paneles
    public void OpenWindow(WindowType window)
    {
        int nextWindow = (int)window;
        //ignorar si ya esta abierta o hay una transicion en curso
        if (isTransitioning || nextWindow == currentWindow) return;
        //empezar corrutina de fade alpha y movimiento
        StartCoroutine(TransitionTo(nextWindow));
    }

    private IEnumerator TransitionTo(int nextWindow)
    {
        isTransitioning = true; //bloquear nuevas transiciones

        RectTransform currentPanel = panels[currentWindow];
        RectTransform nextPanel = panels[nextWindow];

        CanvasGroup currentGroup = canvasGroups[currentWindow];
        CanvasGroup nextGroup = canvasGroups[nextWindow];

        //altura del contenedor para el desplazamiento
        float height = windowContainer.rect.height;

        //posicion inicial y final de la animacion
        Vector2 startPosition = new Vector2(0, -height);
        Vector2 endPosition = new Vector2(0, height);

        //preparar panel entrante
        nextPanel.anchoredPosition = startPosition;
        nextPanel.gameObject.SetActive(true);

        //desactivar interaccion del panel actual
        currentGroup.interactable = false;
        currentGroup.blocksRaycasts = false;

        //limpiar la seleccion del panel anterior
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        //inicializar estado visual del nuevo panel
        nextGroup.alpha = 0;
        nextGroup.interactable = false;
        nextGroup.blocksRaycasts = false;

        float elapsedTime = 0;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            //progreso normalizado de la animacion
            float time = Mathf.Clamp01(elapsedTime / transitionDuration);
            float curveValue = transitionCurve.Evaluate(time);

            //mover panel actual hacia arriba
            currentPanel.anchoredPosition =
                Vector2.Lerp(Vector2.zero, endPosition, curveValue);

            //mover panel nuevo hacia el centro
            nextPanel.anchoredPosition =
                Vector2.Lerp(startPosition, Vector2.zero, curveValue);

            //fundido de opacidad
            currentGroup.alpha = 1 - curveValue;
            nextGroup.alpha = curveValue;

            yield return null;
        }

        //ocultar panel anterior
        currentGroup.alpha = 0;
        currentPanel.anchoredPosition = Vector2.zero;
        currentPanel.gameObject.SetActive(false);

        //activar panel nuevo
        nextGroup.alpha = 1;
        nextGroup.interactable = true;
        nextGroup.blocksRaycasts = true;
        nextPanel.anchoredPosition = Vector2.zero;

        currentWindow = nextWindow; //actualizar ventana actual
        SelectWindowElement(nextWindow); //restaurar el foco en la nueva ventana
        isTransitioning = false; //permitir nuevas transiciones
    }
    private void SelectWindowElement(int windowIndex)
    {
        GameObject selectedObject = null;

        //recuperar el elemento configurado en el inspector
        if (firstSelectedObjects != null &&
            windowIndex < firstSelectedObjects.Length)
        {
            GameObject configuredObject = firstSelectedObjects[windowIndex];

            //comprobar que el elemento sea valido
            if (IsValidSelection(configuredObject, panels[windowIndex]))
            {
                selectedObject = configuredObject;
            }
        }

        //si no hay un elemento asignado, buscamos uno automaticamente
        if (selectedObject == null)
        {
            selectedObject = FindFirstSelectable(panels[windowIndex]);
        }

        //limpiamos cualquier seleccion anterior
        EventSystem.current.SetSelectedGameObject(null);

        //establecemos el nuevo elemento seleccionado
        if (selectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(selectedObject);
        }
        else
        {
            Debug.LogWarning(
                "No selectable element found in window: " +
                ((WindowType)windowIndex)
            );
        }
    }

    private bool IsValidSelection(GameObject target, RectTransform panel)
    {
        if (target == null || !target.activeInHierarchy)
        { return false; }

        //comprobamos que el elemento pertenezca al panel correspondiente
        if (target.transform != panel && !target.transform.IsChildOf(panel))
        { return false; }

        Selectable selectable = target.GetComponent<Selectable>();

        return selectable != null && selectable.IsInteractable();
    }

    private GameObject FindFirstSelectable(RectTransform panel)
    {
        //recuperamos todos los elementos interactivos del panel
        Selectable[] selectables = panel.GetComponentsInChildren<Selectable>(true);

        foreach (Selectable selectable in selectables)
        {
            //buscamos el primer elemento activo e interactuable
            if (IsValidSelection(selectable.gameObject, panel))
            { return selectable.gameObject; }
        }
        return null;
    }
}