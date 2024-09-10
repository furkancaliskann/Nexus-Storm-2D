using Mirror;
using UnityEngine;

public class Footsteps : NetworkBehaviour
{
    PlayerMovement playerMovement;

    [SerializeField] private AudioSource footstepsAudioSource;
    [SerializeField] private AudioClip clip1;
    [SerializeField] private AudioClip clip2;

    private int index = 0;
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }
    void Update()
    {
        CheckSounds();
    }
    private void CheckSounds()
    {
        if(playerMovement.movementType == MovementTypes.Run)
        {
            if(!footstepsAudioSource.isPlaying)
            {
                if(index == 0)
                {
                    footstepsAudioSource.PlayOneShot(clip1);
                    index = 1;
                }
                else
                {
                    footstepsAudioSource.PlayOneShot(clip2);
                    index = 0;
                }
            }
        }
    }

}
