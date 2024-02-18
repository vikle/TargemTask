using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public sealed class Engine : MonoBehaviour
{
    IEngineService[] m_services;
    IEngineEventHandler[] m_allHandlers;
    IUpdateEngineEventHandler[] m_updateHandlers;
    IFixedUpdateEngineEventHandler[] m_fixedUpdateHandlers;
    
    
    void Awake()
    {
        InitServices();
        InjectDependencies();
        InitHandlers();
        OnAwake();
    }

    private void InitServices()
    {
        m_services = GetComponentsInChildren<IEngineService>(true);
    }

    private void InjectDependencies()
    {
        var services_types = m_services.Select(s => s.GetType()).ToArray();
        
        foreach (var service in m_services)
        {
            Inject(service, services_types);
        }
    }

    private void Inject(object instance, Type[] servicesTypes)
    {
        const BindingFlags k_bnd_flags = BindingFlags.Instance 
                                        | BindingFlags.NonPublic 
                                        | BindingFlags.DeclaredOnly;

        for (int i = 0, i_max = m_services.Length; i < i_max; i++)
        {
            var service = m_services[i];
            
            foreach (var field in servicesTypes[i].GetFields(k_bnd_flags))
            {
                if (field.FieldType.IsInstanceOfType(instance))
                {
                    field.SetValue(service, instance);
                }
            }
        }
    }

    private void InitHandlers()
    {
        m_allHandlers = GetComponentsInChildren<IEngineEventHandler>(true);
        
        m_updateHandlers = m_allHandlers
                           .Where(h => h is IUpdateEngineEventHandler)
                           .Cast<IUpdateEngineEventHandler>()
                           .ToArray();
        
        m_fixedUpdateHandlers = m_allHandlers
                                .Where(h => h is IFixedUpdateEngineEventHandler)
                                .Cast<IFixedUpdateEngineEventHandler>()
                                .ToArray();
    }

    void OnAwake()
    {
        for (int i = 0, i_max = m_allHandlers.Length; i < i_max; i++)
        {
            if (m_allHandlers[i] is IAwakeEngineEventHandler system)
            {
                system.OnAwake();
            }
        }
    }
    
    void Start()
    {
        for (int i = 0, i_max = m_allHandlers.Length; i < i_max; i++)
        {
            if (m_allHandlers[i] is IStartEngineEventHandler system)
            {
                system.OnStart();
            }
        }
    }

    void Update()
    {
        for (int i = 0, i_max = m_updateHandlers.Length; i < i_max; i++)
        {
            var handler = m_updateHandlers[i];
            if (handler.IsEnabled) handler.OnUpdate();
        }
    }

    void FixedUpdate()
    {
        for (int i = 0, i_max = m_fixedUpdateHandlers.Length; i < i_max; i++)
        {
            var handler = m_fixedUpdateHandlers[i];
            if (handler.IsEnabled) handler.OnFixedUpdate();
        }
    }
};
