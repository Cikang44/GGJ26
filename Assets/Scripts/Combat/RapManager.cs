using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// Manages the rhythm battle system (FNF-like) and battle flow between player and enemies
/// </summary>
public class RapManager : MonoBehaviourSingletonPersistent<RapManager>
{
    public enum NoteDirection
    { Left = 0, Down = 1, Up = 2, Right = 3 }

    #region Data Structures

    [Serializable]
    public class Note
    {
        public float time;              // Time in milliseconds
        public int noteData;            // 0-3: Left, Down, Up, Right (player) / 4-7: opponent notes
        public float sustainLength;     // Hold duration in milliseconds
        public bool altAnimation;       // Alternative animation flag

        public Note(float time, int noteData, float sustainLength = 0f)
        {
            this.time = time;
            this.noteData = noteData;
            this.sustainLength = sustainLength;
            this.altAnimation = false;
        }

        public NoteDirection GetDirection()
        {
            return (NoteDirection)(noteData % 4);
        }

        public bool IsPlayerNote()
        {
            return noteData < 4;
        }
    }

    [Serializable]
    public class NoteArray
    {
        public List<float> values = new List<float>();
    }

    [Serializable]
    public class Section
    {
        public int lengthInSteps = 16;
        public float bpm;
        public bool changeBPM = false;
        public bool mustHitSection = true;
        public List<NoteArray> sectionNotes = new List<NoteArray>();  // JsonUtility-compatible nested array
        public int typeOfSection = 0;
        public bool altAnim = false;

        public List<Note> GetNotes()
        {
            List<Note> notes = new();
            if (sectionNotes == null) return notes;

            foreach (var noteArray in sectionNotes)
            {
                if (noteArray != null && noteArray.values != null && noteArray.values.Count >= 2)
                {
                    float time = noteArray.values[0];
                    int noteData = (int)noteArray.values[1];
                    float sustain = noteArray.values.Count > 2 ? noteArray.values[2] : 0f;

                    if (!mustHitSection)
                    {
                        // Swap player and opponent notes
                        if (noteData < 4)
                        {
                            noteData += 4; // Player notes become opponent notes
                        }
                        else if (noteData < 8)
                        {
                            noteData -= 4; // Opponent notes become player notes
                        }
                    }

                    notes.Add(new Note(time, noteData, sustain)
                    {
                        altAnimation = altAnim
                    });
                }
            }
            return notes;
        }
    }

    [Serializable]
    public class SongData
    {
        public string song;
        public float bpm;
        public bool needsVoices = true;
        public float speed = 1f;
        public string player1 = "bf";
        public string player2 = "dad";
        public string gf = "gf";
        public string stage = "stage";
        public List<Section> notes = new();
        public bool validScore = true;
    }

    [Serializable]
    public class ChartData
    {
        public SongData song; // Legacy format support

        // Version 2.0.0 format
        public string version;
        public ScrollSpeed scrollSpeed;
        public NotesDifficulty notes;
        public string generatedBy;
    }

    [Serializable]
    public class ScrollSpeed
    {
        public float normal = 1f;
    }

    [Serializable]
    public class NotesDifficulty
    {
        public List<NoteData> easy = new();
        public List<NoteData> normal = new();
    }

    [Serializable]
    public class NoteData
    {
        public float t; // time in milliseconds
        public int d; // direction (0-3: player, 4-7: opponent)
        public float l; // length/sustain
        public List<object> p = new(); // properties
    }

    #endregion

    #region Battle State

    [Header("Battle State")]
    [Tooltip("Is a battle currently active?")]
    public bool IsBattleActive { get; private set; }

    [Tooltip("Current loaded chart data")]
    public ChartData currentChart { get; private set; }

    [Tooltip("Current enemy in battle")]
    public Enemy currentEnemy { get; private set; }

    #endregion

    #region References

    [Header("References")]
    [Tooltip("Player GameObject")]
    public GameObject player;

    [Header("Battle Settings")]
    [Tooltip("Disable player movement during battle")]
    public bool disablePlayerMovement = true;

    [Tooltip("Main gameplay camera")]
    public Camera mainCamera;

    [Header("Battle UI")]
    [Tooltip("Battle UI Canvas")]
    public GameObject battleUICanvas;

    [Header("Battle Components")]
    [Tooltip("Note spawner component")]
    public NoteSpawner noteSpawner;

    [Tooltip("Player character display in battle")]
    public BattleCharacterDisplay playerDisplay;

    [Tooltip("Opponent character display in battle")]
    public BattleCharacterDisplay opponentDisplay;

    [Header("Audio")]
    [Tooltip("Audio source for instrumental track")]
    public AudioSource instrumentalSource;

    [Header("Health System")]
    [Tooltip("Health bar fill image")]
    public UnityEngine.UI.Image healthBarFill;

    [Tooltip("Starting health (0-100, 50 is center)")]
    public float startingHealth = 50f;

    #endregion

    #region Events

    [Header("Events")]
    public UnityEvent<Enemy> OnBattleStart;
    public UnityEvent<bool>
    OnBattleEnd; // bool = player won
    public UnityEvent<Note> OnNoteHit;
    public UnityEvent<Note> OnNoteMiss;

    #endregion

    private PlayerMovement _playerMovement;
    private PlayerBattery _playerBattery;
    private Vector3 _playerBattlePosition;
    private Vector3 _enemyBattlePosition;
    private float _currentBattleTime = 0f;
    private int _currentScore = 0;
    private int _combo = 0;
    private float _currentHealth = 50f;
    private int _totalPlayerNotes = 0;
    private float _healthPerPlayerNote = 0f;

    [Header("Audios")]
    public AudioSource hitSound;
    public AudioSource missSound;

    public override void Awake()
    {
        base.Awake();

        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
        }
    }

    public void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerMovement>().gameObject;
        if (mainCamera == null) mainCamera = Camera.main;

        if (player != null)
        {
            _playerMovement = player.GetComponent<PlayerMovement>();
            _playerBattery = player.GetComponent<PlayerBattery>();
        }
    }

    #region Battle Management

    /// <summary>
    /// Initiates a battle with the specified enemy
    /// </summary>
    public void StartBattle(Enemy enemy)
    {
        if (IsBattleActive || enemy == null)
        {
            return;
        }

        IsBattleActive = true;
        PlayerMovement.isInControl = false;
        currentEnemy = enemy;
        _playerBattery.EnterSleepMode();

        // Store battle positions
        if (player != null)
        {
            _playerBattlePosition = player.transform.position;
        }
        _enemyBattlePosition = enemy.transform.position;

        // Notify enemy
        enemy.EnterBattle();

        // Disable player movement if needed
        if (disablePlayerMovement && _playerMovement != null)
        {
            _playerMovement.enabled = false;
        }

        // Show battle UI
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(true);
        }

        // Load chart data and music
        int chartIndex = UnityEngine.Random.Range(0, enemy.battleChartAsset.Length);
        instrumentalSource.clip = enemy.battleMusicClip[chartIndex];
        if (enemy.battleChartAsset != null)
        {
            LoadChartFromJSON(enemy.battleChartAsset[chartIndex].text);
        }
        else
        {
            Debug.LogWarning("No chart data found for enemy, generating default chart");
            GenerateDefaultChart(enemy);
        }

        // Initialize battle
        _currentBattleTime = 0f;
        _currentScore = 0;
        _combo = 0;
        _currentHealth = startingHealth;
        UpdateHealthBar();

        // Setup character displays
        if (playerDisplay != null)
        {
            playerDisplay.gameObject.SetActive(true);
        }
        if (opponentDisplay != null)
        {
            opponentDisplay.gameObject.SetActive(true);
        }

        // Initialize note spawner
        if (noteSpawner != null && currentChart != null)
        {
            List<Note> allNotes = GetAllNotes();

            // Count player notes
            _totalPlayerNotes = 0;
            foreach (var note in allNotes)
            {
                if (note.IsPlayerNote())
                    _totalPlayerNotes++;
            }

            if (_totalPlayerNotes > 0)
            {
                _healthPerPlayerNote = 50f / _totalPlayerNotes; // Hitting all notes = +50 health
            }

            Debug.Log($"Health per player note: {_healthPerPlayerNote:F3}");
            Debug.Log($"Total player notes: {_totalPlayerNotes}");

            // Get BPM and speed based on chart format
            float bpm = 100f; // Default BPM for v2 format
            float speed = 1f;

            if (currentChart.song != null)
            {
                // Legacy format
                bpm = currentChart.song.bpm;
                speed = currentChart.song.speed;
            }
            else if (currentChart.scrollSpeed != null)
            {
                // v2 format
                speed = currentChart.scrollSpeed.normal;
                // BPM is not specified in v2 format, use default or calculate from notes
            }

            noteSpawner.Initialize(allNotes, bpm, speed);
            noteSpawner.StartSpawning();
        }

        // Play music (if available)
        if (instrumentalSource != null && instrumentalSource.clip != null)
        {
            Debug.Log($"Playing battle music: {instrumentalSource.clip.name}");
            instrumentalSource.Play();
        }

        // Invoke event
        OnBattleStart?.Invoke(enemy);

        Debug.Log($"Battle started with {enemy.enemyName}!");
    }

    /// <summary>
    /// Ends the current battle
    /// </summary>
    /// <param name="playerWon">Did the player win the battle?</param>
    public void EndBattle(bool playerWon)
    {
        if (!IsBattleActive)
        {
            return;
        }

        IsBattleActive = false;
        PlayerMovement.isInControl = true;
        _playerBattery.WakeUp();

        // Notify enemy
        if (currentEnemy != null)
        {
            currentEnemy.ExitBattle(!playerWon);
        }

        // Re-enable player movement
        if (disablePlayerMovement && _playerMovement != null)
        {
            _playerMovement.enabled = true;
        }

        // Hide battle UI
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
        }

        // Hide character displays
        if (playerDisplay != null)
        {
            playerDisplay.gameObject.SetActive(false);
        }
        if (opponentDisplay != null)
        {
            opponentDisplay.gameObject.SetActive(false);
        }

        // Stop note spawner
        if (noteSpawner != null)
        {
            noteSpawner.StopSpawning();
        }

        // Stop music
        if (instrumentalSource != null)
        {
            instrumentalSource.Stop();
        }

        // Invoke event
        OnBattleEnd?.Invoke(playerWon);

        Debug.Log($"Battle ended! Player {(playerWon ? "won" : "lost")}! Score: {_currentScore}");

        currentEnemy = null;
        currentChart = null;
    }

    /// <summary>
    /// Called by rhythm game when player successfully completes the battle
    /// </summary>
    public void OnBattleVictory()
    {
        if (currentEnemy != null) Destroy(currentEnemy.gameObject);
        EndBattle(true);
    }

    /// <summary>
    /// Called by rhythm game when player fails the battle
    /// </summary>
    public void OnBattleDefeat()
    {
        EndBattle(false);
        player.GetComponent<HealthBehaviour>().TakeDamage(50);
    }

    /// <summary>
    /// Force quit the current battle
    /// </summary>
    public void QuitBattle()
    {
        if (IsBattleActive)
        {
            EndBattle(false);
        }
    }

    #endregion

    #region Chart Loading

    /// <summary>
    /// Load chart data from JSON string (FNF chart format)
    /// </summary>
    public void LoadChartFromJSON(string jsonText)
    {
        try
        {
            // Parse using custom parser to handle nested arrays
            currentChart = ParseChartData(jsonText);

            if (currentChart != null)
            {
                if (currentChart.version != null)
                {
                    // v2 format
                    Debug.Log($"Loaded chart v{currentChart.version}");
                    Debug.Log($"Easy notes: {currentChart.notes?.easy.Count ?? 0}, Normal notes: {currentChart.notes?.normal.Count ?? 0}");
                }
                else if (currentChart.song != null)
                {
                    // Legacy format
                    Debug.Log($"Loaded chart: {currentChart.song.song} - BPM: {currentChart.song.bpm}");
                    Debug.Log($"Sections: {currentChart.song.notes.Count}");
                }

                int totalNotes = GetAllNotes().Count;
                Debug.Log($"Total Notes: {totalNotes}");

                if (totalNotes == 0)
                {
                    Debug.LogWarning("Chart loaded but contains no notes!");
                }
            }
            else
            {
                Debug.LogError("Failed to parse chart data");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading chart JSON: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Custom parser to handle FNF chart JSON with nested arrays
    /// </summary>
    private ChartData ParseChartData(string jsonText)
    {
        // Use Unity's built-in JSON parser with some preprocessing
        // Replace nested arrays format to make it JsonUtility-compatible

        ChartData chartData = new ChartData();

        // Simple JSON parsing (could use JsonUtility for non-nested parts)
        try
        {
            var rootDict = MiniJSON.Json.Deserialize(jsonText) as Dictionary<string, object>;
            if (rootDict == null)
            {
                Debug.LogError("Invalid chart format: could not parse JSON");
                return null;
            }

            // Detect format version
            if (rootDict.ContainsKey("version"))
            {
                // Version 2.0.0 format
                return ParseChartDataV2(rootDict);
            }
            else if (rootDict.ContainsKey("song"))
            {
                // Legacy format
                return ParseChartDataLegacy(rootDict);
            }
            else
            {
                Debug.LogError("Invalid chart format: unknown format");
                return null;
            }

        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing chart data: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Parse legacy FNF chart format
    /// </summary>
    private ChartData ParseChartDataLegacy(Dictionary<string, object> rootDict)
    {
        ChartData chartData = new ChartData();
        chartData.song = new SongData();

        var songDict = rootDict["song"] as Dictionary<string, object>;
        if (songDict == null)
        {
            Debug.LogError("Invalid chart format: 'song' is not an object");
            return null;
        }

        // Parse basic song properties
        chartData.song.song = songDict.ContainsKey("song") ? songDict["song"].ToString() : "Unknown";
        chartData.song.bpm = songDict.ContainsKey("bpm") ? Convert.ToSingle(songDict["bpm"]) : 120f;
        chartData.song.speed = songDict.ContainsKey("speed") ? Convert.ToSingle(songDict["speed"]) : 1f;
        chartData.song.player1 = songDict.ContainsKey("player1") ? songDict["player1"].ToString() : "bf";
        chartData.song.player2 = songDict.ContainsKey("player2") ? songDict["player2"].ToString() : "dad";

        if (songDict.ContainsKey("gf"))
            chartData.song.gf = songDict["gf"].ToString();
        if (songDict.ContainsKey("stage"))
            chartData.song.stage = songDict["stage"].ToString();
        if (songDict.ContainsKey("needsVoices"))
            chartData.song.needsVoices = Convert.ToBoolean(songDict["needsVoices"]);
        if (songDict.ContainsKey("validScore"))
            chartData.song.validScore = Convert.ToBoolean(songDict["validScore"]);

        // Parse sections (notes)
        if (songDict.ContainsKey("notes"))
        {
            var notesList = songDict["notes"] as List<object>;
            if (notesList != null)
            {
                foreach (var sectionObj in notesList)
                {
                    var sectionDict = sectionObj as Dictionary<string, object>;
                    if (sectionDict == null) continue;

                    Section section = new Section();

                    // Parse section properties
                    if (sectionDict.ContainsKey("lengthInSteps"))
                        section.lengthInSteps = Convert.ToInt32(sectionDict["lengthInSteps"]);
                    if (sectionDict.ContainsKey("bpm"))
                        section.bpm = Convert.ToSingle(sectionDict["bpm"]);
                    if (sectionDict.ContainsKey("changeBPM"))
                        section.changeBPM = Convert.ToBoolean(sectionDict["changeBPM"]);
                    if (sectionDict.ContainsKey("mustHitSection"))
                        section.mustHitSection = Convert.ToBoolean(sectionDict["mustHitSection"]);
                    if (sectionDict.ContainsKey("typeOfSection"))
                        section.typeOfSection = Convert.ToInt32(sectionDict["typeOfSection"]);
                    if (sectionDict.ContainsKey("altAnim"))
                        section.altAnim = Convert.ToBoolean(sectionDict["altAnim"]);

                    // Parse section notes (nested array)
                    if (sectionDict.ContainsKey("sectionNotes"))
                    {
                        var sectionNotesList = sectionDict["sectionNotes"] as List<object>;
                        if (sectionNotesList != null)
                        {
                            foreach (var noteObj in sectionNotesList)
                            {
                                var noteList = noteObj as List<object>;
                                if (noteList != null && noteList.Count >= 2)
                                {
                                    NoteArray noteArray = new NoteArray();
                                    foreach (var value in noteList)
                                    {
                                        noteArray.values.Add(Convert.ToSingle(value));
                                    }
                                    section.sectionNotes.Add(noteArray);
                                }
                            }
                        }
                    }

                    chartData.song.notes.Add(section);
                }
            }
        }

        return chartData;
    }

    /// <summary>
    /// Parse Version 2.0.0 chart format
    /// </summary>
    private ChartData ParseChartDataV2(Dictionary<string, object> rootDict)
    {
        ChartData chartData = new ChartData();

        // Parse version
        chartData.version = rootDict.ContainsKey("version") ? rootDict["version"].ToString() : "2.0.0";

        // Parse scrollSpeed
        if (rootDict.ContainsKey("scrollSpeed"))
        {
            var scrollSpeedDict = rootDict["scrollSpeed"] as Dictionary<string, object>;
            if (scrollSpeedDict != null)
            {
                chartData.scrollSpeed = new ScrollSpeed();
                if (scrollSpeedDict.ContainsKey("normal"))
                    chartData.scrollSpeed.normal = Convert.ToSingle(scrollSpeedDict["normal"]);
            }
        }
        else
        {
            chartData.scrollSpeed = new ScrollSpeed { normal = 1f };
        }

        // Parse notes
        chartData.notes = new NotesDifficulty();
        if (rootDict.ContainsKey("notes"))
        {
            var notesDict = rootDict["notes"] as Dictionary<string, object>;
            if (notesDict != null)
            {
                // Parse easy difficulty
                if (notesDict.ContainsKey("easy"))
                {
                    var easyList = notesDict["easy"] as List<object>;
                    if (easyList != null)
                    {
                        foreach (var noteObj in easyList)
                        {
                            var noteDict = noteObj as Dictionary<string, object>;
                            if (noteDict != null)
                            {
                                NoteData noteData = new NoteData();
                                if (noteDict.ContainsKey("t"))
                                    noteData.t = Convert.ToSingle(noteDict["t"]);
                                if (noteDict.ContainsKey("d"))
                                    noteData.d = Convert.ToInt32(noteDict["d"]);
                                if (noteDict.ContainsKey("l"))
                                    noteData.l = Convert.ToSingle(noteDict["l"]);
                                if (noteDict.ContainsKey("p"))
                                    noteData.p = noteDict["p"] as List<object> ?? new List<object>();

                                chartData.notes.easy.Add(noteData);
                            }
                        }
                    }
                }

                // Parse normal difficulty
                if (notesDict.ContainsKey("normal"))
                {
                    var normalList = notesDict["normal"] as List<object>;
                    if (normalList != null)
                    {
                        foreach (var noteObj in normalList)
                        {
                            var noteDict = noteObj as Dictionary<string, object>;
                            if (noteDict != null)
                            {
                                NoteData noteData = new NoteData();
                                if (noteDict.ContainsKey("t"))
                                    noteData.t = Convert.ToSingle(noteDict["t"]);
                                if (noteDict.ContainsKey("d"))
                                    noteData.d = Convert.ToInt32(noteDict["d"]);
                                if (noteDict.ContainsKey("l"))
                                    noteData.l = Convert.ToSingle(noteDict["l"]);
                                if (noteDict.ContainsKey("p"))
                                    noteData.p = noteDict["p"] as List<object> ?? new List<object>();

                                chartData.notes.normal.Add(noteData);
                            }
                        }
                    }
                }
            }
        }

        // Parse generatedBy
        if (rootDict.ContainsKey("generatedBy"))
            chartData.generatedBy = rootDict["generatedBy"].ToString();

        Debug.Log($"Loaded chart v{chartData.version}: {chartData.notes.easy.Count} easy notes, {chartData.notes.normal.Count} normal notes");

        return chartData;
    }

    /// <summary>
    /// Load chart from TextAsset
    /// </summary>
    public void LoadChartFromAsset(TextAsset chartAsset)
    {
        if (chartAsset != null)
        {
            LoadChartFromJSON(chartAsset.text);
        }
    }

    /// <summary>
    /// Get all notes from the current chart
    /// </summary>
    public List<Note> GetAllNotes()
    {
        List<Note> allNotes = new();

        if (currentChart == null) return allNotes;

        // Handle v2 format
        if (currentChart.notes != null)
        {
            // Use easy difficulty by default (can be made configurable)
            var noteDataList = currentChart.notes.easy.Count > 0 ? currentChart.notes.easy : currentChart.notes.normal;

            foreach (var noteData in noteDataList)
            {
                // Convert d (0-7) to noteData format
                // d: 0-3 are player notes (Left, Down, Up, Right)
                // d: 4-7 are opponent notes (Left, Down, Up, Right)
                int convertedNoteData = noteData.d < 4 ? noteData.d : noteData.d; // Keep as-is since format matches

                allNotes.Add(new Note(noteData.t, convertedNoteData, noteData.l));
            }
        }
        // Handle legacy format
        else if (currentChart.song != null)
        {
            foreach (var section in currentChart.song.notes)
            {
                allNotes.AddRange(section.GetNotes());
            }
        }

        return allNotes;
    }

    /// <summary>
    /// Get only player notes from the current chart
    /// </summary>
    public List<Note> GetPlayerNotes()
    {
        List<Note> playerNotes = new();
        var allNotes = GetAllNotes();

        foreach (var note in allNotes)
        {
            if (note.IsPlayerNote())
            {
                playerNotes.Add(note);
            }
        }

        return playerNotes;
    }

    /// <summary>
    /// Generate a default chart for testing
    /// </summary>
    private void GenerateDefaultChart(Enemy enemy)
    {
        currentChart = new ChartData
        {
            song = new SongData
            {
                song = enemy.enemyName,
                bpm = 120f,
                player1 = "bf",
                player2 = enemy.enemyName,
                speed = 2f
            }
        };

        // Generate a simple pattern
        Section section = new()
        {
            bpm = 120f,
            mustHitSection = true
        };

        // Add some test notes
        for (int i = 0; i < 16; i++)
        {
            float time = i * 500f; // Every 500ms
            int noteData = i % 4; // Cycle through directions
            section.sectionNotes[i] = new NoteArray();
            section.sectionNotes[i].values.Add(time);
            section.sectionNotes[i].values.Add(noteData);
            section.sectionNotes[i].values.Add(0f); // No sustain
        }

        currentChart.song.notes.Add(section);
    }

    #endregion

    #region Score Management

    public void AddScore(int points)
    {
        _currentScore += points;
        _combo++;
    }

    public void ResetCombo()
    {
        _combo = 0;
    }

    public int GetCurrentScore()
    {
        return _currentScore;
    }

    public int GetCurrentCombo()
    {
        return _combo;
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    #endregion

    #region Health Management

    public void OnPlayerNoteHit()
    {
        float healthGain = _healthPerPlayerNote * 5f;
        _currentHealth = Mathf.Clamp(_currentHealth + healthGain, 0f, 100f);
        UpdateHealthBar();
        CheckWinLoseCondition();
        if (hitSound != null)
        {
            hitSound.Stop();
            hitSound.Play();
        }
    }

    public void OnPlayerSustainHold(float holdDurationMs)
    {
        float healthGainPerMs = _healthPerPlayerNote / 100f; // Divide by 100ms as base
        float healthGain = healthGainPerMs * holdDurationMs;
        _currentHealth = Mathf.Clamp(_currentHealth + healthGain, 0f, 100f);
        UpdateHealthBar();
        CheckWinLoseCondition();
    }

    public void OnPlayerNoteMiss()
    {
        float healthLoss = _healthPerPlayerNote * 15f;
        _currentHealth = Mathf.Clamp(_currentHealth - healthLoss, 0f, 100f);
        UpdateHealthBar();
        CheckWinLoseCondition();
        if (missSound != null)
        {
            missSound.Stop();
            missSound.Play();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Health bar: 0 = dead, 50 = center, 100 = winning
            healthBarFill.fillAmount = _currentHealth / 100f;

            if (_currentHealth < 25f)
            {
                healthBarFill.color = Color.red;
            }
            else if (_currentHealth < 40f)
            {
                healthBarFill.color = Color.Lerp(Color.red, Color.yellow, (_currentHealth - 25f) / 15f);
            }
            else if (_currentHealth < 60f)
            {
                healthBarFill.color = Color.yellow;
            }
            else if (_currentHealth < 75f)
            {
                healthBarFill.color = Color.Lerp(Color.yellow, Color.green, (_currentHealth - 60f) / 15f);
            }
            else
            {
                healthBarFill.color = Color.green;
            }
        }
    }

    public void OnSongEnd()
    {
        if (_currentHealth > 25f)
        {
            OnBattleVictory();
        }
        else
        {
            OnBattleDefeat();
        }
    }
    private void CheckWinLoseCondition()
    {
        if (_currentHealth <= 0f)
        {
            // Player lost
            OnBattleDefeat();
        }
    }

    #endregion
}