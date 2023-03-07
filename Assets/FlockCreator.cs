using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UIElements;

public class FlockCreator : MonoBehaviour
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



        for (int i = 0; i < numAgents; i ++){
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
    private void setUniforms(){
        computeShader.SetInt("numAgents", numAgents);
    }

    //Send the Flock's positions and velocities to the GPU
    private void setBuffer(){
        Vector3[] posData = new Vector3[numAgents];
        Vector3[] velData = new Vector3[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            posData[i] = agents[i].transform.position;
            velData[i] = agents[i].transform.forward * speed;
        }

        posBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
        velBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
        resultBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);

        posBuffer.SetData(posData);
        velBuffer.SetData(velData);

        computeShader.SetBuffer(kernelHandle, "posBuffer", posBuffer);
        computeShader.SetBuffer(kernelHandle, "velBuffer", velBuffer);
        computeShader.SetBuffer(kernelHandle, "resultBuffer", resultBuffer);
        computeShader.SetBuffer(kernelHandle, "Result", resultBuffer);
    }
    void runShader()
    {

        computeShader.Dispatch(kernelHandle, Mathf.CeilToInt(numAgents / 8.0f), 1, 1);

        // Set the buffer containing the results of the computation
        computeShader.SetBuffer(kernelHandle, "Result", resultBuffer);

        resultData = new Vector3[numAgents];
        resultBuffer.GetData(resultData);

        for (int i = 0; i < numAgents; i++)
        {
            // Add a small offset to the target position to avoid "look rotation viewing vector is zero" error
            Vector3 targetPos = resultData[i] + Vector3.one * 0.001f;

            agents[i].transform.position = resultData[i];
            agents[i].transform.forward = Vector3.Normalize(targetPos - agents[i].transform.position);
        }
    }




    void FixedUpdate(){      

        // Dispatch compute shader
        setUniforms();
        setBuffer();
        runShader();
    }

    private void OnDestroy()
    {
        posBuffer.Release();
        velBuffer.Release();
        resultBuffer.Release();
    }
}