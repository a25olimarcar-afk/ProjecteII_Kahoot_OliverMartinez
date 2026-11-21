using TMPro;
using UnityEngine;

public class ReportViewerUI : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private ExceptionReportManager exceptionReportManager;

    [Header("UI References")]
    [SerializeField] private TMP_Text reportTitleText;
    [SerializeField] private TMP_Text reportContentText;

    public void ShowReport(string filePath)
    {
        try
        {
            //obtenemos el nombre y el contenido del informe
            string reportName = exceptionReportManager.GetReportName(filePath);
            string reportContent = exceptionReportManager.ReadReport(filePath);

            //mostrar la info en pantalla
            reportTitleText.text = reportName;
            reportContentText.text = reportContent;
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Error reading report: " + exception.Message);
            //mostramos un mensaje en la interfaz si el archivo no se puede leer
            reportTitleText.text = "Error";
            reportContentText.text = "Could not read the selected report";
        }
    }
}
