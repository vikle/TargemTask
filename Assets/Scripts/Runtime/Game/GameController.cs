using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : Singleton<GameController>
{
    [Tooltip( "Общее количество объектов" )]
    [Range( 2, 250 )]
    public byte objects = 100;
    
    [Tooltip( "Масса каждого объекта" )]
    [Range( 5f, 20f )]
    public float mass = 5f;
    
    [Tooltip( "Заполнение. 0-Кубы, 1-Сферы" )]
    [Range( 0f, 1f )]
    public float cubeOrSphereRatio = .5f;
    
    [Tooltip( "Цвета при столкновении. \nВыбираются рандомно" )]
    public Color32[] colors = new Color32[]
    {
        Color.red, Color.green, Color.blue
    };

    [Header( "Optimization" )] [Tooltip( "Включить Graphics.DrawMesh \nDraws the same mesh multiple times using GPU instancing." )]
    public bool GPUInstance = true;
    public Material mainMaterial;
    
    public GameObject objectsParent { get; private set; }
    readonly List<GravityObject> mr_GravityObjects = new List<GravityObject>();

    
    void OnValidate()
    {
        CheckGPUDrawer();

        if( Application.isPlaying == false ) return;
        
        if( objectsParent == null ) return;
        
        var gpu_drawer = GetComponent<GPUDrawer>();
        if( gpu_drawer == null ) return;
        if( gpu_drawer.enabled == GPUInstance ) return;
        gpu_drawer.enabled = GPUInstance;
        gpu_drawer.StopRefreshing();
        gpu_drawer.StartCoroutine( gpu_drawer.RefreshGPUInstances() );
    }
    
    
    // is called before the first frame update
    void Awake() => CheckGPUDrawer();

    // is called before the first frame update
    IEnumerator Start()
    {
        enabled = false;
        yield return CreateGravityPoint();
        yield return CreateGravityObjects();
        enabled = true;
    }

    void FixedUpdate() => OnUpdate( Time.fixedDeltaTime );

    private void OnUpdate( float deltaTime )
    {
        for( int i = 0; i < mr_GravityObjects.Count; i++ )
        {
            mr_GravityObjects[ i ].OnMovementUpdate( true, deltaTime );
        }
    }
    

    private void CheckGPUDrawer()
    {
        if( GPUInstance && GPUDrawer.Get == null )
        {
            gameObject.AddComponent<GPUDrawer>();
        }
    }

    private IEnumerator CreateGravityPoint()
    {
        var g_obj = GameObject.CreatePrimitive( PrimitiveType.Sphere );
        g_obj.name = nameof( GravityPoint );
        
        g_obj.AddComponent<GravityPoint>();
        
        Destroy( g_obj.GetComponent<Collider>() );
        yield return null;
    }

    private IEnumerator CreateGravityObjects()
    {
        objectsParent = new GameObject( "CreatedGravityObjectsParent" );
        string name_prefix = nameof( GravityObject );
        
        for( byte i = 0; i < objects; i++ )
        {
            var g_obj = new GameObject( $"{name_prefix}_{i}" );
            g_obj.transform.SetParent( objectsParent.transform );
            g_obj.transform.position = Random.insideUnitSphere * Random.Range( -100f, 100f );
            
            var g_comp = g_obj.AddComponent<GravityObject>();
            g_comp.Init();
            mr_GravityObjects.Add( g_comp );
            yield return null;
        }

        if( GPUInstance )
        {
            var gpu_drawer = GetComponent<GPUDrawer>();
            if( gpu_drawer == null ) gpu_drawer = gameObject.AddComponent<GPUDrawer>();
            gpu_drawer.Init();
            yield return gpu_drawer.RefreshGPUInstances();
        }
        
        yield return null;
    }
};
