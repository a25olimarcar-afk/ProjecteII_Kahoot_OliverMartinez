using TMPro;
using UnityEngine;

public class LeaderboardItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;

    //rellena cada fila con los datos de un resultado
    //Setup() recibe la posicion del jugador y su resultado
    public void Setup(int rank, LeaderboardResult result)
    {
        rankText.text = rank.ToString();
        usernameText.text = result.playerName;
        scoreText.text = result.score.ToString();
        timeText.text = result.completionTime.ToString("F1") + "s";
    }
}
