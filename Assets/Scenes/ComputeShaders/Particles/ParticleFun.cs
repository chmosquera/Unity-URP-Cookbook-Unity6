﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFun : MonoBehaviour
{

    private Vector2 cursorPos;

    // struct
    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
    }

    const int SIZE_PARTICLE = 7 * sizeof(float);

    public int particleCount = 1000000;
    public Material material;
    public ComputeShader shader;

    int kernelID;
    ComputeBuffer particleBuffer;

    int groupSizeX; 

    RenderParams rp;
    
    
    // Use this for initialization
    void Start()
    {
        Init();
    }

    void Init()
    {
        // initialize the particles
        Particle[] particleArray = new Particle[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            float x = Random.value * 2 - 1.0f;
            float y = Random.value * 2 - 1.0f;
            float z = Random.value * 2 - 1.0f;
            Vector3 xyz = new Vector3(x, y, z);
            xyz.Normalize();
            xyz *= Random.value * 5;

            particleArray[i].position.x = xyz.x;
            particleArray[i].position.y = xyz.y;
            particleArray[i].position.z = xyz.z;

            particleArray[i].velocity.x = 0;
            particleArray[i].velocity.y = 0;
            particleArray[i].velocity.z = 0;

            // Initial life value
            particleArray[i].life = Random.value * 5.0f + 1.0f;
        }

        // create compute buffer
        particleBuffer = new ComputeBuffer(particleCount, SIZE_PARTICLE);

        particleBuffer.SetData(particleArray);

        // find the id of the kernel
        kernelID = shader.FindKernel("CSParticle");

        uint threadsX;
        shader.GetKernelThreadGroupSizes(kernelID, out threadsX, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)particleCount / (float)threadsX);

        // bind the compute buffer to the shader and the compute shader
        shader.SetBuffer(kernelID, "particleBuffer", particleBuffer);
        material.SetBuffer("particleBuffer", particleBuffer);

        rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); 

    }

    void OnDestroy()
    {
        if (particleBuffer != null)
            particleBuffer.Release();
    }

    // Update is called once per frame
    void Update()
    {

        float[] mousePosition2D = { cursorPos.x, cursorPos.y };

        // Send data to the compute shader
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloats("mousePosition", mousePosition2D);

        // Update the Particles
        shader.Dispatch(kernelID, groupSizeX, 1, 1);

        Graphics.RenderPrimitives(rp, MeshTopology.Points, 1, particleCount );
    }

    void OnGUI()
    {
        Vector3 p = new Vector3();
        Camera c = Camera.main;
        Event e = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = e.mousePosition.x;
        mousePos.y = c.pixelHeight - e.mousePosition.y;

        p = c.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, c.nearClipPlane));

        cursorPos.x = p.x;
        cursorPos.y = p.y;    
    }
}
