using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private PlayerScoreUI _playerScorePrefab;
    private Dictionary<ulong, PlayerScoreUI> _playerScoreUIDic = new();
    private Dictionary<ulong, int> _playerScoreDic = new();

    void Awake()
    {
        _playerScorePrefab.gameObject.SetActive(false);
        PlayerNetwork.OnPlayerEnterGame += OnPlayerEnterGameHandler;
        PlayerNetwork.OnPlayerExitGame += OnPlayerExitGameHandler;
        PlayerNetwork.OnPlayerNameChange += OnPlayerNameChangeHandler;
        PlayerNetwork.OnPlayerScoreChange += OnPlayerScoreChangeHandler;
    }

    void OnDestroy()
    {
        PlayerNetwork.OnPlayerEnterGame -= OnPlayerEnterGameHandler;
        PlayerNetwork.OnPlayerExitGame -= OnPlayerExitGameHandler;
        PlayerNetwork.OnPlayerNameChange -= OnPlayerNameChangeHandler;
        PlayerNetwork.OnPlayerScoreChange -= OnPlayerScoreChangeHandler;
    }

    private void OnPlayerExitGameHandler(PlayerNetwork network)
    {
        if(_playerScoreUIDic.ContainsKey(network.NetworkObjectId))
        {
            Destroy(_playerScoreUIDic[network.NetworkObjectId].gameObject);
            _playerScoreUIDic.Remove(network.NetworkObjectId);
            _playerScoreDic.Remove(network.NetworkObjectId);
        }
    }

    private void OnPlayerScoreChangeHandler(PlayerNetwork network, int score)
    {
        if (_playerScoreUIDic.ContainsKey(network.NetworkObjectId))
        {
            var id = network.NetworkObjectId;
            _playerScoreDic[id] = score;
            var orderedList = _playerScoreDic.OrderByDescending(p => p.Value).Select(x => x.Key).ToList();
            _playerScoreUIDic[id].UpdateScoreText(score.ToString());
            _playerScoreUIDic[id].transform.SetSiblingIndex(orderedList.IndexOf(id));
        }
    }

    private void OnPlayerNameChangeHandler(PlayerNetwork network, string name)
    {
        if(_playerScoreUIDic.ContainsKey(network.NetworkObjectId))
            _playerScoreUIDic[network.NetworkObjectId].UpdateNameText(name);
    }

    private void OnPlayerEnterGameHandler(PlayerNetwork network)
    {
        if(!_playerScoreUIDic.ContainsKey(network.NetworkObjectId))
        {
            var scoreUI = Instantiate(_playerScorePrefab, _playerScorePrefab.transform.parent);
            _playerScoreUIDic.Add(network.NetworkObjectId, scoreUI);
            scoreUI.gameObject.SetActive(true);
        }
    }
}
