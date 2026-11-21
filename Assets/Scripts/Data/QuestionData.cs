using System;
using System.Collections.Generic;

[Serializable]
public class QuestionData
{
    public string questionText; //contenido de la pregunta
    public int duration; //duracion de la pregunta
    public List<AnswerData> answers = new List<AnswerData>(); //utilizar List<AnswerData> porque el numero de respuestas no sera siempre el mismo
}
