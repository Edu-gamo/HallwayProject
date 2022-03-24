using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MyHallwayAgent : Agent {

    public GameObject ground;

    public GameObject area;

    public GameObject symbolOGoal;
    public GameObject symbolXGoal;

    public GameObject symbolOGreen;
    public GameObject symbolXGreen;
    public GameObject symbolORed;
    public GameObject symbolXRed;

    public GameObject targetGreen;
    public GameObject targetRed;

    public bool useVectorObs;

    Rigidbody m_AgentRb;

    Material m_GroundMaterial;
    Renderer m_GroundRenderer;

    HallwaySettings m_HallwaySettings;

    int m_Selection;

    StatsRecorder m_statsRecorder;
    
    private bool withTargetBall = false;


    public override void Initialize() {
        m_HallwaySettings = FindObjectOfType<HallwaySettings>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_GroundRenderer = ground.GetComponent<Renderer>();
        m_GroundMaterial = m_GroundRenderer.material;
        m_statsRecorder = Academy.Instance.StatsRecorder;
    }
    
    public override void CollectObservations(VectorSensor sensor) {
        if (useVectorObs) {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
    }
    
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time) {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = m_GroundMaterial;
    }
    
    public void MoveAgent(ActionSegment<int> act) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var action = act[0];
        
        switch (action) {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        m_AgentRb.AddForce(dirToGo * m_HallwaySettings.agentRunSpeed, ForceMode.VelocityChange);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }
    
    void OnCollisionEnter(Collision col) {
        
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal")) {
            
            if ((((m_Selection == 0 || m_Selection == 2) && col.gameObject.CompareTag("symbol_O_Goal"))
                || ((m_Selection == 1 || m_Selection == 3) && col.gameObject.CompareTag("symbol_X_Goal")))
                && withTargetBall) {
                
                SetReward(1f);
                StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
                m_statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
            
            } else {
                
                SetReward(-0.1f);
                StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.failMaterial, 0.5f));
                m_statsRecorder.Add("Goal/Wrong", 1, StatAggregationMethod.Sum);
            
            }
            
            EndEpisode();
        
        } else if (col.gameObject.CompareTag("Target_Green") || col.gameObject.CompareTag("Target_Red")) {
            
            if (((m_Selection == 0 || m_Selection == 1) && col.gameObject.CompareTag("Target_Green"))
                || ((m_Selection == 2 || m_Selection == 3) && col.gameObject.CompareTag("Target_Red"))) {
                
                SetReward(0.25f);
                StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
                m_statsRecorder.Add("Target/Correct", 1, StatAggregationMethod.Sum);
                
                withTargetBall = true;
                
                targetGreen.gameObject.transform.position = new Vector3(0f, -1000f, 0f) + targetGreen.gameObject.transform.position;
                targetRed.gameObject.transform.position = new Vector3(0f, -1000f, 0f) + targetRed.gameObject.transform.position;
            
            } else {
                
                SetReward(-0.1f);
                StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.failMaterial, 0.5f));
                m_statsRecorder.Add("Target/Wrong", 1, StatAggregationMethod.Sum);
                EndEpisode();
            
            }
        
        }
    
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        if (Input.GetKey(KeyCode.D)) {
            discreteActionsOut[0] = 3;
        } else if (Input.GetKey(KeyCode.W)) {
            discreteActionsOut[0] = 1;
        } else if (Input.GetKey(KeyCode.A)) {
            discreteActionsOut[0] = 4;
        } else if (Input.GetKey(KeyCode.S)) {
            discreteActionsOut[0] = 2;
        }
    
    }
    
    public override void OnEpisodeBegin() {
        var agentOffset = -12f;
        m_Selection = Random.Range(0, 4);
        
        if (m_Selection == 0) {
            
            symbolOGreen.transform.position = new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-5f, 5f)) + ground.transform.position;
            symbolXGreen.transform.position = new Vector3(0f, -1000f, 0f) + ground.transform.position;
            symbolORed.transform.position = new Vector3(0f, -1000f, 5f) + ground.transform.position;
            symbolXRed.transform.position = new Vector3(0f, -1000f, -5f) + ground.transform.position;
        
        } else if (m_Selection == 1) {
            
            symbolOGreen.transform.position = new Vector3(0f, -1000f, 0f) + ground.transform.position;
            symbolXGreen.transform.position = new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-5f, 5f)) + ground.transform.position;
            symbolORed.transform.position = new Vector3(0f, -1000f, 5f) + ground.transform.position;
            symbolXRed.transform.position = new Vector3(0f, -1000f, -5f) + ground.transform.position;
        
        } else if (m_Selection == 2) {
            
            symbolOGreen.transform.position = new Vector3(0f, -1000f, 0f) + ground.transform.position;
            symbolXGreen.transform.position = new Vector3(0f, -1000f, 5f) + ground.transform.position;
            symbolORed.transform.position = new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-5f, 5f)) + ground.transform.position;
            symbolXRed.transform.position = new Vector3(0f, -1000f, -5f) + ground.transform.position;
        
        } else {
            
            symbolOGreen.transform.position = new Vector3(0f, -1000f, 0f) + ground.transform.position;
            symbolXGreen.transform.position = new Vector3(0f, -1000f, 5f) + ground.transform.position;
            symbolORed.transform.position = new Vector3(0f, -1000f, -5f) + ground.transform.position;
            symbolXRed.transform.position = new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-5f, 5f)) + ground.transform.position;
        
        }
        
        transform.position = new Vector3(Random.Range(-3f, 3f), 1f, agentOffset + Random.Range(-5f, 5f)) + ground.transform.position;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;
        
        var goalPos = Random.Range(0, 2);
        if (goalPos == 0) {
            
            symbolOGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
            symbolXGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
        
        } else {
            
            symbolXGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
            symbolOGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
        
        }
        
        var targetPos = Random.Range(0, 2);
        if (targetPos == 0) {
            
            targetGreen.transform.position = new Vector3(-5f, 1.5f, -20f) + area.transform.position;
            targetRed.transform.position = new Vector3(5f, 1.5f, -20f) + area.transform.position;
        
        } else {
            
            targetGreen.transform.position = new Vector3(5f, 1.5f, -20f) + area.transform.position;
            targetRed.transform.position = new Vector3(-5f, 1.5f, -20f) + area.transform.position;
        
        }
        
        withTargetBall = false;
        
        m_statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Target/Correct", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Target/Wrong", 0, StatAggregationMethod.Sum);
    
    }

}