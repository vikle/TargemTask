using UnityEngine;

public class GravityObjectElement : MonoBehaviour
{
    internal GravityObject owner;
    
    Transform m_Transform;
    Collider m_Coll;
    Vector3 m_PrevPos;
    
    
    // is called before the first frame update
    public void Init()
    {
        m_Transform = transform;
        m_PrevPos = m_Transform.position;
        m_Coll = GetComponent<Collider>();
    }

    public void SetColliderActive( bool value ) => m_Coll.enabled = value;

    public void OnBoxCast( float deltaTime )
    {
        var velocity = m_Transform.position - m_PrevPos;
        var bounds = m_Coll.bounds;

        if( Physics.BoxCast( bounds.center, bounds.extents, velocity.normalized
            , out var hit
            , m_Transform.rotation, velocity.magnitude ) )
        {
            var other = hit.collider.GetComponent<GravityObjectElement>();
            if( other != null && other.owner != owner )
            {
                var impulse_dir = velocity.normalized;
                var impulse = impulse_dir * owner.mass / deltaTime;

                float mult = Random.value;
                other.owner.impulse = impulse * ( mult * 1.5f );
                owner.impulse = impulse * -mult;
                
                other.owner.OnMovementUpdate( false, deltaTime );

                other.owner.torque = impulse_dir;
                owner.torque = -impulse_dir;
                
                other.owner.SelectRandomAnchor();
                
                other.ChangeColor();
                ChangeColor();
                
                other.owner.SetCollidersActive( false );
                owner.SetCollidersActive( false );

                CountersController.Get.AddClashe();
            }
        }

        m_PrevPos = m_Transform.position;
    }

    public void ChangeColor()
    {
        var mats = GameController.Get.materials;
        if( mats.Length == 0 ) return;
        
        var rend = GetComponent<MeshRenderer>();
        rend.sharedMaterial = mats[ Random.Range( 0, mats.Length ) ];
        
        if( GameController.Get.GPUInstance && GPUDrawer.Get != null )
        {
            GPUDrawer.Get.StartCoroutine( GPUDrawer.Get.RefreshGPUInstances() );
        }
    }
};
