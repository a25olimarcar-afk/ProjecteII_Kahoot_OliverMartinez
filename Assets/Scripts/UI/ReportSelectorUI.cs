using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReportSelectorUI : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private ExceptionReportManager exceptionReportManager;
    [SerializeField] private ReportViewerUI reportViewer;
    [SerializeField] private WindowManager windowManager;

    [Header("UI References")]
    [SerializeField] private Transform reportContent;
    [SerializeField] private ReportListItemUI reportItemPrefab;
    [SerializeField] private TMP_Text messageText;

    public void OpenReports() //abrir el selector y actualizar su contenido
    {
        PopulateReports();
        windowManager.OpenReportSelector();
    }

    private void PopulateReports()
    {
        ClearReports();

        try
        {
            //recuperar todos los informes disponibles
            List<string> reports = exceptionReportManager.GetReports();

            if (reports.Count == 0)
            {
                ShowMessage("No reports available");
                return;
            }

            messageText.gameObject.SetActive(false);

            //crear una fila para cada informe encontrado
            foreach (string filePath in reports)
            {
                ReportListItemUI item = Instantiate(reportItemPrefab,reportContent);

                string reportName = exceptionReportManager.GetReportName(filePath);

                item.Setup(filePath, reportName, OpenSelectedReport);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("Error loading reports: " + exception.Message);
            ShowMessage("Could not load reports");
        }
    }

    private void OpenSelectedReport(string filePath)
    {
        reportViewer.ShowReport(filePath); //mostrar el informe seleccionado
        windowManager.OpenReportViewer(); //abrir la ventana del visor
    }

    private void ClearReports()
    {
        for (int i = reportContent.childCount - 1; i >= 0; i--)
        {
            Destroy(reportContent.GetChild(i).gameObject);
        }
    }

    private void ShowMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }
}
