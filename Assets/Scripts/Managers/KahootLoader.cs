using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KahootLoader : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private ExceptionReportManager exceptionReportManager;

    //lista para almacenar todos los kahoots que esten disponibles
    private List<KahootEntry> loadedKahoots = new List<KahootEntry>();

    public List<KahootEntry> LoadedKahoots //propiedad solo lectura
    {
        get { return loadedKahoots; }
    }

    private void Awake()
    {
        LoadAllKahoots(); //iniciar proceso de carga de todos los kahoots disponibles
        Debug.Log("Total Kahoots found: " + loadedKahoots.Count);

        //mostramos todos los cuestionarios encontrados por su nombre
        for (int i = 0; i < loadedKahoots.Count; i++)
        {
            Debug.Log((i + 1) + ". " + loadedKahoots[i].displayName);
        }
    }

    //este metodo limpia la lista actual y carga tanto los Kahoots internos como los externos
    public void LoadAllKahoots() 
    {
        loadedKahoots.Clear(); //vaciamos lista para evitar duplicar elementos

        LoadDefaultKahoots(); //cargar kahoots de Resources (interno)
        LoadExternalKahoots(); //cargar tambien kahoots guardados en el almacenamiento local
    }

    private void LoadDefaultKahoots()
    {
        //cargamos todos los archivos .json incluidos en Resources (nuestros predeterminados)
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Kahoots");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            //pasamos la info al metodo AddKahoot, especificamos que no es externo
            AddKahoot(jsonFile.text, jsonFile.name, false, "");
        }
    }

    private void LoadExternalKahoots()
    {
        //obtener la ruta de la carpeta externa (Application.persistentDataPath)
        string folderPath = Path.Combine(Application.persistentDataPath, "Kahoots");

        try
        {
            //crear la carpeta si no existe aun
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            //obtenemos todos los archivos .json de la carpeta
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json"); //*.json = solo buscar extension .json

            foreach (string filePath in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath); //leemos el contenido de cada archivo
                    string fileName = Path.GetFileNameWithoutExtension(filePath); //obtener el nombre sin la extension

                    //pasamos la info a AddKahoot, este si es externo
                    AddKahoot(jsonContent, fileName, true, filePath);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Error reading JSON file: " + exception.Message);

                    exceptionReportManager.GenerateReport
                    ("KahootLoader - External JSON",filePath,exception);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("Error accessing Kahoots folder: " + exception.Message);

            exceptionReportManager.GenerateReport
            ("KahootLoader - External Kahoots Folder",folderPath,exception);
        }
        Debug.Log("External Kahoots Path: " + folderPath);
    }

    //este metodo intenta deserializar texto .json, validar su estructura y insertarlo en la lista de Kahoots
    private void AddKahoot(string jsonContent, string fileName, bool isExternal, string filePath)
    {
        try //usar try-catch para prevenir que un .json corrupto rompa la ejecucion del juego
        {
            //convertimos el contenido .json en un objeto KahootData
            KahootData kahoot = JsonUtility.FromJson<KahootData>(jsonContent);

            if (!ValidateKahoot(kahoot)) //validar que el objeto cumpla con las reglas minimas
            {
                Debug.LogError("Invalid Kahoot file: " + fileName);

                //si los datos no cumplen las reglas, generamos un informe
                FormatException formatException = new FormatException
                ("The JSON file does not contain a valid Kahoot structure.");

                string affectedFile = isExternal ? filePath : "Resources/Kahoots/"+fileName+".json";

                exceptionReportManager.GenerateReport
                ("KahootLoader - Kahoot Validation",affectedFile,formatException);

                return;
            }

            //crear entrada con los datos y el origen del kahoot
            KahootEntry entry = new KahootEntry();
            //le pasamos todos los datos
            entry.data = kahoot;
            entry.displayName = kahoot.title;
            entry.fileName = fileName;
            entry.isExternal = isExternal;
            entry.filePath = filePath;

            loadedKahoots.Add(entry); //añadir a la lista

            Debug.Log("Kahoot loaded successfully: " + kahoot.title);
        }
        //si el.json esta mal formateado o le faltan campos criticos, capturamos el error
        catch (Exception exception)
        {
            Debug.LogError("Error loading Kahoot " + fileName + ": " + exception.Message);

            string affectedFile = isExternal ? filePath : "Resources/Kahoots/"+fileName+".json";

            exceptionReportManager.GenerateReport
            ("KahootLoader - JSON Deserialization",affectedFile,exception);
        }
    }

    //este metodo realiza un control de reglas sobre los datos del kahoot
    //se asegura de que pueda ser jugable, devuelve true si todo esta correcto
    private bool ValidateKahoot(KahootData kahoot)
    {
        //REGLA1: el objeto no puede ser nulo y el titulo no puede estar vacio
        if (kahoot == null || string.IsNullOrWhiteSpace(kahoot.title))
        { return false; }

        //REGLA2: el kahoot debe contener obligatoriamente al menos una pregunta
        if (kahoot.questions == null || kahoot.questions.Count == 0)
        { return false; }

        //recorrer y analizar cada pregunta del cuestionario
        foreach (QuestionData question in kahoot.questions)
        {
            //REGLA3: la pregunta no puede ser null, debe tener su texto de pregunta y tambien una lista de respuestas
            if (question == null ||
                string.IsNullOrWhiteSpace(question.questionText) ||
                question.answers == null ||
                question.answers.Count == 0)
            { return false; }

            int correctAnswers = 0; //contador para verificar respuestas correctas en una pregunta 

            foreach (AnswerData answer in question.answers) //recorrer todas las respuestas de la pregunta actual
            {
                //REGLA4: la respuesta no debe ser null y el texto de la opcion no puede estar en blanco
                if (answer == null || string.IsNullOrWhiteSpace(answer.answerText))
                { return false; }

                //si la respuesta esta marcada como verdadera, la sumamos al contador correctAnswers
                if (answer.isCorrect) { correctAnswers++; }
            }

            //REGLA5: el tiempo limite para responder no puede ser menor o igual a 0
            if (question.duration <= 0) //por lo tanto el tiempo minimo aceptado es 1 segundo
            { return false; }

            //REGLA6: utilizamos el modo "Examen" original de Kahoot, lo que equivale a "seleccion unica"
            //significa que si hay mas de una respuesta correcta, el archivo se considerara invalido
            if (correctAnswers != 1) //de esta forma tampoco se permiten 0 respuestas
            { return false; }
        }

        //si supera todas estas condiciones se devuelve true
        return true; //significa que el archivo es seguro
    }
}