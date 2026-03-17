using UnityEngine;


// this class is responsible for managing the audio played when actions are performed in the game

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    // inspector fields for dragging in audio clips 
    [Header("Sound Effects")]
    [SerializeField] private AudioClip snapSound;
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip goalCompleteSound;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

    private AudioSource _source;

    // awake function destroys duplicates
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
    }

    //Public functions that allow for other objects to play sounds when the conditions have been met 
    public void PlaySnap()          => Play(snapSound);
    public void PlayPurchase()      => Play(purchaseSound);
    public void PlayError()         => Play(errorSound);
    public void PlayGoalComplete()  => Play(goalCompleteSound);

    // this function plays the audio clip if it is there and if it isnt does nothing
    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip, sfxVolume);
    }
}
