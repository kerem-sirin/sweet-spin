using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    public class PaytableEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image symbolIcon;
        [SerializeField] private TextMeshProUGUI payout3x;
        [SerializeField] private TextMeshProUGUI payout4x;
        [SerializeField] private TextMeshProUGUI payout5x;

        public void SetData(SymbolData symbolData)
        {
            if (symbolIcon != null && symbolData.sprite != null)
            {
                symbolIcon.sprite = symbolData.sprite;
                symbolIcon.enabled = true;
            }
            else if (symbolIcon != null)
            {
                symbolIcon.enabled = false;
            }

            if (payout3x != null)
            {
                payout3x.text = symbolData.payouts[0].ToString();
            }

            if (payout4x != null)
            {
                payout4x.text = symbolData.payouts[1].ToString();
            }

            if (payout5x != null)
            {
                payout5x.text = symbolData.payouts[2].ToString();
            }
        }
    }
}