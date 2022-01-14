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
    private readonly int[] layers = new int[] { 10, 8, 8, 8, 3 }; // sätter hur många lager och hur många näder på alla lager
    private List<NeuralNetwork> nets;
    private List<CarController> cars = null;
    /// <summary>
    /// stänger av träningsläge
    /// </summary>
    void Timer()
    {
        isTraning = false;
        isTimerStarted = false;
    }

    /// <summary>
    /// Sätter timer för hur länge en genreation ska trännas
    /// </summary>
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
                for (int i = 0; i < populationSize / 10; i++) // tar dem 10 bästa
                {

                    nets[i] = new NeuralNetwork(nets[i]); // Deepcopy

                    for (int j = 10; j < populationSize; j += 10) // muterar 9 version av nätverket och lägger in i listan 
                    {
                        nets[j + i] = new NeuralNetwork(nets[i]);
                        nets[j + i].Mutate(); //mutate network
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
        //om de finns bilar tabort dem
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);
            }
        }
        cars = new List<CarController>();
        //skapa nya bilar
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
        //kollar ifall det redan finns sparat nätverk
        var savedNetWork = ManageJson.GetSavedNetwork();
        if (savedNetWork.Count == 0)
        {
            nets = new List<NeuralNetwork>();
            for (int i = 0; i < populationSize; i++)
            {
                //skapar ett helt nytt nuralt nätverk
                NeuralNetwork net = new NeuralNetwork(layers);
                //Mmuterar så att alla inte är samma
                net.Mutate();
                //lägger till allting i nätverks listan
                nets.Add(net);
            }
        }
        else
        {
            nets = savedNetWork;
        }
    }
}
