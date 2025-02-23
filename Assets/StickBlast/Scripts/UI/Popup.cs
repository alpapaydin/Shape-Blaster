using UnityEngine;
using UnityEngine.SceneManagement;
using StickBlast.Level;
public class Popup : MonoBehaviour
{
    [SerializeField] private AudioClip thudClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip tapClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayThud() { audioSource.PlayOneShot(thudClip); }
    public void PlayLose() { audioSource.PlayOneShot(loseClip); }
    public void GoToMainMenu() { LevelManager.Instance.LoadMainMenu(); }
    public void RestartLevel()
    {
        audioSource.PlayOneShot(tapClip);
        LevelManager.Instance.RestartLevel();
    }
}