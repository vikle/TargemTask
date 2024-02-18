using UnityEngine;

public sealed class GravityObjectElement : MonoBehaviour
{
    public GravityObject owner;
    public Color Color { get; private set; } = Color.white;
    public MeshRenderer Render { get; private set; }
    public MeshFilter MeshFilter { get; private set; }

    Transform m_transform;
    Collider m_coll;
    Vector3 m_prevPos;

    // for DI
    GravityService m_gravityService;
    ClashesCounterService m_clashesCounterService;
    MeshRendererService m_meshRendererService;
    
    // is called before the first frame update
    public void Init(GravityService gravityService, 
                     ClashesCounterService clashesCounterService, 
                     MeshRendererService meshRendererService)
    {
        m_gravityService = gravityService;
        m_clashesCounterService = clashesCounterService;
        m_meshRendererService = meshRendererService;

        m_transform = transform;
        m_prevPos = m_transform.position;
        m_coll = GetComponent<Collider>();
        Render = GetComponent<MeshRenderer>();
        MeshFilter = GetComponent<MeshFilter>();
    }

    public void SetColliderActive(bool value) 
        => m_coll.enabled = value;

    public void OnBoxCast(float deltaTime)
    {
        var velocity = (m_transform.position - m_prevPos);
        var bounds = m_coll.bounds;

        if (Physics.BoxCast(bounds.center, 
                            bounds.extents, 
                            velocity.normalized, 
                            out var hit, 
                            m_transform.rotation, 
                            velocity.magnitude))
        {
            var other = hit.collider.GetComponent<GravityObjectElement>();
            if (other != null && other.owner != owner)
            {
                var impulse_dir = velocity.normalized;
                var impulse = impulse_dir * owner.mass / deltaTime;

                float mult = Random.value;
                other.owner.Impulse = (impulse * (mult * 1.5f));
                owner.Impulse = (impulse * -mult);

                other.owner.OnMovementUpdate(false, deltaTime);

                other.owner.Torque = impulse_dir;
                owner.Torque = -impulse_dir;

                other.owner.SelectRandomAnchor();

                other.ChangeColor();
                ChangeColor();

                other.owner.SetCollidersActive(false);
                owner.SetCollidersActive(false);

                m_clashesCounterService.AddClash();
            }
        }

        m_prevPos = m_transform.position;
    }

    public void ChangeColor()
    {
        var colors = m_gravityService.colors;
        if (colors.Length == 0) return;

        Color = colors[Random.Range(0, colors.Length)];

        if (m_gravityService.GPUInstance && m_meshRendererService != null)
        {
            m_meshRendererService.StartCoroutine(m_meshRendererService.RefreshGPUInstances());
        }

        Render.material.SetColor("_Color", Color);
    }
};
