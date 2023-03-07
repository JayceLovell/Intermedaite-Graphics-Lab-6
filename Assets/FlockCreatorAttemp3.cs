using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockCreatorAttemp3 : MonoBehaviour
{

    public int numAgents = 100;
    public int spawnRadius = 20;
    public GameObject agent;
    public float speed = 1.0f;

    private GameObject[] agents;
    private ComputeShader computeShader;
    private ComputeBuffer posBuffer;
    private ComputeBuffer velBuffer;
    private ComputeBuffer resultBuffer;
    private int kernelHandle;

    private const int THREADS_PER_GROUP = 64;

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

            GameObject currentAgent = Instantiate(agent, pos, Quaternion.identity);
            currentAgent.transform.parent = this.transform;
            agents[i] = currentAgent;
        }

        computeShader = (ComputeShader)Resources.Load("Attemp3Compute");
        kernelHandle = computeShader.FindKernel("CSMain");

        posBuffer = new ComputeBuffer(numAgents, 3 * sizeof(float));
        velBuffer = new ComputeBuffer(numAgents, 3 * sizeof(float));
        resultBuffer = new ComputeBuffer(numAgents, 3 * sizeof(float));

        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        Vector3[] initialPositions = new Vector3[numAgents];
        Vector3[] initialVelocities = new Vector3[numAgents];

        for (int i = 0; i < numAgents; i++)
        {
            initialPositions[i] = agents[i].transform.position;
            initialVelocities[i] = Random.insideUnitSphere * speed;
        }

        posBuffer.SetData(initialPositions);
        velBuffer.SetData(initialVelocities);

        computeShader.SetBuffer(kernelHandle, "PosBuffer", posBuffer);
        computeShader.SetBuffer(kernelHandle, "VelBuffer", velBuffer);
        computeShader.SetBuffer(kernelHandle, "ResultBuffer", resultBuffer);

        computeShader.SetFloat("Speed", speed);
        computeShader.SetInt("NumAgents", numAgents);
    }

    private void RunShader()
    {
        computeShader.Dispatch(kernelHandle, numAgents / THREADS_PER_GROUP, 1, 1);
    }

    void FixedUpdate()
    {
        RunShader();

        Vector3[] resultData = new Vector3[numAgents];
        resultBuffer.GetData(resultData);

        for (int i = 0; i < numAgents; i++)
        {
            agents[i].transform.position = resultData[i];
        }
    }

    private void OnDestroy()
    {
        posBuffer.Release();
        velBuffer.Release();
        resultBuffer.Release();
    }
}
