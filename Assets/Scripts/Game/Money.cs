using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Money : NetworkBehaviour
{
    public int money;

    [SerializeField] private Text moneyText;
    [SerializeField] private GameObject moneyChangePanel;
    [SerializeField] private Text moneyChangeText;

    private void UpdateMoneyText()
    {
        moneyText.text = money.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + "$";
    }

    private void MoneyChangeAmount(int amount)
    {
        if (IsInvoking(nameof(CloseMoneyChangePanel))) CancelInvoke(nameof(CloseMoneyChangePanel));

        if (amount >= 0)
        {
            moneyChangeText.color = Color.green;
            moneyChangeText.text = "+" + amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + "$";
        }
            
        else
        {
            moneyChangeText.color = Color.red;
            moneyChangeText.text = amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + "$";
        }

        moneyChangePanel.SetActive(true);

        Invoke(nameof(CloseMoneyChangePanel), 3f);
    }
    private void CloseMoneyChangePanel()
    {
        moneyChangePanel.SetActive(false);
    }

    [Server]
    public void UpdateMoney(bool moneyChanged, int oldMoney, int newMoney)
    {
        money = newMoney;
        TargetUpdateMoney(moneyChanged, oldMoney, newMoney);
    }

    [TargetRpc]
    private void TargetUpdateMoney(bool moneyChanged, int oldMoney, int newMoney)
    {
        if(moneyChanged) MoneyChangeAmount(newMoney - oldMoney);

        money = newMoney;
        UpdateMoneyText();
    }
}
