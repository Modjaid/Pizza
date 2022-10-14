using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameStatus
{
    Started,
    Paused,
}

/// <summary>
/// Main class for game. Controlls all other managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameStatus GameStatus { get; private set; }
    public PlayerAxisController player;
    public LevelsData levelsData;
    public CameraController cameraController;

    [Header("Debug")]
    public bool jumpStartGame;
    public bool completeDeliveryAtStart;
    public bool dontChangePlayerLocation;

    public delegate void gameStartDelegate();
    public event gameStartDelegate OnGameStarted;

    public delegate void gameStopDelegate();
    public event gameStartDelegate OnGameStopped;

    // (in this session)
    private float deliveriesCompleted;
    private int loadedLevel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("levelBlockIsLoaded", 0) == 1)
        {
            loadedLevel = PlayerPrefs.GetInt("levelBlockLoaded");
            Debug.Log("Level block is loaded: level " + loadedLevel);
        }
        else
        {
            loadedLevel = PlayerPrefs.GetInt("level");
            Debug.Log("Level is loaded: level " + loadedLevel);
        }

        UIManager.Instance.ShowCurrentLevel(loadedLevel);

        ChangeGameStatus(GameStatus.Paused);
        Time.timeScale = 1f;

        ApplyLevelData();

        if (!dontChangePlayerLocation)
            DeliveryJobManager.Instance.SelectRandomPlayerPosition();

        if (!DeliveryJobManager.Instance.HasJob())
        {
            DeliveryJobManager.Instance.SelectNewJob();
        }

        if (jumpStartGame)
            StartGame();

        if (completeDeliveryAtStart)
            OnGameComplete();
    }

    private void Update()
    {
#if UNITY_EDITOR

        if (Input.GetKeyDown("z"))
        {
            DeliveryJobManager.Instance.OnArrivedInDestination();
        }

#endif
    }

    public void StartGame()
    {
        ChangeGameStatus(GameStatus.Started);

        OnGameStarted?.Invoke();
    }

    public void ChangeLocationAndStartNewDelivery()
    {
        // Boilerplate code.

        LevelLoader.Instance.ReloadLevel();
    }

    public void OnPlayerCatched()
    {
        ChangeGameStatus(GameStatus.Paused);
        UIManager.Instance.SetPlayerControlsActive(false);
    }

    // If there are still deliveries to be done in this game
    public bool HasMoreDeliveriesToDo()
    {
        if (deliveriesCompleted < levelsData.deliveriesCount.GetCountAt(loadedLevel))
            return true;
        return false;
    }

    public void OnSuccesfulDelivery()
    {
        deliveriesCompleted++;

        ChangeGameStatus(GameStatus.Paused);
        Time.timeScale = 0f;
        UIManager.Instance.SetPlayerControlsActive(false);

        if (HasMoreDeliveriesToDo())
        {
            UIManager.Instance.ShowSuccesfullDeliveryScreen();
        }
        else
        {
            OnGameComplete();
        }
    }

    // Triggered when there are still deliveries to do
    // And player closes DeliveryCompleted screen.
    public void OnDeliveryContinued()
    {
        Time.timeScale = 1f;

        DeliveryJobManager.Instance.DeletePreviousJobSigns();
        DeliveryJobManager.Instance.SelectNewJob();
        cameraController.ChangeMode(CameraMode.FlyBy);
        UIManager.Instance.ShowSkipFlyByButton();
        UIManager.Instance.SetBlockClicksPanelActive(true);
        // order of execution here is important
        player.StopSprint();
        VFXManager.Instance.ClearSprintEffect();
        cameraController.StopSprintEffectImmediately();
    }

    private void ChangeGameStatus(GameStatus newStatus)
    {
        GameStatus = newStatus;

        switch (newStatus)
        {
            case GameStatus.Paused:
                OnGameStopped?.Invoke();
                break;

            case GameStatus.Started:
                OnGameStarted?.Invoke();
                break;
        }
    }

    private void ApplyLevelData()
    {
        int removedEnemiesCount = levelsData.deductedEnemiesCount.GetCountAt(loadedLevel);

        EnemiesManager.Instance.RemoveEnemies(removedEnemiesCount);
    }

    private void OnGameComplete()
    {
        UIManager.Instance.ShowGameCompleteScreen();

        // note: not accounting for max levels count

        if (PlayerPrefs.GetInt("levelBlockIsLoaded") == 1)
        {
            PlayerPrefs.SetInt("levelBlockLoaded", loadedLevel + 1);
        }
        else
        {
            int level = PlayerPrefs.GetInt("level") + 1;

            PlayerPrefs.SetInt("level", level);
        }
    }
}