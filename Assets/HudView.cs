using TMPro;
using UnityEngine;

public class HudView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text quotaText;
    [SerializeField] private TMP_Text energyText;
    
    public void SetScore(int value) => scoreText.text = $"Очки: {value}";
    public void SetGold(int value) => goldText.text = $"Золото: {value}";
    public void SetTurn(int value) => turnText.text = $"Ход: {value}";
    public void SetRound(int value) => roundText.text = $"Раунд: {value}";
    public void SetQuota(int value) => quotaText.text = $"Квота: {value}";
    public void SetEnergy(int value) => energyText.text = $"Энергия: {value}";
}