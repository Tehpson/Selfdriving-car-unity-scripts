using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft;

public class ManageJson : MonoBehaviour
{
    public const string BrainFile = "Brain.json"; //sätter nament på sparfilen
    
    /// <summary>
    /// Sparat nätverk
    /// </summary>
    /// <param name="networks">nätverk att spara</param>
    public static void SaveNetworkToJson(List<NeuralNetwork> networks)
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "AIBrain");
        Directory.CreateDirectory(path);
        path = Path.Combine(path, BrainFile);
        var data = Newtonsoft.Json.JsonConvert.SerializeObject(networks);
        File.WriteAllText(path, data);
    }

    /// <summary>
    /// hämtar sparadt nätverk
    /// </summary>
    /// <returns></returns>
    public static List<NeuralNetwork> GetSavedNetwork()
    {
        try
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "AIBrain");
            path = Path.Combine(path, BrainFile);
            var data = File.ReadAllText(path);
            var network = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NeuralNetwork>>(data);
            return network;
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
            return new List<NeuralNetwork>();
        }
    }

    /// <summary>
    /// Get ett vist antal av nätverket
    /// </summary>
    /// <param name="count">hur många</param>
    /// <returns></returns>
    public static List<NeuralNetwork> GetSavedNetwork(int count)
    {
        try
        {
            List<NeuralNetwork> returnList = new List<NeuralNetwork>();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "AIBrain");
            path = Path.Combine(path, BrainFile);
            var data = File.ReadAllText(path);
            var network = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NeuralNetwork>>(data);
            if(network.Count < count || count < 1) // så att det finns så många nätverk som är bätt om
            {
                return null;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    returnList.Add(network[i]);
                }
                return network;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            return null;
        }
    }
}