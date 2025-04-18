using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

public class PointCloudRenderer : MonoBehaviour
{
    [Header("Point Cloud Settings")]
    public int pointCount = 100000;
    public float maxDistance = 100f;
    public float minSize = 0.01f;
    public float spawnRadius = 50f;
    public Vector2 pointSizeRange = new Vector2(0.1f, 2.0f);
    
    [Header("Rendering")]
    public Mesh instancedMesh;
    public Material material;
    public Bounds renderBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
    
    [Header("Debug")]
    public bool debugMode = false;
    
    // Compute shader
    public ComputeShader cullShader;
    
    // Kernel IDs
    private int cullKernel;
    
    // Buffers
    private ComputeBuffer pointsBuffer;
    private ComputeBuffer visiblePointsBuffer;
    private GraphicsBuffer argsBuffer;
    
    // For indirect rendering
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    
    // Structure for point data
    [System.Serializable]
    public struct PointData
    {
        public Vector3 position;
        public float size;
        public Color color;
    }
    
    void Start()
    {
        InitializeBuffers();
        InitializeComputeShader();
    }
    
    void InitializeBuffers()
    {
        // Create and fill the points buffer with sample data
        pointsBuffer = new ComputeBuffer(pointCount, Marshal.SizeOf(typeof(PointData)));
        PointData[] points = new PointData[pointCount];
        
        // Generate random points for the demo
        for (int i = 0; i < pointCount; i++)
        {
            points[i] = new PointData
            {
                position = Random.insideUnitSphere * spawnRadius,
                size = Random.Range(pointSizeRange.x, pointSizeRange.y),
                color = new Color(Random.value, Random.value, Random.value, 1.0f)
            };
        }
        pointsBuffer.SetData(points);
        
        // Buffer for visible point indices
        visiblePointsBuffer = new ComputeBuffer(pointCount, sizeof(uint), ComputeBufferType.Append);
        
        // Indirect arguments buffer
        argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        GraphicsBuffer.IndirectDrawIndexedArgs[] argsData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        argsData[0].indexCountPerInstance = instancedMesh.GetIndexCount(0);
        argsData[0].instanceCount = 0; // Will be set by compute shader
        argsData[0].startIndex = instancedMesh.GetIndexStart(0);
        argsData[0].baseVertexIndex = instancedMesh.GetBaseVertex(0);
        argsData[0].startInstance = 0;
        argsBuffer.SetData(argsData);
    }
    
    void InitializeComputeShader()
    {
        // Get kernel IDs
        cullKernel = cullShader.FindKernel("CullPoints");
        
        // Set buffers
        cullShader.SetBuffer(cullKernel, "_AllPoints", pointsBuffer);
        cullShader.SetBuffer(cullKernel, "_VisiblePoints", visiblePointsBuffer);
        cullShader.SetBuffer(cullKernel, "_ArgsBuffer", argsBuffer);
    }
    
    void Update()
    {
        // Reset visible points buffer
        visiblePointsBuffer.SetCounterValue(0);
        
        // Reset args buffer count
        GraphicsBuffer.IndirectDrawIndexedArgs[] argsData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        argsData[0].indexCountPerInstance = instancedMesh.GetIndexCount(0);
        argsData[0].instanceCount = 0; // Reset instance count
        argsData[0].startIndex = instancedMesh.GetIndexStart(0);
        argsData[0].baseVertexIndex = instancedMesh.GetBaseVertex(0);
        argsData[0].startInstance = 0;
        argsBuffer.SetData(argsData);
        
        // Update culling parameters
        cullShader.SetFloat("_MaxDistance", maxDistance);
        cullShader.SetFloat("_MinSize", minSize);
        cullShader.SetVector("_CameraPosition", Camera.main.transform.position);
        cullShader.SetFloat("_CameraNearPlane", Camera.main.nearClipPlane);
        cullShader.SetFloat("_CameraFarPlane", Camera.main.farClipPlane);
        
        // Dispatch compute shader
        int threadGroups = Mathf.CeilToInt(pointCount / 64.0f);
        cullShader.Dispatch(cullKernel, threadGroups, 1, 1);
        
        // Set buffers for the material to use
        material.SetBuffer("_AllPoints", pointsBuffer);
        material.SetBuffer("_VisiblePoints", visiblePointsBuffer);
        
        // Render with indirect arguments
        var renderParams = new RenderParams(material)
        {
            worldBounds = renderBounds,
            shadowCastingMode = ShadowCastingMode.Off,
            receiveShadows = false
        };
        
        Graphics.RenderMeshIndirect(renderParams, instancedMesh, argsBuffer);
        
        if (debugMode)
        {
            // Create a temporary ComputeBuffer for getting the count
            ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);
            ComputeBuffer.CopyCount(visiblePointsBuffer, countBuffer, 0);
            
            // Read the count
            uint[] countData = new uint[1] { 0 };
            countBuffer.GetData(countData);
            uint visibleCount = countData[0];
            
            // Release the temporary buffer
            countBuffer.Release();
            
            Debug.Log($"Visible points: {visibleCount} of {pointCount}");
        }
    }
    
    void OnDestroy()
    {
        // Release buffers
        pointsBuffer?.Release();
        visiblePointsBuffer?.Release();
        argsBuffer?.Dispose();
    }
} 