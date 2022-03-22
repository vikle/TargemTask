using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUDrawer : Singleton<GPUDrawer>
{
    [Range( 128, 1023 )]
    public int matrixLimit = 512;
    public bool receiveShadows = true;
    public ShadowCastingMode castShadows = ShadowCastingMode.On;
    
    [System.Serializable]
    public sealed class MeshData
    {
        public Mesh mesh;
        public Color color = Color.white;
        internal MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
        public List<Transform> transforms = new List<Transform>();
        internal List<Matrix4x4> rendMatrix = new List<Matrix4x4>();

        internal void UpdateRenderMatrix()
        {
            rendMatrix.Clear();
            rendMatrix.Capacity = transforms.Count;
            for( int i = 0; i < transforms.Count; i++ )
            {
                rendMatrix.Add( transforms[i].localToWorldMatrix );
            }
        }
    };
    
    public List<MeshData> meshes = new List<MeshData>();
    
    
    Material m_MainMaterial;
    bool m_Refreshing;
    GravityObjectElement[] m_SceneObjects;
    
    
    void Awake() => enabled = false;
    public void Init()
    {
        enabled = true;
        m_MainMaterial = GameController.Get.mainMaterial;
        m_SceneObjects = GameController.Get.objectsParent.GetComponentsInChildren<GravityObjectElement>( true );
    }

    // is called once per frame
    void Update()
    {
        if( m_MainMaterial == null ) return;

        for( int i = 0; i < meshes.Count; i++ )
        {
            meshes[i].UpdateRenderMatrix();
            
            Graphics.DrawMeshInstanced( 
                meshes[i].mesh
                , 0
                , m_MainMaterial
                , meshes[i].rendMatrix
                , meshes[i].materialBlock
                , castShadows
                , receiveShadows
                , 0
                , null 
            );
        }
    }

    public void StopRefreshing()
    {
        StopAllCoroutines();
        m_Refreshing = false;
    }
    
    public IEnumerator RefreshGPUInstances()
    {
        if( m_Refreshing ) yield break;
        m_Refreshing = true;
        if( m_SceneObjects == null ) Init();
        yield return null;
        meshes.Clear();

        bool rend_enabled = !GameController.Get.GPUInstance;
        
        for( int i = 0; i < m_SceneObjects.Length; i++ )
        {
            m_SceneObjects[i].render.enabled = rend_enabled;

            if( rend_enabled ) continue;
            
            bool have_it = false;
            
            for( int j = 0; j < meshes.Count; j++ )
            {
                if( meshes[j].transforms.Count >= matrixLimit ) continue;
                if( meshes[j].mesh != m_SceneObjects[i].meshFilter.sharedMesh || meshes[j].color != m_SceneObjects[i].color ) continue;
                have_it = true;
                meshes[j].transforms.Add( m_SceneObjects[i].transform );
                break;
            }
            
            if( have_it ) continue;
            
            var md = new MeshData
            {
                mesh = m_SceneObjects[i].meshFilter.sharedMesh,
                color = m_SceneObjects[i].color
            };

            md.materialBlock.SetColor( "_Color", md.color );
            md.transforms.Add( m_SceneObjects[i].transform );

            meshes.Add( md );
        }

        yield return null;
        m_Refreshing = false;
    }
};
