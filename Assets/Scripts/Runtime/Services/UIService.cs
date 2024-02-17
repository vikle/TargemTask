using UnityEngine;
using UnityEngine.UI;

public sealed class UIService : MonoBehaviour, IEngineService
{
    public Text clashesLabel;
    public Text timeLabel;

    // for DI
    readonly ClashesCounterService m_clashesCounterService;
    
    
    void Start() 
        => RefreshUI();

    public void OnResetCounters() 
        => m_clashesCounterService.ResetData();
    
    public void RefreshUI()
    {
        RefreshClashesText();
        RefreshTimeText();
    }
    
    public void RefreshClashesText() 
        => clashesLabel.text = $"Столкновений: {m_clashesCounterService.Clashes}";
    
    public void RefreshTimeText() 
        => timeLabel.text = $"Прошло времени: {m_clashesCounterService.TimeInSeconds}";
};
