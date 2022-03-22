using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    public Text clashesLabel;
    public Text timeLabel;


    void Awake() => RefreshUI();

    public void OnResetCounters() => CountersController.Get.ResetData();
    public void RefreshUI()
    {
        RefreshClashesText();
        RefreshTimeText();
    }
    
    public void RefreshClashesText() => clashesLabel.text = $"Столкновений: {CountersController.Get.clashes}";
    public void RefreshTimeText() => timeLabel.text = $"Прошло времени: {CountersController.Get.timeInSeconds}";
};
