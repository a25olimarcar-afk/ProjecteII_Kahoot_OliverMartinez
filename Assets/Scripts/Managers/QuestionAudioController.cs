using UnityEngine;

public class QuestionAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource idleMusicSource; //musica de fondo cuando ya no hay pregunta activa
    [SerializeField] private AudioSource questionMusicSource; //musica que suena mientras hay una pregunta en curso
    [SerializeField] private AudioSource feedbackAudioSource; //sonido gong que suena al terminar (mostrar feedback) 

    [Header("Question Music Clips")]
    [SerializeField] private AudioClip questionMusic5;
    [SerializeField] private AudioClip questionMusic10;
    [SerializeField] private AudioClip questionMusic20;
    [SerializeField] private AudioClip questionMusic30;
    [SerializeField] private AudioClip questionMusic60;

    //devuelve el clip que corresponde a la duracion de la pregunta
    private AudioClip GetQuestionMusic(int duration)
    {
        if (duration >= 60) //cualquier duracion superior a 60 sigue utilizando ese clip
        { return questionMusic60; }

        else if (duration >= 30)
        { return questionMusic30; } //questionMusic30 para 30 o mas segundos

        else if (duration >= 20)
        { return questionMusic20; } //questionMusic20 para 20 o mas segundos

        else if (duration >= 10)
        { return questionMusic10; } //questionMusic10 para 10 o mas segundos

        else
        { return questionMusic5; } //cualquier duracion menor a 10 usa la musica corta
    }

    //detener la musica idle y reproducir la musica de la pregunta
    public void PlayQuestionMusic(int duration)
    {
        AudioClip selectedClip = GetQuestionMusic(duration);

        //detenemos la musica idle
        if (idleMusicSource != null)
        {
            idleMusicSource.Stop();
        }

        //reproducimos la musica seleccionada
        questionMusicSource.Stop(); //parar cualquier clip anterior antes de cambiarlo
        questionMusicSource.clip = selectedClip; //referenciar el clip 
        questionMusicSource.loop = false; //solo debe sonar una vez (suelen durar lo mismo que la pregunta)
        questionMusicSource.Play(); //reproducir el clip seleccionado
    }

    //reproducir el sonido de feedback
    public void PlayFeedbackSound()
    {
        if (feedbackAudioSource != null)
        {
            feedbackAudioSource.Play();
        }
    }

    //detener la musica de pregunta
    public void StopQuestionMusic()
    {
        questionMusicSource.Stop();
    }

    //para la musica de la pregunta pero luego vuelve a poner idle en bucle
    public void ResumeIdleMusic()
    {
        StopQuestionMusic();

        //solo arrancamos si no estaba sonando ya, asi no se reinicia
        if (idleMusicSource != null && !idleMusicSource.isPlaying)
        {
            idleMusicSource.loop = true;
            idleMusicSource.Play();
        }
    }
}