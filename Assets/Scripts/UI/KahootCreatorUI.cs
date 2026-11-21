using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KahootCreatorUI : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private KahootSelectorUI kahootSelectorUI;
    [SerializeField] private ExceptionReportManager exceptionReportManager;

    [Header("UI References")]
    [SerializeField] private TMP_InputField kahootTitleInput;
    [SerializeField] private Transform questionsContent;
    [SerializeField] private QuestionEditorUI questionPrefab;
    [SerializeField] private TMP_Text messageText;

    [Header("Buttons")]
    [SerializeField] private Button addQuestionButton;
    [SerializeField] private Button createKahootButton;

    private List<QuestionEditorUI> questionItems = new List<QuestionEditorUI>();

    private void Awake()
    {
        //asignamos las funciones a los botones
        addQuestionButton.onClick.AddListener(AddQuestion);
        createKahootButton.onClick.AddListener(CreateKahoot);
    }

    private void Start()
    {
        AddQuestion(); //crear primera pregunta automaticamente
    }

    private void AddQuestion()
    {
        //instanciamos un nuevo item de pregunta
        QuestionEditorUI question = Instantiate(questionPrefab,questionsContent);

        //configuramos su numero y la funcion para eliminarla
        question.Setup(questionItems.Count + 1, RemoveQuestion);

        questionItems.Add(question);

        UpdateQuestionNumbers();
        
        messageText.text = ""; //limpiar el mensaje anterior
    }

    private void RemoveQuestion(QuestionEditorUI question)
    {
        if (questionItems.Count <= 1) //debe existir minimo una pregunta
        {
            ShowMessage("A Kahoot must have at least one question");
            return;
        }
        questionItems.Remove(question);
        Destroy(question.gameObject);

        UpdateQuestionNumbers();
    }

    private void UpdateQuestionNumbers()
    {
        //actualizar numeracion de todas las preguntas
        for (int i = 0; i < questionItems.Count; i++)
        {
            questionItems[i].UpdateQuestionNumber(i + 1);
        }
    }

    private void CreateKahoot()
    {
        //comprobamos que el titulo no este vacio
        string kahootTitle = kahootTitleInput.text.Trim();

        if (string.IsNullOrWhiteSpace(kahootTitle))
        {
            ShowMessage("Please enter a Kahoot title");
            return;
        }

        //crear el objeto principal del kahoot
        KahootData kahoot = new KahootData();
        kahoot.title = kahootTitle;

        //recuperar y validar todas las preguntas
        foreach (QuestionEditorUI questionItem in questionItems)
        {
            if (!questionItem.TryBuildQuestion
            (out QuestionData question,out string errorMessage))
            {
                ShowMessage(errorMessage);
                return;
            }
            kahoot.questions.Add(question);
        }

        //obtener ruta de los kahoots externos
        string folderPath = Path.Combine(Application.persistentDataPath,"Kahoots");

        try
        {
            //crear carpeta si todavia no existe
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            //obtener un nombre que no sobrescriba otros kahoots
            string filePath = GetUniqueFilePath(folderPath, kahootTitle);

            //convertir el objeto kahoot a formato json
            string jsonContent = JsonUtility.ToJson(kahoot, true);

            File.WriteAllText(filePath, jsonContent); //guardar el archivo
            Debug.Log("Kahoot created successfully: " + filePath);

            //actualizar lista interna de kahoots disponibles del selector
            kahootSelectorUI.RefreshKahoots();
            ShowMessage("Kahoot created successfully!");
        }
        catch (Exception exception)
        {
            Debug.LogError("Error creating Kahoot: " + exception.Message);

            //generamos un informe si ocurre algun error al guardar archivo
            exceptionReportManager.GenerateReport
            ("KahootCreatorUI - Create Kahoot",folderPath,exception);

            ShowMessage("Could not create the Kahoot.");
        }
    }

    private string GetUniqueFilePath(string folderPath, string kahootTitle)
    {
        //eliminar caracteres no permitidos en nombre de archivo
        string safeFileName = kahootTitle; 

        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            safeFileName = safeFileName.Replace(invalidCharacter, '_');
        }
        //evitar nombres que puedan causar problemas en windows
        safeFileName = safeFileName.Trim().TrimEnd('.');

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "Kahoot";
        }

        string filePath = Path.Combine(folderPath, safeFileName + ".json");

        //si existe un archivo con ese mismo nombre
        int counter = 1;

        while (File.Exists(filePath)) // --> añadimos un numero
        {
            filePath = Path.Combine(folderPath,safeFileName+"_"+counter+".json");
            counter++;
        }
        return filePath;
    }

    private void ShowMessage(string message)
    {
        messageText.text = message;
    }
}
