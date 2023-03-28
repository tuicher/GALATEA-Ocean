using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudContainerEdgePainter : MonoBehaviour
{
    [SerializeField] bool  _paintEdges = true;
    [SerializeField] Color _color = new Color(255, 132, 238, 255);
    
    void OnDrawGizmos()
    {
        if(_paintEdges)
        {
            Gizmos.color = _color;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
