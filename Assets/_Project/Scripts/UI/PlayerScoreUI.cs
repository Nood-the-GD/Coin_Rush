using TMPro;
using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void UpdateNameText(string playerName)
    {
        _playerNameText.text = playerName;
    }

    public void UpdateScoreText(string score)
    {
        _scoreText.text = score;
    }
}