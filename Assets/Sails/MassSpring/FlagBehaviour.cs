using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Node
{
    public Vector3 localPos;
    public Vector3 velocity;
    public Vector3 force;
    public int isFixed;
}

public struct Spring
{
    public int nodeA;
    public int nodeB;
    public float stiffness;
    public float length;
    public float length0;
}

public class FlagBehaviour : MonoBehaviour
{
    [SerializeField] ComputeShader _compute;

    //Procedural Mesh Params
    [SerializeField] private int _n = 20, _m = 10;
    [SerializeField] private float _horizontalSpacing = 1.0f, _verticalSpacing = 1.0f;
    private Vector3[] _vertices;
  
    private int[] _triangles;
    Mesh _mesh;

    // Mass Spring Params
    private Node[] _nodes;
    [SerializeField] private List<Spring> _springs;
    
    [SerializeField] private float _mass = 0.1f;
    [SerializeField] private float _stiffness = 5.0f;
    [SerializeField] private float _damping = 0.1f;
    [SerializeField]private float _timeStep = 1.0f;	
    
    private void InitilizeMesh()
    {
        _mesh = gameObject.GetComponent<MeshFilter>().mesh;

        _vertices = new Vector3[_n * _m];
        _nodes = new Node[_n * _m];
        // Procedural Verts
        for (int i = 0; i < _n; i++)
            for (int j = 0; j < _m; j++)
            {
                var idx = i * _m + j;
                var newNode = new Node();
                var newPos = new Vector3(i * _horizontalSpacing, j * _verticalSpacing, 1);
                newNode.localPos = newPos;
                
                if (j == _m - 1)
                    newNode.isFixed = 1;
                else
                    newNode.isFixed = 0;

                _nodes[idx] = newNode;
                _vertices[idx] = newPos;
            }

        _triangles = new int[(_n-1) * (_m-1) * 2 * 3];
        // Procedural Tris
        int t = 0;
        for (int i = 0; i < _n-1; i++)
            for (int j = 0; j < _m-1; j++) 
            {
                var index = i*_m + j;
                _triangles[t] = index;
                _triangles[t + 1] = index + 1;
                _triangles[t + 2] = index + _m;
                t += 3;
                _triangles[t] = index + 1;
                _triangles[t + 1] = index + _m+1;
                _triangles[t + 2] = index + _m;
                t += 3;
            }
        
        _springs = new List<Spring>();
        // Procedural Springs
        for (int i = 0; i < _n; i++)
            for (int j = 0; j < _m; j++)
            {
                var contador = i * _m + j;
                if (contador % _m != 0)
                {
                    // Vertical Springs
                    CreateSpring(contador, contador - 1, false);
                }
                if(contador >= _m)
                {
                    // Horizontal Springs
                    CreateSpring(contador, contador - _m, false);
                   
                    //Oblicue
                    if (contador % _m != 0)
                        CreateSpring(contador, contador - _m - 1, false);
                    

                    if (contador % _m != _m - 1)
                        CreateSpring(contador, contador - _m + 1, true);
                    
                } 
            }

        print(_springs.Count);
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.RecalculateNormals();
        
    }

    private void DispatchKernel()
    {
        var nodesBufferSize = (sizeof(float) * 3 * 3) + sizeof(int);

        ComputeBuffer nodesBuffer = new ComputeBuffer(_nodes.Length, nodesBufferSize, ComputeBufferType.Raw);
        nodesBuffer.SetData(_nodes);

        var springsBufferSize = (sizeof(int) * 2) + (sizeof(float) * 3);
        ComputeBuffer springsBuffer = new ComputeBuffer(_springs.Count, springsBufferSize, ComputeBufferType.Raw);
        springsBuffer.SetData(_springs);

        //print(_vertices[0]);
        //_compute.SetBuffer(1, "springs", springsBuffer);
        _compute.SetBuffer(0, "nodes", nodesBuffer);
        //_compute.Dispatch(1, 712, 1, 1);
        _compute.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        _compute.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);
        _compute.SetFloat("_TimeStep", _timeStep);
        _compute.SetFloat("_Mass", _mass);
        _compute.SetFloat("_Damping", _damping);
        _compute.Dispatch(0, _n, _m, 1);

        nodesBuffer.GetData(_nodes);

        for(int i= 0; i < _n; i++)
            for(int j = 0; j < _m; j++)
            {
                var idx = i * _m + j;
                _vertices[idx] = _nodes[idx].localPos;
            }

        _mesh.SetVertices(_vertices);

        print(_vertices[0] + "\n" + _vertices[1] + "\n" + _vertices[2]);

        nodesBuffer.Release();
    }

    private void CreateSpring(int nodeA, int nodeB, bool isCompressive)
    {
        var factor = (isCompressive) ? 0.5f : 1.0f;;
        var aux = (_vertices[nodeA] - _vertices[nodeB]).magnitude;

        var spr = new Spring();
        spr.nodeA = nodeA;
        spr.nodeB = nodeB;
        spr.stiffness = _stiffness * factor;
        spr.length = aux;
        spr.length0 = aux;

        _springs.Add(spr);
    }

    private void CalculateSprings()
    {

    }
    void Awake()
    {
        InitilizeMesh();

        //DispatchKernel();
    }

    void FixedUpdate()
    {
        DispatchKernel();
    }
}
