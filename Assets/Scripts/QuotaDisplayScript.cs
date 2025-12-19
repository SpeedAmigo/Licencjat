using FishNet.Object;
using TMPro;
using UnityEngine;

public class QuotaDisplayScript : NetworkBehaviour
{
    [SerializeField] private TMP_Text currentQuotaText;
    [SerializeField] private TMP_Text targetQuotaText;
    
    private void OnEnable()
    {
        QuotaManagerScript.OnMoneyChanged += UpdateCurrentMoney;
        QuotaManagerScript.OnTargetQuotaChanged += UpdateTargetQuota;
    }

    private void OnDisable()
    {
        QuotaManagerScript.OnMoneyChanged -= UpdateCurrentMoney;
        QuotaManagerScript.OnTargetQuotaChanged -= UpdateTargetQuota;
    }

    private void UpdateCurrentMoney(uint currentMoney)
    {
        UpdateCurrentMoneyClients(currentMoney);
    }

    [ObserversRpc(BufferLast = true)]
    private void UpdateCurrentMoneyClients(uint currentMoney)
    {
        currentQuotaText.text = currentMoney.ToString();
    }

    private void UpdateTargetQuota(uint targetQuota)
    {
        UpdateTargetQuotaClients(targetQuota);
    }

    [ObserversRpc(BufferLast = true)]
    private void UpdateTargetQuotaClients(uint targetQuota)
    {
        targetQuotaText.text = targetQuota.ToString();
    }
}
