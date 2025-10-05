using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource audioSource;

    [Header("Clips")]
    public AudioClip pistolShot;
    public AudioClip shotgunShot;
    public AudioClip footstep;
    public AudioClip background;
    public AudioClip zombieGroan;

    [Header("Zombie Settings")]
    public float zombieGroanCooldown = 3f; // seconds between groans
    private float lastZombieGroanTime = -10f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    public void PlayBackground()
    {
        if (background != null)
        {
            audioSource.loop = true;
            audioSource.clip = background;
            audioSource.Play();
        }
    }

    public void PlayZombieGroan(float volume = 1f)
    {
        if (zombieGroan == null)
            return;

        // Only play if cooldown has passed
        if (Time.time - lastZombieGroanTime >= zombieGroanCooldown)
        {
            audioSource.PlayOneShot(zombieGroan, volume);
            lastZombieGroanTime = Time.time;
        }
    }

    public void StopZombieGroan()
    {
        // If zombie groan is looping, stop it.
        // If you only use PlayOneShot, you can leave this empty.
        if (audioSource.isPlaying && audioSource.clip == zombieGroan)
        {
            audioSource.Stop();
        }
    }
}
