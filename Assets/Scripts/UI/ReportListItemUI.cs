using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReportListItemUI : MonoBehaviour //
{
    [Header("UI References")]
    [SerializeField] private TMP_Text reportNameText;
    [SerializeField] private Button viewButton;

    private string reportFilePath;
    private Action<string> onViewSelected;

    private void Awake()
    {
        //vincular el evento clic del boton con HandleViewClicked
        viewButton.onClick.AddListener(HandleViewClicked);
    }

    // Configura los datos visuales del elemento y guarda la acción a ejecutar al seleccionarlo.
    public void Setup(string filePath, string reportName, Action<string> viewCallback)
    {
        reportFilePath = filePath;
        onViewSelected = viewCallback;

        reportNameText.text = reportName;
    }

    private void HandleViewClicked()
    {
        //avisar a ReportSelectorUI enviandole la ruta del informe
        onViewSelected?.Invoke(reportFilePath);
    }
}
