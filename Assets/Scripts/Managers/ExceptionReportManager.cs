using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ExceptionReportManager : MonoBehaviour
{
    private string reportsFolderPath;

    private void Awake()
    {
        EnsureReportsFolder();
    }

    public void GenerateReport(string source, string affectedFile, Exception exception)
    {
        EnsureReportsFolder();
        try
        {
            DateTime currentDate = DateTime.Now; //obtener fecha y hora actuales

            //generar ruta completa con nombre del archivo y marca de tiempo
            string timestamp = currentDate.ToString("yyyyMMdd_HHmmss_fff");
            string fileName = "Error_" + timestamp + ".txt";
            string filePath = Path.Combine(reportsFolderPath,fileName);

            //redacta el contenido del informe
            StringBuilder report = new StringBuilder();

            report.AppendLine("KAHOOT APPLICATION - EXCEPTION REPORT");
            report.AppendLine("-------------------------------------");
            report.AppendLine();
            report.AppendLine("Date: " + currentDate.ToString("yyyy-MM-dd"));
            report.AppendLine("Time: " + currentDate.ToString("HH:mm:ss"));
            report.AppendLine("Source: " + source);
            report.AppendLine("Affected file: " + affectedFile);
            report.AppendLine();
            report.AppendLine("------------ DESCRIPTION ------------");
            report.AppendLine("Encountered a problem while processing the file.");
            report.AppendLine();
            report.AppendLine("----------- ERROR DETAILS -----------");
            report.AppendLine("Exception type: " + exception.GetType().Name);
            report.AppendLine("Message: " + exception.Message);
            report.AppendLine();
            report.AppendLine("--------- TECHNICAL DETAILS ---------");
            report.AppendLine(exception.StackTrace);

            //guardar el informe como archivo de texto
            File.WriteAllText(filePath, report.ToString());

            Debug.Log("Exception report generated: " + filePath);
        }
        catch (Exception reportException)
        {
            Debug.LogError("Could not generate exception report: " + reportException.Message);
        }
    }

    public List<string> GetReports()
    {
        EnsureReportsFolder();

        //recuperamos todos los archivos de informes
        List<string> reports = new List<string>();

        //crear carpeta si todavia no existe
        if (!Directory.Exists(reportsFolderPath)) { Directory.CreateDirectory(reportsFolderPath); }

        //rutas de todos los archivos .txt encontrados en la carpeta
        string[] files = Directory.GetFiles(reportsFolderPath, "*.txt");
        reports.AddRange(files);

        //ordenamos los informes usando el metodo de comparacion
        reports.Sort(CompareReportsByDate);

        return reports;
    }

    //mostramos primero los informes mas recientes
    private int CompareReportsByDate(string reportA, string reportB)
    {
        DateTime dateA = File.GetCreationTime(reportA);
        DateTime dateB = File.GetCreationTime(reportB);

        //comparar B con A para que los mas recientes aparezcan siempre primero
        return dateB.CompareTo(dateA);
    }

    public string ReadReport(string filePath)
    {
        return File.ReadAllText(filePath); //leer el contenido de un informe seleccionado
    }

    public string GetReportName(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath); //recuperar solo el nombre del report
    }

    private void EnsureReportsFolder()
    {
        //comprobamos que la ruta ya este inicializada
        if (string.IsNullOrEmpty(reportsFolderPath))
        {
            //define la carpeta donde se guardaran los informes
            reportsFolderPath = Path.Combine(Application.persistentDataPath,"Reports");
        }
        //crear carpeta si todavia no existe
        if (!Directory.Exists(reportsFolderPath)) { Directory.CreateDirectory(reportsFolderPath); }
    }
}
