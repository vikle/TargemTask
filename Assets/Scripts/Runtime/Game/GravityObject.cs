using System.Collections.Generic;
using UnityEngine;

public class GravityObject : MonoBehaviour
{
    [Range( 5f, 30f )]
    public float mass = 5f;
    
    internal Vector3 velocity { get; private set; }
    internal Vector3 impulse { get; set; }
    internal Vector3 torque { get; set; }

    
    Transform m_Transform, m_Anchor;
    static Transform s_GravityPoint;
    bool m_CollidersEnabled = true;
   
    readonly List<GravityObjectElement> mr_Elements = new List<GravityObjectElement>();
    
    
    public void Init()
    {
        m_Transform = transform;
        if( s_GravityPoint == null ) s_GravityPoint = GravityPoint.Get.getTransform;
        torque = Random.rotation.eulerAngles;
        mass = GameController.Get.mass;

        CreateElements();
        SelectRandomAnchor();
    }

    public void SetCollidersActive( bool value )
    {
        if( m_CollidersEnabled == value ) return;
        m_CollidersEnabled = value;
        mr_Elements.ForEach( e => e.SetColliderActive( value ) );
    }

    private void CreateElements()
    {
        for( float x = -1.5f; x < 1.5f; x++ )
        {
            for( float y = -2f; y < 2f; y++ )
            {
                for( float z = -1.5f; z < 1.5f; z++ )
                {
                    if( Random.value > .4f ) continue;
                    bool is_cube = ( Random.value > GameController.Get.cubeOrSphereRatio );
                    var g_obj = GameObject.CreatePrimitive( is_cube ? PrimitiveType.Cube : PrimitiveType.Sphere );
                    g_obj.transform.SetParent( transform );
                    
                    var offset = new Vector3( x, y, z );
                    g_obj.transform.localPosition = offset;
                    g_obj.name += offset;
                    
                    var g_comp = g_obj.AddComponent<GravityObjectElement>();
                    g_comp.owner = this;
                    g_comp.Init();
                    mr_Elements.Add( g_comp );
                }
            }   
        }
    }

    public void SelectRandomAnchor()
    {
        var prev_anchor = m_Anchor;
        void on_next_anchor() => m_Anchor = mr_Elements[ Random.Range( 0, mr_Elements.Count ) ].transform;

        do
        {
            on_next_anchor();
        } while( prev_anchor == m_Anchor );
    }

    
    public void OnMovementUpdate( bool boxCast, float deltaTime )
    {
        var anchor_position = m_Anchor.position;
        anchor_position += ( m_Transform.position - anchor_position );

        float distance = Vector3.Distance( anchor_position, s_GravityPoint.position );
        float t_gravity = deltaTime * ( mass / distance ) * 2f;
        var next_position = Vector3.Lerp( anchor_position, s_GravityPoint.position, t_gravity );

        if( boxCast )
        {
            for( int i = 0; i < mr_Elements.Count; i++ )
            {
                mr_Elements[ i ].OnBoxCast( deltaTime );
            }
        }
        
        var impulse_pos = s_GravityPoint.position + impulse;
        next_position = Vector3.Lerp( next_position, impulse_pos, deltaTime / 2f );
        impulse = Vector3.Lerp( impulse, Vector3.zero, deltaTime * mass / 2f );
        
        if( m_CollidersEnabled == false && impulse.magnitude < 10f )
        {
            SetCollidersActive( true );
        }
        
        velocity = next_position - anchor_position;
        m_Transform.position = next_position;
        m_Transform.RotateAround( m_Anchor.position, torque, deltaTime * 100f );
    }
};
