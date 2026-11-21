using System;
using System.Collections.Generic;

[Serializable]
public class KahootData
{
    public string title; //titulo del Kahoot
    public List<QuestionData> questions = new List<QuestionData>(); //utilizar List<QuestionData> porque el numero de preguntas no sera siempre el mismo
}
