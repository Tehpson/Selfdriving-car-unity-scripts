using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    public bool isUserControled = false;

    public float idealRPM = 500f;
    public float maxRPM = 1000f;

    public Transform centerOfGravity;

    public WheelCollider WheelFL;
    public WheelCollider WheelFR;
    public WheelCollider WheelRL;
    public WheelCollider WheelRR;

    public float turnRadius = 30f;
    public float toruqe = 250f;
    public float breakTorque = 100f;

    public float AntiRoll = 20000.0f;

    public NeuralNetwork network;

    private new Rigidbody rigidbody;

    private Vector3 startPosition;


    [Header("Debud")]
    [Range(-1f, 1f)]
    public float verticalInput;

    [Range(-1f, 1f)]
    public float horizontalInput;

    public bool isBreaking;

    [Header("Fitness")]
    public float overalFittness;

    public float distanceMultipler = 1.4f;
    public float avgSpeedMultipler = 0.2f;
    public float sensorMultiplier = 0.1f;
    public float sensorRange = 0.4f;

    private List<Collider> Cheackpoints;
    public float[] CheackpointTime;
    private bool[] CheackpointPassed;
    private int lap = 0;

    private Manager Manager;
    private Manager_Grid Manager_grid;
    private ManagerCity Manager_City;

    private Vector3 lastPosition;

    private float totalDistanceTravelled;
    private float avgSpeed;

    private float timeSinceStart;
    private float time;

    public List<float> Sensors;


    private void InputSensors()
    {
        var F = transform.forward;
        var RE = -F;
        var R = transform.right;
        var L = -R;
        var FR = F + R; 
        var FL = F + L;
        var FFL = F + F + L;
        var FFR = F + F + R;
        var FRR = F + R + R;
        var FLL = F + L + L;
        /*
        var RER = RE + R; 
        var REL = RE + L;
        var REREL = RE + RE + L;
        var RERER = RE + RE + R;
        var RERR = RE + R + R;
        var RELL = RE + L + L;
        */
        RaycastHit hit;

        var directions = new List<Vector3>() { F, R, L, FR, FL, FFL, FFR, FRR, FLL/*, RER, REL, REREL, RERER, RERR, RELL, RE*/ };
        Sensors.Clear();

        var vecor = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        var r = new Ray(vecor, FR);
        LayerMask layerMask = 1 << 7;
        foreach (var direction in directions)
        {
            r.direction = direction;
            if (Physics.Raycast(r.origin, r.direction, out hit, 300f, 1))
            {
                Sensors.Add(hit.distance);
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }
        Sensors.Add(rigidbody.velocity.magnitude);
    }


    private void CalculateFittness()
    {
        //CalculatePerDistance();
    }


    private void OnTriggerEnter(Collider coll)
    {
        if (Cheackpoints != null && (Manager != null || Manager_grid != null))
        {
            CalculateFittensPetCheackpoint(coll);
        }
    }

    private void CalculateFittensPetCheackpoint(Collider coll)
    {
        for (int i = 0; i < Cheackpoints.Count; i++)
        {
            if (coll == Cheackpoints[i].GetComponent<Collider>() && CheackpointPassed[i] == false)
            {
                if (i == 0)
                {
                    CheackpointTime[i] = timeSinceStart;
                    CheackpointPassed[i] = true;
                    overalFittness = (1000 * (i + 1)) * ((lap+1) *10000)  - CheackpointTime[i];

                    network.SetFitness(overalFittness);
                }
                else if (CheackpointPassed[i - 1] == true)
                {
                    CheackpointTime[i] = timeSinceStart;
                    CheackpointPassed[i] = true;
                    overalFittness = (1000 * (i + 1)) * ((lap + 1) * 10000) - CheackpointTime[i];

                    network.SetFitness(overalFittness);
                    if(i == Cheackpoints.Count - 1)
                    {
                        for (int j = 0; j < CheackpointPassed.Length; j++)
                        {
                            CheackpointPassed[j] = false;
                        }
                        lap++;
                        if (lap ==(Manager != null ? Manager.Laps: Manager_grid.Laps))
                        {
                            if(Manager != null)
                            {
                                Manager.StartTimer();
                            }
                            else
                            {
                                Manager_grid.StartTimer();
                            }
                        }
                    }
                }
            }
        }
    }

    private void CalculatePerDistance()
    {
        totalDistanceTravelled += Vector2.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;
        overalFittness = ((totalDistanceTravelled - 20) * distanceMultipler) + (avgSpeed * avgSpeedMultipler);

        network.SetFitness(overalFittness);
    }

    private void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centerOfGravity.localPosition;
        CheackpointPassed = new bool[Cheackpoints.Count];
        CheackpointTime = new float[Cheackpoints.Count];
        for (int i = 0; i < CheackpointPassed.Length; i++)
        {
            CheackpointPassed[i] = false;
            CheackpointTime[i] = -1;
        }
    }

    private void FixedUpdate()
    {
        InputSensors();

        if (isUserControled)
        {
            GetInput();
        }
        else
        {
            //Create all Values into an array
            // send in value and get out 3 putput for what to do
            float[] output = network.FeedForward(Sensors.ToArray());

            verticalInput = output[0];
            horizontalInput = output[1];
            if(output[2] > 0)
            {
                isBreaking = true;
            }
            else
            {
                isBreaking = false;
            }
        }

        MoveCar();

        timeSinceStart += Time.deltaTime;
        time += Time.deltaTime;
        if (time > 2)
        {
            time = 0;
            lastPosition = transform.position;
        }

        CalculateFittness();
    }

    private void MoveCar()
    {
        var scaledTorque = verticalInput * toruqe;

        if (WheelFL.rpm < idealRPM)
        {
            scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, WheelFL.rpm / idealRPM);
        }
        else
        {
            scaledTorque = Mathf.Lerp(scaledTorque, 0, (WheelFL.rpm - idealRPM) / (maxRPM - idealRPM));

            DoRollBar(WheelFR, WheelFL);
        }
        WheelFL.steerAngle = horizontalInput * turnRadius;
        WheelFR.steerAngle = horizontalInput * turnRadius;

        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody.velocity.y > 0 || scaledTorque > 0)
        {
            WheelRL.motorTorque = scaledTorque;
            WheelRR.motorTorque = scaledTorque;
            WheelFL.motorTorque = scaledTorque;
            WheelFR.motorTorque = scaledTorque;
        }
        else
        {
            WheelRL.motorTorque = scaledTorque / 5;
            WheelRR.motorTorque = scaledTorque / 5;
            WheelFL.motorTorque = scaledTorque / 5;
            WheelFR.motorTorque = scaledTorque / 5;
        }
        if (isBreaking)
        {
            WheelRL.brakeTorque = breakTorque;
            WheelRR.brakeTorque = breakTorque;
        }
        else
        {
            WheelRL.brakeTorque = 0;
            WheelRR.brakeTorque = 0;
            WheelFL.brakeTorque = 0;
            WheelFR.brakeTorque = 0;
        }
    }

    private void DoRollBar(WheelCollider WheelL, WheelCollider WheelR)
    {
        var travlL = 1.0f;
        var travlR = 1.0f;

        var groundedL = WheelL.GetGroundHit(out WheelHit hit);
        if (groundedL)
            travlL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        var groundedR = WheelR.GetGroundHit(out hit);
        if (groundedR)
            travlR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
        var antiRollForce = (travlL - travlR) * AntiRoll;

        if (groundedL)
            rigidbody.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);
        if (groundedR)
            rigidbody.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position);
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);

        isBreaking = Input.GetKey(KeyCode.Space);
    }

    public void Init(NeuralNetwork net, Vector3 StartingPoss, List<Collider> Cheackpoints, Manager manager)
    {
        network = net;
        lastPosition = StartingPoss;
        this.Cheackpoints = Cheackpoints;
        this.Manager = manager;

    }    
    public void Init(NeuralNetwork net, Vector3 StartingPoss, List<Collider> Cheackpoints, Manager_Grid manager)
    {
        network = net;
        lastPosition = StartingPoss;
        this.Cheackpoints = Cheackpoints;
        this.Manager_grid = manager;

    }
    public void Init(NeuralNetwork net, Vector3 StartingPoss)
    {
        network = net;
        lastPosition = StartingPoss;

    }
    public void Init(NeuralNetwork net, ManagerCity manager)
    {
        network = net;
        this.Manager_City = manager;

    }
}