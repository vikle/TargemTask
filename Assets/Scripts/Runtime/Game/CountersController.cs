using UnityEngine;

public class CountersController : Singleton<CountersController>
{
    public int clashes { get; private set; }
    public int timeInSeconds { get; private set; }

    
    float t_elapsed;
    // is called once per frame
    void Update()
    {
        t_elapsed += Time.deltaTime;
        if( t_elapsed < 1f ) return;
        t_elapsed = 0f;
        timeInSeconds++;
        UIController.Get.RefreshTimeText();
    }
    
    
    public void AddClashe()
    {
        clashes++;
        UIController.Get.RefreshClashesText();
    }

    public void ResetData()
    {
        clashes = 0;
        t_elapsed = 0f;
        timeInSeconds = 0;
        UIController.Get.RefreshUI();
    }
};
