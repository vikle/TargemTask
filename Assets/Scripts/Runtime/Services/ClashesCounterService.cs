using UnityEngine;

public sealed class ClashesCounterService : MonoBehaviour, 
                                            IEngineService,
                                            IUpdateEngineEventHandler
{
    public bool isEnabled = true;
    public bool IsEnabled => isEnabled;
    
    public int Clashes { get; private set; }
    public int TimeInSeconds { get; private set; }


    float m_elapsed;

    // for DI
    readonly UIService m_uiService;
    
    // is called once per frame
    void IUpdateEngineEventHandler.OnUpdate()
    {
        m_elapsed += Time.deltaTime;
        if (m_elapsed < 1f) return;
        m_elapsed = 0f;
        TimeInSeconds++;
        m_uiService.RefreshTimeText();
    }


    public void AddClash()
    {
        Clashes++;
        m_uiService.RefreshClashesText();
    }

    public void ResetData()
    {
        Clashes = 0;
        m_elapsed = 0f;
        TimeInSeconds = 0;
        m_uiService.RefreshUI();
    }
};
