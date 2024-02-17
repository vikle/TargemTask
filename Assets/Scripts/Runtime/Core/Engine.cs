using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public sealed class Engine : MonoBehaviour
{
    IEngineService[] m_services;

    // is called before the first frame update
    void Awake()
    {
        InitServices();
        InjectDependencies();
    }

    private void InitServices()
    {
        m_services = GetComponentsInChildren<IEngineService>(true);
    }

    private void InjectDependencies()
    {
        const BindingFlags k_bnd_flags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        foreach (var service in m_services)
        {
            foreach (var field in service.GetType().GetFields(k_bnd_flags))
            {
                var field_type = field.FieldType;
                
                foreach (var value in m_services)
                {
                    if (field_type != value.GetType()) continue;
                    field.SetValue(service, value);
                    break;
                }
            }
        }
    }
    
    // is called once per frame
    void Update()
    {

    }
};
