using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderboardManager : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private ExceptionReportManager exceptionReportManager;

    private string leaderboardFolderPath;

    private void Awake()
    {
        //definimos una carpeta donde se almacenaran los leaderboards
        leaderboardFolderPath = Path.Combine(Application.persistentDataPath,"Leaderboards");
    }

    public void SaveResult(KahootEntry kahoot, LeaderboardResult result)
    {
        string filePath = GetLeaderboardFilePath(kahoot);

        try
        {
            //creamos la carpeta si es que todavia no existe
            if (!Directory.Exists(leaderboardFolderPath)) { Directory.CreateDirectory(leaderboardFolderPath); }

            XmlDocument xmlDocument = new XmlDocument();

            if (File.Exists(filePath)) //si ya existe el archivo --> cargamos sus resultados anteriores
            {
                xmlDocument.Load(filePath);
            }
            else //si no existe --> creamos un xml nuevo
            {
                XmlDeclaration declaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDocument.AppendChild(declaration);

                XmlElement root = xmlDocument.CreateElement("Leaderboard");

                root.SetAttribute("KahootTitle", kahoot.displayName);
                xmlDocument.AppendChild(root);
            }
            //si el documento esta vacio o no es un Leaderboard --> se considera invalido
            if (xmlDocument.DocumentElement == null || xmlDocument.DocumentElement.Name != "Leaderboard")
            {
                throw new XmlException("Invalid leaderboard XML structure.");
            }
            //crear otro elemento solo para el resultado
            XmlElement resultElement = xmlDocument.CreateElement("Result");

            AddElement(xmlDocument, resultElement, "PlayerName", result.playerName);
            AddElement(xmlDocument, resultElement, "Score", result.score.ToString());
            AddElement(xmlDocument, resultElement, "CorrectAnswers", result.correctAnswers.ToString());
            AddElement(xmlDocument, resultElement, "TotalQuestions", result.totalQuestions.ToString());
            AddElement(xmlDocument, resultElement, "CompletionTime", result.completionTime.ToString("F2", CultureInfo.InvariantCulture)); //2 decimales usando punto universal
            AddElement(xmlDocument, resultElement, "Date", result.date);

            xmlDocument.DocumentElement.AppendChild(resultElement);
            xmlDocument.Save(filePath); //guardamos todos esos resultados en el archivo xml

            Debug.Log("Leaderboard result saved: " + filePath);
        }
        catch (Exception exception)
        {
            Debug.LogError("Error saving leaderboard: " + exception.Message);

            exceptionReportManager.GenerateReport
            ("LeaderboardManager - Save Result",filePath,exception);

            throw; //despues de generar informe volver a lanzar la excepcion para que KahootGameUI sepa que la operacion ha fallado
        }
    }

    public List<LeaderboardResult> LoadResults(KahootEntry kahoot)
    {
        List<LeaderboardResult> results = new List<LeaderboardResult>();

        string filePath = GetLeaderboardFilePath(kahoot);

        //si no existen resultados devolvemos una lista vacia
        if (!File.Exists(filePath)) { return results; }

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            //si el documento esta vacio o no es un Leaderboard --> se considera invalido
            if (xmlDocument.DocumentElement == null || xmlDocument.DocumentElement.Name != "Leaderboard")
            {
                throw new XmlException("Invalid leaderboard XML structure.");
            }

            //recuperamos todos los resultados almacenados
            XmlNodeList resultNodes = xmlDocument.DocumentElement.SelectNodes("Result");

            foreach (XmlNode node in resultNodes)
            {
                LeaderboardResult result = new LeaderboardResult();

                //extrae y convierte los datos del xml usando formato numerico universal
                result.playerName = GetElementValue(node, "PlayerName");
                result.score = int.Parse(GetElementValue(node, "Score"), CultureInfo.InvariantCulture);
                result.correctAnswers = int.Parse(GetElementValue(node, "CorrectAnswers"), CultureInfo.InvariantCulture);
                result.totalQuestions = int.Parse(GetElementValue(node, "TotalQuestions"), CultureInfo.InvariantCulture);
                result.completionTime = float.Parse(GetElementValue(node, "CompletionTime"), CultureInfo.InvariantCulture);
                result.date = GetElementValue(node, "Date");

                results.Add(result);
            }
            results.Sort(CompareResults); //ordenamos usando el metodo comparador
            return results;
        }
        catch (Exception exception)
        {
            Debug.LogError("Error loading leaderboard: " + exception.Message);

            exceptionReportManager.GenerateReport
            ("LeaderboardManager - Load Results",filePath,exception);

            throw; //volver a lanzar la excepcion para que LeaderboardUI sepa que ha fallado
        }
    }

    //ordenamos primero por puntuacion y despues por tiempo
    private int CompareResults(LeaderboardResult a, LeaderboardResult b)
    {
        int scoreComparison = b.score.CompareTo(a.score);
        if (scoreComparison != 0) { return scoreComparison; }

        return a.completionTime.CompareTo(b.completionTime);
    }

    private void AddElement(XmlDocument document, XmlElement parent, string elementName, string value)
    {
        //creamos un elemento xml y asignamos su contenido
        XmlElement element = document.CreateElement(elementName);
        element.InnerText = value;
        parent.AppendChild(element);
    }

    private string GetElementValue(XmlNode parent, string elementName)
    {
        //recuperar el contenido de un elemento xml
        XmlNode element = parent[elementName];

        if (element == null)
        { throw new FormatException("Missing XML element: " + elementName); }

        return element.InnerText;
    }

    private string GetLeaderboardFilePath(KahootEntry kahoot)
    {
        //pasamos el origen para evitar conflictos entre archivos con el mismo nombre
        string prefix = kahoot.isExternal ? "External_" : "Default_";
        //si es true se le añade "External_" si es false "Default_"

        string safeFileName = kahoot.fileName; //nombre del archivo

        //sustituimos cualquier caracter prohibido por un guion bajo
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            safeFileName = safeFileName.Replace(invalidCharacter, '_');
        }
        return Path.Combine(leaderboardFolderPath, prefix + safeFileName + ".xml");
    }
}
