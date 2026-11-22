using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("In Game")]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _countDownText;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private TextMeshProUGUI _resultText;


    void Awake()
    {
        _startButton.onClick.AddListener(() => GameManager.Instance.RequestStartRpc());
        _exitButton.onClick.AddListener(() => NetworkManager.Singleton.Shutdown());
    }

    void Update()
    {
        var gm = GameManager.Instance;
        var nm = NetworkManager.Singleton;

        bool online = gm != null && gm.IsSpawned;

        var timeText = online && gm.State.Value == GameState.Playing ? Mathf.CeilToInt(gm.TimeLeft()).ToString() : "";
        _timeText.text = timeText;

        if (online && gm.State.Value == GameState.Ended)
        {
            PlayerNetwork top = PlayerNetwork.All.OrderByDescending(p => p.Score.Value).FirstOrDefault();
            _resultText.text = top != null ? $"{top.PlayerName.Value} Win" : "";
        }

        _playerNumberText.text = $"{PlayerNetwork.All.Count}/4";

        _timeText.gameObject.SetActive(online && gm.State.Value == GameState.Playing);

        _countDownText.gameObject.SetActive(online && gm.State.Value == GameState.CountDown);
        _countDownText.text = gm.CountDownTime.Value.ToString();

        _startButton.gameObject.SetActive(online && nm.IsHost && gm.State.Value is not (GameState.Playing or GameState.CountDown));
        _resultPanel.SetActive(online && gm.State.Value == GameState.Ended);
    }
}
