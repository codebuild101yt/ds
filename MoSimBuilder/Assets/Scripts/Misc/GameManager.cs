using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const int MATCH_DURATION = 150;

    public static GameState GameState { get; private set; }

    public float secondaryTimer;
    private float timer;

    public TextMeshProUGUI timerText;
    public RobotSpawnController spawn;

    private AudioSource player;

    [SerializeField] private AudioResource auto;
    [SerializeField] private AudioResource teleop;
    [SerializeField] private AudioResource endgame;
    [SerializeField] private AudioResource end;

    private bool triggerEnd = true;
    private bool triggerTeleop = true;
    private bool triggerEndgame = true;

    private bool isResetting = false;
    private bool countdown = true;

    // ===============================
    // GLOBAL STATE FLAGS
    // ===============================
    public static bool canRobotMove { get; private set; }
    public static bool isDisabled = false;
    public static bool endBuzzerPlaying = false;

    // 🔹 AUTO motion override (scripts control robot)
    public static bool AutoMotionOverride { get; private set; }

    // ===============================
    // UI / OBJECTS
    // ===============================
    [SerializeField] private GameObject button;
    [SerializeField] private GameObject videoPlayer;
    [SerializeField] private GameObject scoreCard;
    public string[] tagsToDestroy;

    private IResettable[] resettables;
    private DriveController[] swerveControllers;

    private const int SHOW_SCORE_DELAY = 4;
    private const int AUTO_TO_TELEOP_DELAY = 3;

    // ===============================
    // INPUT SYSTEM
    // ===============================
    [SerializeField] private InputActionAsset actions;

    // ===============================
    // SINGLETON
    // ===============================
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (actions != null)
            DisableAllActionMaps();
    }

    private void Start()
    {
        swerveControllers = FindObjectsByType<DriveController>(FindObjectsSortMode.None);
        resettables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IResettable>().ToArray();

        GameState = GameState.Auto;

        player = GetComponent<AudioSource>();

        ResetTimer();
        canRobotMove = true;

        UpdatePlayerInputState();
    }

    private void Update()
    {
        secondaryTimer = timer;

        if (isResetting) return;

        if (countdown)
            timer -= Time.deltaTime;

        // ===============================
        // END BUZZER
        // ===============================
        if (timer <= 0f && triggerEnd)
        {
            endBuzzerPlaying = true;
            isDisabled = true;
            triggerEnd = false;
            timer = 0f;

            player.resource = end;
            player.Play();

            StartCoroutine(WaitEndgame());
            countdown = false;
            timerText.color = Color.red;

            StartCoroutine(ShowMatchScore());
        }
        // ===============================
        // AUTO → TELEOP
        // ===============================
        else if (timer < MATCH_DURATION - 15f && triggerTeleop)
        {
            triggerTeleop = false;
            StartCoroutine(Wait());
        }
        // ===============================
        // ENDGAME MUSIC
        // ===============================
        else if (timer <= 30f && triggerEndgame)
        {
            triggerEndgame = false;
            GameState = GameState.Endgame;

            player.resource = endgame;
            player.Play();
        }

        UpdateTimerDisplay(timer);
        UpdatePlayerInputState();
    }

    // =====================================================
    // INPUT CONTROL (PLAYER ONLY — NOT ROBOT PHYSICS)
    // =====================================================
    private void UpdatePlayerInputState()
    {
        if (actions == null) return;

        bool enablePlayerInput =
            !AutoMotionOverride &&
            !endBuzzerPlaying &&
            GameState == GameState.Teleop;

        foreach (var map in actions.actionMaps)
        {
            if (enablePlayerInput)
                map.Enable();
            else
                map.Disable();
        }
    }

    private void DisableAllActionMaps()
    {
        if (actions == null) return;

        foreach (var map in actions.actionMaps)
            map.Disable();
    }

    // =====================================================
    // AUTO OVERRIDE API (USED BY AUTO SCRIPTS)
    // =====================================================
    public static void SetAutoMotionOverride(bool enabled)
    {
        AutoMotionOverride = enabled;
    }

    // Used by DriveController
    public static bool PlayerMotionAllowed()
    {
        if (AutoMotionOverride) return true;
        if (isDisabled) return false;
        if (endBuzzerPlaying) return false;
        return true;
    }

    // =====================================================
    // COROUTINES
    // =====================================================
    private IEnumerator ShowMatchScore()
    {
        button.SetActive(true);
        yield return new WaitForSeconds(SHOW_SCORE_DELAY);

        if (PlayerPrefs.GetFloat("endVideo") == 1)
            videoPlayer.SetActive(true);
    }

    private IEnumerator Wait()
    {
        countdown = false;

        player.resource = end;
        player.Play();

        isDisabled = true;
        canRobotMove = false;

        yield return new WaitForSeconds(1);

        isDisabled = false;
        canRobotMove = true;
        countdown = true;

        player.resource = teleop;
        player.Play();

        yield return new WaitForSeconds(AUTO_TO_TELEOP_DELAY);
        GameState = GameState.Teleop;
    }

    private IEnumerator WaitEndgame()
    {
        countdown = false;
        isDisabled = true;
        canRobotMove = false;

        yield return new WaitForSeconds(5);

        GameState = GameState.End;
        isDisabled = false;
        canRobotMove = true;
    }

    // =====================================================
    // RESET LOGIC
    // =====================================================
    private void ResetTimer()
    {
        player.resource = auto;
        player.Play();

        AutoMotionOverride = false;
        isDisabled = false;
        countdown = true;

        triggerEnd = true;
        triggerEndgame = true;
        triggerTeleop = true;
        endBuzzerPlaying = false;

        timer = MATCH_DURATION;
        timerText.color = Color.white;

        UpdatePlayerInputState();
    }

    public void Reset()
    {
        if (!isResetting)
            ResetMatch();
    }

    private void ResetMatch()
    {
        StopAllCoroutines();
        isResetting = true;

        AutoMotionOverride = false;
        isDisabled = false;
        canRobotMove = true;

        LevelManager.Instance.PlayTransition("CrossFade");
        scoreCard.SetActive(false);
        player.Stop();

        GameState = GameState.Auto;

        if (GameObject.FindGameObjectsWithTag("MainCamera") != null)
        {
            foreach (GameObject cam in GameObject.FindGameObjectsWithTag("MainCamera"))
                cam.transform.parent = spawn.transform;
        }

        foreach (string tag in tagsToDestroy)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
                Destroy(obj);
        }

        StartCoroutine(spawnBots());
        button.SetActive(false);

        isResetting = false;
        ResetTimer();
    }

    private void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes}:{seconds:00}";
    }

    private IEnumerator spawnBots()
    {
        yield return new WaitForSeconds(0.2f);
        spawn.Respawn();
    }
}
