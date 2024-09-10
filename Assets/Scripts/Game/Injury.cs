using Mirror;
using UnityEngine;

public class Injury : NetworkBehaviour
{
    [SerializeField] private AudioSource getDamageAudioSource;
    [SerializeField] private AudioClip getDamageSound;

    [SerializeField] private Animation injuryEffect;
    

    [ClientRpc]
    public void RpcPlayGetDamageSound()
    {
        getDamageAudioSource.PlayOneShot(getDamageSound);
    }

    [TargetRpc]
    public void TargetPlayInjuryEffect()
    {
        if(injuryEffect.isPlaying) injuryEffect.Stop();

        injuryEffect.Play("Injury");
    }
}
