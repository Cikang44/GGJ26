using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and manages notes during battle
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Note prefab to spawn")]
    public GameObject notePrefab;
    
    [Header("Receptors")]
    [Tooltip("Player receptors (Left, Down, Up, Right)")]
    public NoteReceptor[] playerReceptors = new NoteReceptor[4];
    
    [Tooltip("Opponent receptors (Left, Down, Up, Right)")]
    public NoteReceptor[] opponentReceptors = new NoteReceptor[4];
    
    [Header("Spawn Settings")]
    [Tooltip("Distance from receptor where notes spawn")]
    public float spawnDistance = 1000f;
    
    [Tooltip("Scroll speed multiplier")]
    public float scrollSpeed = 1f;
    
    [Tooltip("Time in seconds before note hit time to spawn")]
    public float spawnTimeOffset = 2f;
    
    [Header("Hit Detection")]
    [Tooltip("Perfect hit window in milliseconds")]
    public float perfectWindow = 45f;
    
    [Tooltip("Good hit window in milliseconds")]
    public float goodWindow = 90f;
    
    [Tooltip("OK hit window in milliseconds")]
    public float okWindow = 135f;
    
    [Tooltip("Miss threshold in milliseconds")]
    public float missWindow = 180f;
    
    private List<RapManager.Note> _chartNotes;
    private readonly List<Note> _activeNotes = new();
    private float _songStartTime;
    private int _nextNoteIndex;
    private bool _isPlaying;
    private float _chartBPM;
    private float _chartSpeed;
    private bool _songEnded;
    private float _lastNoteTime; // Time of the last note in the chart

    public void Initialize(List<RapManager.Note> notes, float bpm, float chartSpeed)
    {
        _chartNotes = notes;
        _nextNoteIndex = 0;
        _activeNotes.Clear();
        _chartBPM = bpm;
        _chartSpeed = chartSpeed;
        _songEnded = false;
        
        // Sort notes by time
        _chartNotes.Sort((a, b) => a.time.CompareTo(b.time));
        
        // Find the last note time (including sustain)
        _lastNoteTime = 0f;
        foreach (var note in _chartNotes)
        {
            float noteEndTime = note.time + note.sustainLength;
            if (noteEndTime > _lastNoteTime)
            {
                _lastNoteTime = noteEndTime;
            }
        }
        
        Debug.Log($"NoteSpawner initialized: {notes.Count} notes, BPM: {bpm}, Speed: {chartSpeed}, Last note time: {_lastNoteTime}ms");
    }

    public void StartSpawning()
    {
        _songStartTime = Time.time;
        _isPlaying = true;
    }

    public void StopSpawning()
    {
        _isPlaying = false;
        
        // Clear all active notes
        foreach (var note in _activeNotes)
        {
            if (note != null)
            {
                Destroy(note.gameObject);
            }
        }
        _activeNotes.Clear();
    }

    private void Update()
    {
        if (!_isPlaying || _chartNotes == null) return;
        
        float currentTime = (Time.time - _songStartTime) * 1000f; // Convert to milliseconds
        
        // Check for player input (needs to be in Update for responsive input)
        CheckPlayerInput(currentTime);
    }

    private void FixedUpdate()
    {
        if (!_isPlaying || _chartNotes == null) return;
        
        float currentTime = (Time.time - _songStartTime) * 1000f; // Convert to milliseconds
        
        // Spawn notes
        SpawnNotes(currentTime);
        
        // Clean up missed notes
        CleanupNotes();
        
        // Check if song has ended
        CheckSongEnd(currentTime);
    }

    private void SpawnNotes(float currentTime)
    {
        float spawnTimeThreshold = spawnTimeOffset * 1000f;
        
        while (_nextNoteIndex < _chartNotes.Count)
        {
            RapManager.Note noteData = _chartNotes[_nextNoteIndex];
            
            // Check if it's time to spawn this note
            if (noteData.time - currentTime <= spawnTimeThreshold)
            {
                SpawnNote(noteData);
                _nextNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnNote(RapManager.Note noteData)
    {
        if (notePrefab == null) return;
        
        // Get appropriate receptor
        bool isPlayer = noteData.IsPlayerNote();
        NoteReceptor[] receptors = isPlayer ? playerReceptors : opponentReceptors;
        int directionIndex = (int)noteData.GetDirection();
        
        if (directionIndex < 0 || directionIndex >= receptors.Length)
        {
            Debug.LogError($"Invalid note direction: {directionIndex}");
            return;
        }
        
        NoteReceptor receptor = receptors[directionIndex];
        if (receptor == null)
        {
            Debug.LogError($"Missing receptor for direction {directionIndex}");
            return;
        }
        
        // Calculate spawn position (below receptor)
        Vector3 spawnPos = receptor.GetPosition() + Vector3.down * spawnDistance;
        
        // Spawn note
        GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity, transform);
        Note note = noteObj.GetComponent<Note>();
        
        if (note != null)
        {
            // Calculate travel time (how long the note takes to reach receptor)
            float travelTime = spawnTimeOffset * 1000f;
            
            // Calculate effective scroll speed based on chart speed and BPM
            float effectiveScrollSpeed = scrollSpeed * _chartSpeed;
            
            note.Initialize(noteData, receptor, effectiveScrollSpeed, spawnPos, _songStartTime, travelTime);
            _activeNotes.Add(note);
        }
    }

    private void CheckPlayerInput(float currentTime)
    {
        // Check each direction
        for (int i = 0; i < playerReceptors.Length; i++)
        {
            NoteReceptor receptor = playerReceptors[i];
            if (receptor == null) continue;
            
            if (Input.GetKeyDown(receptor.inputKey))
            {
                CheckNoteHit(i, currentTime);
            }
        }
    }

    private void CheckNoteHit(int directionIndex, float currentTime)
    {
        Note closestNote = null;
        float closestTimeDiff = float.MaxValue;
        
        // Find closest note in this direction
        foreach (var note in _activeNotes)
        {
            if (note == null || !note.isPlayerNote || note.HasBeenHit || note.HasMissed) continue;
            
            if ((int)note.direction == directionIndex)
            {
                float timeDiff = Mathf.Abs(note.hitTime - currentTime);
                if (timeDiff < closestTimeDiff)
                {
                    closestTimeDiff = timeDiff;
                    closestNote = note;
                }
            }
        }
        
        // Check if note is within hit window
        if (closestNote != null && closestTimeDiff <= missWindow)
        {
            string rating = GetRating(closestTimeDiff);
            closestNote.Hit();
            
            // Notify RapManager
            if (RapManager.Instance != null)
            {
                RapManager.Instance.OnNoteHit?.Invoke(new RapManager.Note(closestNote.hitTime, (int)closestNote.direction, closestNote.sustainLength));
                
                // Add score based on rating
                int score = GetScoreForRating(rating);
                RapManager.Instance.AddScore(score);
                
                // Update health for hit
                RapManager.Instance.OnPlayerNoteHit();
            }
            
            Debug.Log($"Hit! Rating: {rating} (±{closestTimeDiff:F1}ms)");
        }
        else
        {
            // Bad press
            if (RapManager.Instance != null)
            {
                RapManager.Instance.ResetCombo();
            }
            Debug.Log("Miss - no note nearby");
        }
    }

    private void CheckSongEnd(float currentTime)
    {
        if (_songEnded) return;
        
        // Check if all notes have been spawned and we're past the last note time (with some grace period)
        float gracePeriod = 2000f; // 2 seconds after last note
        if (_nextNoteIndex >= _chartNotes.Count && currentTime > _lastNoteTime + gracePeriod)
        {
            _songEnded = true;
            OnSongEnd();
        }
    }
    
    private void OnSongEnd()
    {
        Debug.Log("Song ended!");
        
        if (RapManager.Instance != null)
        {
            RapManager.Instance.OnSongEnd();
        }
    }
    
    private void CleanupNotes()
    {
        _activeNotes.RemoveAll(note => note == null);
    }

    private string GetRating(float timeDiff)
    {
        if (timeDiff <= perfectWindow) return "Perfect";
        if (timeDiff <= goodWindow) return "Good";
        if (timeDiff <= okWindow) return "OK";
        return "Bad";
    }

    private int GetScoreForRating(string rating)
    {
        return rating switch
        {
            "Perfect" => 350,
            "Good" => 200,
            "OK" => 100,
            "Bad" => 50,
            _ => 0
        };
    }
}
