using System.Collections.Generic;
using UnityEngine;

public sealed class GravityObject : MonoBehaviour
{
    [Range(5f, 30f)]
    public float mass = 5f;

    public Vector3 Velocity { get; private set; }
    public Vector3 Impulse { get; set; }
    public Vector3 Torque { get; set; }


    Transform m_transform;
    Transform m_anchor;
    bool m_collidersEnabled = true;

    readonly List<GravityObjectElement> m_elements = new List<GravityObjectElement>();

    Transform m_gravityPoint;
    
    // for DI
    GravityService m_gravityService;
    ClashesCounterService m_clashesCounterService;
    MeshRendererService m_meshRendererService;

    public void Init(GravityService gravityService, 
                     ClashesCounterService clashesCounterService, 
                     MeshRendererService meshRendererService)
    {
        m_gravityService = gravityService;
        m_clashesCounterService = clashesCounterService;
        m_meshRendererService = meshRendererService;

        m_transform = transform;
        m_gravityPoint = gravityService.GravityPoint;
        
        Torque = Random.rotation.eulerAngles;
        mass = gravityService.mass;

        CreateElements();
        SelectRandomAnchor();
    }

    public void SetCollidersActive(bool value)
    {
        if (m_collidersEnabled == value) return;
        m_collidersEnabled = value;

        for (int i = 0, i_max = m_elements.Count; i < i_max; i++)
        {
            m_elements[i].SetColliderActive(value);
        }
    }

    private void CreateElements()
    {
        for (float x = -1.5f; x < 1.5f; x++)
        {
            for (float y = -2f; y < 2f; y++)
            {
                for (float z = -1.5f; z < 1.5f; z++)
                {
                    if (Random.value > .4f) continue;
                    
                    bool is_cube = (Random.value > m_gravityService.cubeOrSphereRatio);
                    var g_obj = GameObject.CreatePrimitive(is_cube ? PrimitiveType.Cube : PrimitiveType.Sphere);
                    g_obj.transform.SetParent(transform);

                    var offset = new Vector3(x, y, z);
                    g_obj.transform.localPosition = offset;
                    g_obj.name += offset;

                    var g_comp = g_obj.AddComponent<GravityObjectElement>();
                    g_comp.owner = this;
                    g_comp.Init(m_gravityService, m_clashesCounterService, m_meshRendererService);
                    m_elements.Add(g_comp);
                }
            }
        }
    }

    public void SelectRandomAnchor()
    {
        var prev_anchor = m_anchor;

        do
        {
            int index = Random.Range(0, m_elements.Count);
            m_anchor = m_elements[index].transform;
        } while (prev_anchor == m_anchor);
    }

    public void OnMovementUpdate(bool boxCast, float deltaTime)
    {
        var anchor_position = m_anchor.position;
        anchor_position += (m_transform.position - anchor_position);

        var target_position = m_gravityPoint.position;
        float distance = Vector3.Distance(anchor_position, target_position);
        float gravity_force = (deltaTime * (mass / distance) * 2f);
        var next_position = Vector3.Lerp(anchor_position, target_position, gravity_force);

        if (boxCast)
        {
            for (int i = 0, i_max = m_elements.Count; i < i_max; i++)
            {
                m_elements[i].OnBoxCast(deltaTime);
            }
        }

        var impulse_pos = (target_position + Impulse);
        next_position = Vector3.Lerp(next_position, impulse_pos, deltaTime / 2f);
        Impulse = Vector3.Lerp(Impulse, Vector3.zero, deltaTime * mass / 2f);

        if (m_collidersEnabled == false && Impulse.magnitude < 10f)
        {
            SetCollidersActive(true);
        }

        Velocity = (next_position - anchor_position);
        m_transform.position = next_position;
        m_transform.RotateAround(m_anchor.position, Torque, deltaTime * 100f);
    }
};
