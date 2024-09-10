using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class AmmoText : NetworkBehaviour
{
    [SerializeField] private GameObject ammoPanel;
    [SerializeField] private Text ammoText;

    [TargetRpc]
    public void TargetUpdatePanel(int ammoInside, int totalAmmo)
    {
        if (!ammoPanel.activeInHierarchy) ammoPanel.SetActive(true);

        ammoText.text = ammoInside + " / " + totalAmmo;
    }
    [TargetRpc]
    public void TargetClosePanel()
    {
        if(ammoPanel.activeInHierarchy)
            ammoPanel.SetActive(false);
    }
}
