public interface IEngineEventHandler
{
    bool IsEnabled { get; }
};

public interface IAwakeEngineEventHandler : IEngineEventHandler
{
    void OnAwake();
};
    
public interface IStartEngineEventHandler : IEngineEventHandler
{
    void OnStart();
};
    
public interface IUpdateEngineEventHandler : IEngineEventHandler
{
    void OnUpdate();
};
    
public interface IFixedUpdateEngineEventHandler : IEngineEventHandler
{
    void OnFixedUpdate();
};