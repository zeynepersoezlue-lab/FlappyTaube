using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class GameManager : MonoBehaviour
{
   
   public static GameManager Instance{get; private set;}
   [Header("Start Screen")]

   [SerializeField] private GameObject logo;
   [SerializeField] private GameObject playbutton;
   [SerializeField] private GameObject exitButton;

   [Header ("Score")]
   [SerializeField] private TMP_Text score;

   [Header ("Game Ready Section")]
   [SerializeField] private GameObject gameReadyPanel;

   [Header ("Game Over Section")]
   [SerializeField] private GameObject gameOverPanel;
   [SerializeField] private TMP_Text gameOverScore;
   [SerializeField] private TMP_Text gameOverBestScore;

   [Header("References")]
   [SerializeField] private PlayerController playerController;
   [SerializeField] private PipeSpawner pipeSpawner;

   [Header("Difficulty")]
   [SerializeField] private float startSpeed = 1.5f;
   [SerializeField] private float speedStep = 0.1f;
   [SerializeField] private int pointsPerStep = 5;
   [SerializeField] private float maxSpeed = 3f;

   [Header("Pause")]
   [SerializeField] private GameObject pausePanel;

   [Header("Post Processing")]
   [SerializeField] private Volume volume;
   private ColorAdjustments colorAdjustments;
   private Vignette vignette;
   [SerializeField] private float fadeDuration = 1f;
   private Coroutine effectRoutine;

   private GameState stateBeforePause;
   private float gameOverTime;

   public float PipeSpeed{ get; private set; }

   private const string BEST_SCORE_KEY = "BestScore";  

   private GameState gameState = GameState.Home;

   public GameState GameState => gameState;
   private int currentScore;

   public int CurrentScore{

    get => currentScore;
    set{
        currentScore = value;
        score.text = currentScore.ToString();
    }
   }

   public int BestScore{
     get => PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
     set{
        PlayerPrefs.SetInt(BEST_SCORE_KEY, value);
        PlayerPrefs.Save();
     }
   }

   private void Awake(){
    if(Instance != null && Instance != this){
        Destroy(gameObject);
        return;
    }
    Instance = this;

   }

   private void Start() {
    gameState = GameState.Home;
    PipeSpeed = startSpeed;
    logo.SetActive(true);
    playbutton.SetActive(true);

    gameOverPanel.SetActive(false);
    gameReadyPanel.SetActive(false);
    score.gameObject.SetActive(false);
    pausePanel.SetActive(false);
    AudioManager.Instance.MusicOn(); 
    exitButton.SetActive(true);

    volume.profile.TryGet(out colorAdjustments);
    volume.profile.TryGet(out vignette);
    SetGameOverEffect(false);
   }

   public void PlayButtonClick(){
    gameState = GameState.GetReady;
    CurrentScore = 0;
    AudioManager.Instance.MusicOn();

    logo.SetActive(false);
    playbutton.SetActive(false);

    gameOverPanel.SetActive(false);
    gameReadyPanel.SetActive(true);
    score.gameObject.SetActive(true);
    exitButton.SetActive(false);

    SetGameOverEffect(false);

    ResetGame();
   }

   private void ResetGame(){

    PipeSpeed = startSpeed;
    playerController.ResetPlayer();
    pipeSpawner.ResetSpawner();
   }

   public void GamePlay(){
    gameState = GameState.Playing;
    gameReadyPanel.SetActive(false);
   }

   public void GameOver(){
    gameOverTime = Time.time;
    gameState = GameState.GameOver;
    score.gameObject.SetActive(false);
    gameOverPanel.SetActive(true);
    playbutton.SetActive(true);
    AudioManager.Instance.MusicOff();
    exitButton.SetActive(true);
    SetGameOverEffect(true);


    gameOverScore.text = CurrentScore.ToString();
    if(CurrentScore > BestScore){
        BestScore = CurrentScore;
    }
    gameOverBestScore.text = BestScore.ToString();
   }

   public void AddScore(){
    if(gameState != GameState.Playing) return;

    CurrentScore++;

    AudioManager.Instance.Score();

    if(CurrentScore % pointsPerStep == 0) {
        PipeSpeed = Mathf.Min(PipeSpeed + speedStep, maxSpeed);
    }
   }
   
   private void Update(){
    var kb = Keyboard.current;
    if (kb == null) return;

    if (kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
        TogglePause();

    if (kb.spaceKey.wasPressedThisFrame){
        
        if (gameState == GameState.Home)
            PlayButtonClick();                                  
        else if (gameState == GameState.Paused)
            ResumeGame();                                       
        else if (gameState == GameState.GameOver && Time.time - gameOverTime > 0.5f)
            PlayButtonClick();                                 
            
    }
   }

   public void TogglePause(){
    
    if(gameState == GameState.Paused) ResumeGame();
    else if(gameState == GameState.Playing || gameState == GameState.GetReady)
            PauseGame();
   }

   public void PauseGame(){
    stateBeforePause = gameState;
    gameState = GameState.Paused;
    Time.timeScale = 0f;
    pausePanel.SetActive(true);
   }

   public void ResumeGame(){
    pausePanel.SetActive(false);
    Time.timeScale = 1f;
    gameState = stateBeforePause;
   }

   public void QuitToMenu(){
    pausePanel.SetActive(false);
    Time.timeScale = 1f;
    ResetGame();
    gameState = GameState.Home;
    gameReadyPanel.SetActive(false);
    gameOverPanel.SetActive(false);
    logo.SetActive(true);
    playbutton.SetActive(true);
    score.gameObject.SetActive(false);
    exitButton.SetActive(true);
    SetGameOverEffect(false);
    AudioManager.Instance.MusicOn();
   }

   public void QuitGame(){
    #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;  
    #elif UNITY_WEBGL
    QuitToMenu();                                     
    #else
    Application.Quit();                                
    #endif
    
    }

    private void SetGameOverEffect(bool on){
    if (effectRoutine != null) StopCoroutine(effectRoutine);

    if (on) effectRoutine = StartCoroutine(FadeGameOverEffect());
    else {
        colorAdjustments.saturation.Override(0f);
        vignette.intensity.Override(0f);
        }
    }
    
    private IEnumerator FadeGameOverEffect(){
        float t = 0f;
        while (t < 1f){
            t += Time.deltaTime / fadeDuration;
            colorAdjustments.saturation.Override(Mathf.Lerp(0f, -40f, t));  
            vignette.intensity.Override(Mathf.Lerp(0f, 0.45f, t));         
            yield return null;  
    }
}
}
