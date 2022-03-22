using UnityEngine;

public class GravityPoint : Singleton<GravityPoint>
{
    Transform m_Transform;
    
    public Transform getTransform
    {
        get
        {
            if( m_Transform != null ) return m_Transform;
            return (m_Transform = transform);
        }
    }
};
