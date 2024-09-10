using Mirror;
using UnityEngine;

public class RoundSounds : NetworkBehaviour
{
    private RoundSoundsVariables roundSoundsVariables;

    private AudioSource roundAudioSource2D;
    private AudioClip redTeamWinsSound;
    private AudioClip blueTeamWinsSound;
    private AudioClip roundStartedSound1, roundStartedSound2, roundStartedSound3;

    public override void OnStartAuthority()
    {
        roundSoundsVariables = Camera.main.GetComponent<RoundSoundsVariables>();

        roundAudioSource2D = roundSoundsVariables.roundAudioSource2D;
        redTeamWinsSound = roundSoundsVariables.redTeamWinsSound;
        blueTeamWinsSound = roundSoundsVariables.blueTeamWinsSound;
        roundStartedSound1 = roundSoundsVariables.roundStartedSound1;
        roundStartedSound2 = roundSoundsVariables.roundStartedSound2;
        roundStartedSound3 = roundSoundsVariables.roundStartedSound3;
    }

    [TargetRpc]
    public void TargetPlayRedTeamWinsSound()
    {
        roundAudioSource2D.PlayOneShot(redTeamWinsSound);
    }

    [TargetRpc]
    public void TargetPlayBlueTeamWinsSound()
    {
        roundAudioSource2D.PlayOneShot(blueTeamWinsSound);
    }

    [TargetRpc]
    public void TargetPlayRoundStartedSound(int index)
    {
        if (index == 0) roundAudioSource2D.PlayOneShot(roundStartedSound1);
        else if (index == 1) roundAudioSource2D.PlayOneShot(roundStartedSound2);
        else roundAudioSource2D.PlayOneShot(roundStartedSound3);
    }
}
