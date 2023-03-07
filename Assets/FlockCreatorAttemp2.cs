using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class FlockCreatorAttemp2 : MonoBehaviour
{

    public int numAgents = 100;
    public int spawnRadius = 20;
    public GameObject agent;

    public float speed = 1.0f;

    private Vector3[] sightRays;
    private GameObject[] agents;

    //GPU stuff
    private ComputeBuffer posBuffer;
    private ComputeBuffer velBuffer;
    private ComputeBuffer resultBuffer;

    private Vector3[] resultData;
    [SerializeField] ComputeShader computeShader;
    private int kernelHandle;
    //https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere/44164075#44164075

    // Start is called before the first frame update
    void Start()
    {
        agents = new GameObject[numAgents];
        agent.layer = LayerMask.NameToLayer("Agents");



        for (int i = 0; i < numAgents; i++)
        {
            Vector3 pos = UnityEngine.Random.insideUnitCircle;
            pos += this.gameObject.transform.position;
            pos.x += UnityEngine.Random.value * spawnRadius;
            pos.y += UnityEngine.Random.value * spawnRadius;
            pos.z += UnityEngine.Random.value * spawnRadius;
            //GameObject currentAgent = Instantiate(agent, this.transform.position, Quaternion.identity);
            GameObject currentAgent = Instantiate(agent, pos, Quaternion.identity);
            currentAgent.transform.parent = this.transform;
            agents[i] = currentAgent;
        }


        kernelHandle = computeShader.FindKernel("CSMain");



    }
    //Think of thses as the uniform variables in OpenGL.
    private void setUniforms()
    {
        float dt = Time.fixedDeltaTime;
        computeShader.SetFloat("dt", dt);
        computeShader.SetFloat("speed", speed);
    }


    //Send the Flock's positions and velocities to the GPU
    private void setBuffer()
    {
        Vector3[] posData = new Vector3[numAgents];
        Vector3[] velData = new Vector3[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            posData[i] = agents[i].transform.position;
            velData[i] = Vector3.zero;
        }

        posBuffer = new ComputeBuffer(numAgents, 12);
        posBuffer.SetData(posData);
        computeShader.SetBuffer(kernelHandle, "posBuffer", posBuffer);

        velBuffer = new ComputeBuffer(numAgents, 12);
        velBuffer.SetData(velData);
        computeShader.SetBuffer(kernelHandle, "velBuffer", velBuffer);

        resultBuffer = new ComputeBuffer(numAgents, 12);
        resultData = new Vector3[numAgents];
        computeShader.SetBuffer(kernelHandle, "resultBuffer", resultBuffer);
    }

    void runShader()
    {
        int threadGroups = Mathf.CeilToInt(numAgents / 64f);

        computeShader.SetBuffer(kernelHandle, "posBuffer", posBuffer);
        computeShader.SetBuffer(kernelHandle, "velBuffer", velBuffer);
        computeShader.SetBuffer(kernelHandle, "resultBuffer", resultBuffer);
        computeShader.SetInt("numAgents", numAgents);

        computeShader.Dispatch(kernelHandle, threadGroups, 1, 1);

        resultBuffer.GetData(resultData);

        for (int i = 0; i < numAgents; i++)
        {
            agents[i].transform.position = resultData[i];
        }
    }




    void FixedUpdate()
    {
        setUniforms();
        setBuffer();
        runShader();
    }

}