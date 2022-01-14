using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBestManager : MonoBehaviour
{
    public GameObject CarPrefab;
    public Vector3 StartingPos = new Vector3(0, 0, 0);
    public Vector3 Rotation = new Vector3(0, 0, 0);

    private NeuralNetwork net;

    // Start is called before the first frame update
    void Start()
    {
        net = ManageJson.GetSavedNetwork(1)[0]; // tar den bästa sparade nätverk
        GameObject carobj = (GameObject)Instantiate(CarPrefab, StartingPos, CarPrefab.transform.rotation, transform); //skapar ny bil
        carobj.transform.Rotate(Rotation);
        CarController car = carobj.GetComponent<CarController>();
        car.Init(net, StartingPos); //sätter nätverk till bilen
    }
}
