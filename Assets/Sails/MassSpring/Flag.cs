using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum NodeType
{
    Default,
    Fixed,
    Weight
}

public class Pair
{
    public int nodeA;
    public int nodeB;

    public Pair(int nA, int nb)
    {
        nodeA = nA;
        nodeB = nb;
    }
}

[RequireComponent(typeof(MeshFilter))]
public class Flag : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private MeshCollider meshCollider;
    
    // Data Structures
    [SerializeField] private Vector3[] vertices;
    [SerializeField] private Vector3[] positions;
    private int[] triangles;
    private Vector3[] velocities;       //Node speed
    private Vector3[] forces;           //Node Forces
    private List<float> lenghts;
    private List<float> lenghts0;
    private Transform[] Fixers;
    [SerializeField] private List<Collider> Fixator;
    [SerializeField] private List<Collider> Weights;

    private Vector3 Gravity = new Vector3(0.0f, -2.0f, 0.0f);
    private Vector3[] StartPos;
    private List<Pair> springs;
    private List<float> stiffList;
    private float stiffness = 5;
    public float windDamping = 0.2f;
    public float weightMass = 2.5f; 
    
    // Magic numbers <3
    // G (0,-1,0)
    //mass = 0.03
    //stifness = 5
    //Damping = 0.1

    private float mass = 0.0075f;
    private float[] massArray;
    private float damping = 0.1f;
    private float TimeStep = 0.002f;
    private uint subSteps = 19;
    [SerializeField] private NodeType[] isFixed;
    public bool Paused;
    
    #region MeshHandler
    [SerializeField]
    private int N = 20, M = 10;
    [SerializeField]
    private float hSpacing = 1.0f, vSpacing = 1.0f;

    [SerializeField] private bool windSimulation;
    [SerializeField] private Vector3 wind = Vector3.left;
    [SerializeField] private float windSpeed = 0.4f;
    
    void Initialize()
    {
        // Initialize Arrays
        vertices = new Vector3[N * M];
        StartPos = new Vector3[N * M];
        Fixers = new Transform[N * M];
        positions = new Vector3[vertices.Length]; 
        triangles = new int[(N-1) * (M-1) * 2 * 3];
        
        // Procedural Verts
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < M; j++)
            {
                vertices[i*M + j] = new Vector3(i * hSpacing, j * vSpacing, 0);
            }
        }

        
        // Procedural Tris
        int t = 0;
        for (int i = 0; i < N-1; i++)
        {
            for (int j = 0; j < M-1; j++) 
            {
                var index = i*M + j;
                triangles[t] = index;
                triangles[t + 1] = index + 1;
                triangles[t + 2] = index + M;
                t += 3;
                triangles[t] = index + 1;
                triangles[t + 1] = index + M+1;
                triangles[t + 2] = index + M;
                t += 3;
            }
        }

        velocities = new Vector3[vertices.Length];
        forces = new Vector3[vertices.Length];
        lenghts = new List<float>();
        lenghts0 = new List<float>();
        stiffList = new List<float>();
        springs = new List<Pair>();
        isFixed = new NodeType[N * M];
        massArray = new float[N * M];

        for (int i = 0; i < isFixed.Length; i++)
        {
            isFixed[i] = NodeType.Default;
            massArray[i] = mass;
        }

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < M; j++)
            {
                var contador = i * M + j;
                if (contador % M != 0)
                {
                    // Vertical Springs
                    CreateSpring(contador, contador - 1, false);
                }
                if(contador >= M)
                {
                    // Horizontal Springs
                    
                    CreateSpring(contador, contador - M, false);
                   
                    //Oblicue
                    if (contador % M != 0)
                    {
                        CreateSpring(contador, contador - M - 1, false);
                    }

                    if (contador % M != M - 1)
                    {
                        CreateSpring(contador, contador - M + 1, true);
                    }
                }
            }
        }
        
        //FixPoint(0, pole);
        //FixPoint(M-1, pole);
        //FixPoint(N * M - 1);
        //FixPoint(N * M - M);
        
        for (int i = 0; i < vertices.Length; i++)
        {
            foreach (var fixer in Fixator)
            {
                if (isFixed[i] == NodeType.Default && fixer.bounds.Contains(transform.TransformPoint(vertices[i])))
                {
                    FixPoint(i, vertices[i]-fixer.transform.position, fixer.transform);
                }
            }
        }
        
        for (int i = 0; i < vertices.Length; i++)
        {
            foreach (var weight in Weights)
            {
                if (isFixed[i] == NodeType.Default && weight.bounds.Contains(transform.TransformPoint(vertices[i])))
                {
                    WeightPoint(i, weight.transform);
                }
            }
        }
    }

    
    void UpdateMesh()
    {
        //mesh.Clear();
        mesh.vertices = vertices;
        //mesh.triangles = triangles;
        mesh.RecalculateNormals(); //Calculamos las Normales
    }

    void CreateSpring(int nodeA, int nodeB, bool isCompressive)
    {
        var factor = (isCompressive) ? 0.5f : 1.0f;;
        
        springs.Add(new Pair(nodeA, nodeB));
        var aux = (vertices[nodeA] - vertices[nodeB]).magnitude;
        stiffList.Add(stiffness * factor);
        lenghts0.Add(aux);
        lenghts.Add(aux);
        
    }
    
    private const int k = 60;
    private const int rotationRatio = 360 * k;
    private int windDirection = 0;

    private void FixPoint(int idx, Vector3 startPos, Transform fixer)
    {
        isFixed[idx] = NodeType.Fixed;
        StartPos[idx] = startPos;
        Fixers[idx] = fixer;
    }

    private void WeightPoint(int idx, Transform fixer)
    {
        isFixed[idx] = NodeType.Weight;
        Fixers[idx] = fixer;
        massArray[idx] += weightMass;
    }
    
    private void UpdateWind()
    {
        wind = new Vector3(Mathf.Cos(Time.time), 0.0f, Mathf.Sin(Time.time));
    }

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider.sharedMesh = mesh;

        UpdateMesh();
    }

    private void FixedUpdate()
    {
        if (!Paused)
        {
            // Sub-Steps
            for (int i = 0; i < subSteps; i++)
            {
                stepSymplectic();
            }
        }
    }

    private void Update()
    {
        //UpdateMesh();
    }

    #endregion

    public GameObject pole;
    private void stepSymplectic()
    {
        // Pasar de Locales a Globales
        for (int i = 0; i < positions.Length; i++)
        {
            if (isFixed[i] == NodeType.Fixed)
            {
                positions[i] = (Fixers[i].rotation * transform.TransformPoint(StartPos[i])) + Fixers[i].position;
            }
            else
            {
                positions[i] = transform.TransformPoint(vertices[i]);
            }
        }
        
        // Calculamos la Grabedad de los nodos
        Parallel.For(0, vertices.Length, it =>
        {
            forces[it] = Vector3.zero;
                                            // meto la masa en el damping
            forces[it] += (mass * Gravity) - (damping * mass * velocities[it]);
        });

        // Calcular la fuerza en los muelles    
        Parallel.For(0, springs.Count, it =>
        {
            var nodeA = springs[it].nodeA;
            var nodeB = springs[it].nodeB;
            
            var u = positions[nodeA] - positions[nodeB];
            u.Normalize();

            float stress = stiffList[it] * (lenghts[it]-lenghts0[it]) + stiffList[it] * damping * Vector3.Dot(u, velocities[nodeA] - velocities[nodeB]);
            var force = -stress * u;
            forces[nodeA] += force;
            forces[nodeB] -= force;
        });

        if (windSimulation)
        {
            var normals = mesh.normals;
            var t = 0;
            for (int i = 0; i < normals.Length; i++)
            {
                var velocity = velocities[triangles[t]] + velocities[triangles[t+1]] + velocities[triangles[t+3]];
                var stress = Mathf.Abs(Vector3.Dot(normals[i], wind - velocity)) * windDamping;
                forces[triangles[t]] += mass * wind * stress * windSpeed;
                forces[triangles[t+1]] += mass * wind * stress * windSpeed;
                forces[triangles[t+3]] += mass * wind * stress * windSpeed;
                t += 3;
            }
        }
        

        Parallel.For(0, vertices.Length, it =>
        {
            if (isFixed[it] != NodeType.Fixed)
            {
                velocities[it] += TimeStep / massArray[it] * forces[it];
                positions[it] += TimeStep * velocities[it];
            }
        });

        for (int i = 0; i < vertices.Length; i++)
        {
            if (isFixed[i] == NodeType.Weight)
            {
                Fixers[i].position = positions[i];
            }
            vertices[i] = transform.InverseTransformPoint(positions[i]);
            
        }

        Parallel.For(0, springs.Count, it =>
        {
            var v = positions[springs[it].nodeA] -  positions[springs[it].nodeB];
            lenghts[it] = v.magnitude;
        });

    }
}
