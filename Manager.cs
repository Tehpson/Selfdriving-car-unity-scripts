using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject CarPrefab;
    public int TraningTimeSec = 15;
    public int Laps = 3;
    public Vector3 StartingPos = new Vector3(0, 0, 0);
    public Vector3 Rotation = new Vector3(0, 0, 0);
    public int populationSize = 20;
    public List<Collider> Cheackpoint = new List<Collider>();

    private bool isTimerStarted = false;
    private bool isTraning = false;
    public int generationNumber = 0;
    private readonly int[] layers = new int[] { 10, 8, 8, 8, 3 };
    private List<NeuralNetwork> nets;
    private List<CarController> cars = null;
    void Timer()
    {
        isTraning = false;
        isTimerStarted = false;
    }

    public  void StartTimer()
    {
        if (!isTimerStarted)
        {
            isTimerStarted = true;
            Invoke("Timer", (float)TraningTimeSec);
        }
    }

    // Update is called once per frame
     void Update()
    {
        Console.Write("I am rubning");
        if (isTraning == false)
        {
            if (generationNumber == 0)
            {
                InitCarNetWork();
            }
            else
            {
                nets = nets.OrderByDescending(x => x.fitness).ToList();

                ManageJson.SaveNetworkToJson(nets);
                //mutets half of the the population
                for (int i = 0; i < populationSize / 10; i++)
                {

                    nets[i] = new NeuralNetwork(nets[i]);

                    for (int j = 10; j < populationSize; j += 10)
                    {
                        nets[j + i] = new NeuralNetwork(nets[i]);
                        nets[j + i].Mutate();
                    }
                }

                for (int i = 0; i < populationSize; i++)
                {
                    //Reset Fitnet
                    nets[i].SetFitness(0f);
                }
            }

            generationNumber++;
            //Starts traning and stop traning after set time
            isTraning = true;
            if(Cheackpoint.Count == 0)
            {
                StartTimer();
            }
            else if(Laps == 0)
            {
                StartTimer();
            }

            CreateCars();
        }
    }

    private void CreateCars()
    {
        //If There do exist cars then Destory them
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);
            }
        }
        cars = new List<CarController>();
        //create set ampunt of cars
        for (int i = 0; i < populationSize; i++)
        {
            //Spawn car;
            GameObject carobj = (GameObject)Instantiate(CarPrefab, StartingPos, CarPrefab.transform.rotation, transform);
            carobj.transform.Rotate(Rotation);
            CarController car = carobj.GetComponent<CarController>();
            car.Init(nets[i], StartingPos, Cheackpoint, this);
            cars.Add(car);
        }
    }

    private void InitCarNetWork()
    {
        var savedNetWork = ManageJson.GetSavedNetwork();
        if (savedNetWork.Count == 0)
        {
            //Creates new Lsit of nuweal networks
            nets = new List<NeuralNetwork>();
            for (int i = 0; i < populationSize; i++)
            {
                //create a nural network with Layers as above
                NeuralNetwork net = new NeuralNetwork(layers);
                //Mutases all Values so thay aint all the same
                net.Mutate();
                //add network to the networs list
                nets.Add(net);
            }
        }
        else
        {
            nets = savedNetWork;
        }
    }
}
