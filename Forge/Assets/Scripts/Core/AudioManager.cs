using UnityEngine;


// Simple centralised audio manager.
// Assign AudioClips in the Inspector. All calls are null-safe so the game
// works without audio assets while in development.

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip snapSound;
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip goalCompleteSound;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

    private AudioSource _source;

    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
    }

    //Public API
    public void PlaySnap()          => Play(snapSound);
    public void PlayPurchase()      => Play(purchaseSound);
    public void PlayError()         => Play(errorSound);
    public void PlayGoalComplete()  => Play(goalCompleteSound);

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip, sfxVolume);
    }
}
