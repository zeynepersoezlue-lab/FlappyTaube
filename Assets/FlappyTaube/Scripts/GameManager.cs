using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
   
   public static GameManager Instance{get; private set;}
   [Header("Start Screen")]

   [SerializeField] private GameObject logo;
   [SerializeField] private GameObject playbutton;

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
   }

   public void PlayButtonClick(){
    gameState = GameState.GetReady;
    CurrentScore = 0;

    logo.SetActive(false);
    playbutton.SetActive(false);

    gameOverPanel.SetActive(false);
    gameReadyPanel.SetActive(true);
    score.gameObject.SetActive(true);

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
    gameState = GameState.GameOver;
    score.gameObject.SetActive(false);
    gameOverPanel.SetActive(true);
    playbutton.SetActive(true);

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
}
