using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSystem : MonoBehaviour
{
    [SerializeField]
    private AudioClip collisionSFX;

    [SerializeField]
    private AudioClip eliminationSFX;

    [SerializeField]
    private AudioClip tileDestroyedSFX;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameSystem.Instance.OnPlayerCollision += OnPlayerCollision;
        GameSystem.Instance.OnPlayerEliminated += OnPlayerEliminated;
        GameSystem.Instance.OnTileDestroyed += OnTileDestroyed;
    }

    private void OnPlayerCollision(object sender, EventArgs args)
    {
        audioSource.PlayOneShot(collisionSFX);
    }

    private void OnPlayerEliminated(object sender, EventArgs args)
    {
        audioSource.PlayOneShot(eliminationSFX);
    }

    private void OnTileDestroyed(object sender, EventArgs args)
    {
        audioSource.PlayOneShot(tileDestroyedSFX);
    }
}
