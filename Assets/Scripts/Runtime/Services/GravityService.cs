using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class GravityService : MonoBehaviour, 
                                     IEngineService, 
                                     IStartEngineEventHandler, 
                                     IFixedUpdateEngineEventHandler
{
    public bool isEnabled = true;
    public bool IsEnabled => isEnabled;
    
    [Space]
    [Tooltip("Общее количество объектов")]
    [Range(2, 250)]public byte objects = 100;

    [Tooltip("Масса каждого объекта")]
    [Range(5f, 20f)]public float mass = 5f;

    [Tooltip("Заполнение. 0-Кубы, 1-Сферы")]
    [Range(0f, 1f)]public float cubeOrSphereRatio = .5f;

    [Tooltip("Цвета при столкновении. \nВыбираются рандомно")]
    public Color32[] colors = { Color.red, Color.green, Color.blue };

    [Header("Optimization")]
    [Tooltip("Включить Graphics.DrawMesh \nDraws the same mesh multiple times using GPU instancing.")]
    public bool GPUInstance = true;
    public Material mainMaterial;

    public Transform GravityPoint { get; private set; }
    public GameObject ObjectsParent { get; private set; }
    
    readonly List<GravityObject> m_gravityObjects = new List<GravityObject>();

    // for DI
    readonly ClashesCounterService m_clashesCounterService;
    readonly MeshRendererService m_meshRendererService;

    void OnValidate()
    {
        if (Application.isPlaying == false) return;

        if (ObjectsParent == null) return;

        var mrs = m_meshRendererService;
        if (mrs == null) return;
        if (mrs.enabled == GPUInstance) return;
        mrs.enabled = GPUInstance;
        mrs.StopRefreshing();
        mrs.StartCoroutine(mrs.RefreshGPUInstances());
    }

    void IStartEngineEventHandler.OnStart()
        => StartCoroutine(CreateObjects());

    // is called before the first frame update
    IEnumerator CreateObjects()
    {
        isEnabled = false;
        yield return CreateGravityPoint();
        yield return CreateGravityObjects();
        isEnabled = true;
    }

    void IFixedUpdateEngineEventHandler.OnFixedUpdate()
    {
        float delta_time = Time.fixedDeltaTime;
        
        for (int i = 0, i_max = m_gravityObjects.Count; i < i_max; i++)
        {
            m_gravityObjects[i].OnMovementUpdate(true, delta_time);
        }
    }

    private IEnumerator CreateGravityPoint()
    {
        var g_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g_obj.name = "GravityPoint";
        GravityPoint = g_obj.transform;
        Destroy(g_obj.GetComponent<Collider>());
        yield return null;
    }

    private IEnumerator CreateGravityObjects()
    {
        ObjectsParent = new GameObject("CreatedGravityObjectsParent");
        const string k_name_prefix = nameof(GravityObject);

        for (byte i = 0; i < objects; i++)
        {
            var g_obj = new GameObject($"{k_name_prefix}_{i}");
            g_obj.transform.SetParent(ObjectsParent.transform);
            g_obj.transform.position = (Random.insideUnitSphere * Random.Range(-100f, 100f));

            var g_comp = g_obj.AddComponent<GravityObject>();
            g_comp.Init(this, m_clashesCounterService, m_meshRendererService);
            m_gravityObjects.Add(g_comp);
            yield return null;
        }

        if (GPUInstance)
        {
            m_meshRendererService.Init();
            yield return m_meshRendererService.RefreshGPUInstances();
        }

        yield return null;
    }
};
