using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : Singleton<GameController>
{
    [Range( 2, 250 )]
    public byte gravityObjectCount = 10;
    
    [Range( 5f, 30f )]
    public float gravityObjectMass = 15f;
    
    [Range( 0f, 1f )]
    public float cubeOrSphereRatio = .5f;
    
    public Material[] materials;

    [Header( "Optimization" )]
    public bool GPUInstance = true;
    public Material mainMaterial;
    
    public GameObject objectsParent { get; private set; }
    readonly List<GravityObject> mr_GravityObjects = new List<GravityObject>();


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
        
        for( byte i = 0; i < gravityObjectCount; i++ )
        {
            var g_obj = new GameObject( $"{name_prefix}_{i}" );
            g_obj.transform.SetParent( objectsParent.transform );
            g_obj.transform.position = Random.insideUnitSphere * Random.Range( -75f, 75f );
            
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
