using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
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
        


        for(int i = 0; i < numAgents; i ++){
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

    }

    //Send the Flock's positions and velocities to the GPU
    private void setBuffer(){

    } 
    void runShader(){

        
    }



    void FixedUpdate(){
        for(int i = 0; i < numAgents; i ++){
            float closetDist = Mathf.Infinity;
            Vector3 directionVector = new Vector3(0.0f, 0.0f, 0.0f);
            GameObject currentAgent = agents[i];
            Vector3 currentAgentPos = currentAgent.transform.position;
            for(int j = 0; j < numAgents; j ++){
                GameObject tempAgent = agents[j];
                Vector3 tempAgentPos = tempAgent.transform.position;
                float dist = Vector3.Distance(currentAgentPos, tempAgentPos);
                if(i != j){
                    if(dist < closetDist){
                        closetDist = dist;
                        directionVector = currentAgentPos - tempAgentPos;
                        directionVector = Vector3.Normalize(directionVector);
                        directionVector *= -1.0f;
                    }
                }
            }
            currentAgent.transform.position += directionVector * speed;
        }
    }
}