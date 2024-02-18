using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class MeshRendererService : MonoBehaviour, 
                                          IEngineService,
                                          IAwakeEngineEventHandler,
                                          IUpdateEngineEventHandler
{
    public bool isEnabled = true;
    public bool IsEnabled => isEnabled;
    
    [Space]
    [Range(128, 1023), Tooltip("Лимит объектов на рендер за один проход")]
    public int matrixLimit = 512;
    public bool receiveShadows = true;
    public ShadowCastingMode castShadows = ShadowCastingMode.On;

    // for DI
    readonly GravityService m_gravityService;
    
    
    private sealed class MeshData
    {
        public Mesh mesh;
        public Color color = Color.white;
        internal MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
        public List<Transform> transforms = new List<Transform>();
        internal List<Matrix4x4> rendMatrix = new List<Matrix4x4>();

        public void UpdateRenderMatrix()
        {
            rendMatrix.Clear();

            for (int i = 0, i_max = transforms.Count; i < i_max; i++)
            {
                rendMatrix.Add(transforms[i].localToWorldMatrix);
            }
        }
    };

    readonly List<MeshData> m_meshes = new List<MeshData>();


    Material m_mainMaterial;
    bool m_refreshing;
    GravityObjectElement[] m_sceneObjects;
    static readonly int sr_color = Shader.PropertyToID("_Color");


    void IAwakeEngineEventHandler.OnAwake()
        => isEnabled = false;

    public void Init()
    {
        isEnabled = true;
        m_mainMaterial = m_gravityService.mainMaterial;
        m_sceneObjects = m_gravityService.ObjectsParent.GetComponentsInChildren<GravityObjectElement>(true);
    }

    // is called once per frame
    void IUpdateEngineEventHandler.OnUpdate()
    {
        if (m_mainMaterial == null) return;

        for (int i = 0, i_max = m_meshes.Count; i < i_max; i++)
        {
            var mesh = m_meshes[i];
            mesh.UpdateRenderMatrix();
            
            Graphics.DrawMeshInstanced(mesh.mesh, 
                                       0, 
                                       m_mainMaterial, 
                                       mesh.rendMatrix, 
                                       mesh.materialBlock, 
                                       castShadows, 
                                       receiveShadows, 
                                       0, 
                                       null);
        }
    }

    public void StopRefreshing()
    {
        StopAllCoroutines();
        m_refreshing = false;
    }

    public IEnumerator RefreshGPUInstances()
    {
        if (m_refreshing) yield break;
        m_refreshing = true;
        if (m_sceneObjects == null) Init();
        yield return null;
        m_meshes.Clear();

        bool rend_enabled = !m_gravityService.GPUInstance;

        for (int i = 0, i_max = m_sceneObjects.Length; i < i_max; i++)
        {
            var scn_obj = m_sceneObjects[i];
            scn_obj.Render.enabled = rend_enabled;

            if (rend_enabled) continue;

            bool have_it = false;

            for (int j = 0, j_max = m_meshes.Count; j < j_max; j++)
            {
                var mesh = m_meshes[j];
                if (mesh.transforms.Count >= matrixLimit) continue;
                if (mesh.mesh != scn_obj.MeshFilter.sharedMesh || mesh.color != scn_obj.Color) continue;
                mesh.transforms.Add(scn_obj.transform);
                have_it = true;
                break;
            }

            if (have_it) continue;

            var md = new MeshData
            {
                mesh = scn_obj.MeshFilter.sharedMesh,
                color = scn_obj.Color
            };

            md.materialBlock.SetColor(sr_color, md.color);
            md.transforms.Add(scn_obj.transform);

            m_meshes.Add(md);
        }

        yield return null;
        m_refreshing = false;
    }
};
