using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles overall game state, win/lose toggling, and pausing
/// </summary>
public class GameManager : IGameManager, IDisposable
{
    private bool pauseOn;
    private LevelData currentLevelData;

    public LevelData GetLevelData => currentLevelData;
    public Action VictoryEvent { get; set; }
    public Action DefeatEvent { get; set; }

    public event Action GamePaused;
    public event Action GameUnPaused;

    public void Initialize()
    {
        // Set level specific data
        SetLevelData();

        // Subscribe to events
        Services.Get<IInputManager>().EscapeButton += PauseToggle;
        Services.Get<IGemManager>().AllGemsLost += GameLost;
        WaveManager.AllEnemiesProcessed += GameWon;
        HUD.OnRestartButton += RestartGame;

        SetTimeScale(1f);
    }

    private void SetTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }

    /// <summary>
    /// Called via event when the player hits the restart button
    /// </summary>
    private void RestartGame()
    {
        PauseToggle();
        Loader.Restart();
    }

    /// <summary>
    /// Set level data for this scene
    /// </summary>
    private void SetLevelData()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string levelNumber = sceneName.Replace("Level", "").Trim();

        LevelData[] allLevelData = Resources.LoadAll<LevelData>("");
        currentLevelData = allLevelData.FirstOrDefault(level => level.name.Contains(levelNumber));
    }

    /// <summary>
    /// Triggered when the player has lost all their gems
    /// </summary>
    private void GameLost()
    {
        DefeatEvent?.Invoke();
        PauseToggle();

        Services.Get<IGameStateHandler>().SetGameState(GameState.Defeat);
    }

    /// <summary>
    /// Triggered when the player has defeated all waves
    /// </summary>
    private void GameWon()
    {
        VictoryEvent?.Invoke();
        PauseToggle();

        Services.Get<IGameStateHandler>().SetGameState(GameState.Victory);
    }

    private void PauseToggle()
    {
        pauseOn = !pauseOn;

        if (pauseOn)
        {
            GamePaused?.Invoke();
            SetTimeScale(0.0000001f);
        }
        else
        {
            GameUnPaused?.Invoke();
            SetTimeScale(1f);
        }
    }

    public void Dispose()
    {
        Services.Get<IInputManager>().EscapeButton -= PauseToggle;
        Services.Get<IGemManager>().AllGemsLost -= GameLost;
        WaveManager.AllEnemiesProcessed -= GameWon;
        HUD.OnRestartButton -= RestartGame;
    }

    public void CallRestart()
    {
        RestartGame();
    }

    public void CallWinState()
    {
        GameWon();
    }

    public void CallLoseState()
    {
        GameLost();
    }
}