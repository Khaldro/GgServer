using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Linq;
using System;


public class MyServer : NetworkManager
{

    private const int MAX_CONNECTIONS = 100;
    private int port = 8080;
    private int serverHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;
 
    Authentication auth;
    List<MyClient> Clients;
    SqlTest mySqlTest;
    
    //int sessionId = 0;

    void Start()
    {
        
        Clients = new List<MyClient>();
        
       

        Application.runInBackground = true;
        // Init
        NetworkTransport.Init();
        
        // Config
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unreliableChannel = config.AddChannel(QosType.Unreliable);

        // Topology
        HostTopology topology = new HostTopology(config, MAX_CONNECTIONS);

        // Host (socket)
        serverHostId = NetworkTransport.AddHost(topology, port);

        isStarted = true;
        Debug.Log(networkAddress.ToString());
        
        
    }

    private void ConnectToDatabase()
    {
        SqlTest.conn = new MySqlConnection(SqlTest.connStr);
        SqlTest.conn.Open();
    }
    
   
    private void Update()
    {
        if (!isStarted)
            return;

        int clientHostId;
        int connectionId;
        int channelId;
        byte[] receivedBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType receivedData = NetworkTransport.Receive(out clientHostId, out connectionId,
            out channelId, receivedBuffer, bufferSize, out dataSize, out error);

        switch (receivedData)
        {
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log(string.Format("Host: {0} | ConnectionId: {1}", clientHostId, connectionId));
                break;
            case NetworkEventType.DataEvent:       //3
                ReceiveData(clientHostId, connectionId, receivedBuffer, dataSize);
                break;
            case NetworkEventType.DisconnectEvent: //4
                break;
        }
    }




    private void ReceiveData(int clientHostId, int connectionId, byte[] buffer, int bufferLength)
    {
        byte[] receivedBuffer = buffer;

        string str = Encoding.Unicode.GetString(receivedBuffer, 0, bufferLength);
        str = str.Trim();
        string[] msg = str.Split('|');
        
        Routing(clientHostId, connectionId, msg);
        
    }



    public void SendData(int clientHostId, int connectionId, string message)
    {
        byte[] sendBuffer = new byte[1024];
        string str = message;
        sendBuffer = Encoding.Unicode.GetBytes(str);
        
            
        
            NetworkTransport.Send(clientHostId, connectionId, reliableChannel, sendBuffer, str.Length * sizeof(char), out error);
            if(error !=0)
                Debug.Log("Data sent: "+error);
       
    }

    public void MulticastData(string message)
    {
        byte[] sendBuffer = new byte[1024];
        string str = message;
        sendBuffer = Encoding.Unicode.GetBytes(str);



        NetworkTransport.StartSendMulticast(serverHostId, reliableChannel, sendBuffer, str.Length * sizeof(char), out error);
        if (error != 0)
            Debug.Log("Data sent: " + error);

    }

    public void Routing(int clientHostId, int connectionId, string[] msg)
    {
        switch (msg[0])
        {
            case "auth":
                if (!IsUserAuhenticated(clientHostId, connectionId, msg))
                {
                    Login(clientHostId, connectionId, msg);
                }
                else return;
                break;
            case "rps":
                UpdatePlayerStatsOnDB(msg);
                break;
            case "rptf":
                SendPlayersToFight(clientHostId, connectionId, msg);
                break;
            case "gptf":
                SendPlayerToFight(clientHostId, connectionId, msg);
                break;
            case "bp":
                Debug.Log(msg[1] +" | "+ msg[2]);
                break;
        }
    }

    private void SendPlayerToFight(int clientHostId, int connectionId, string[] msg)
    {
        ArrayList temp = SqlTest.GetPlayerToFight(msg[1]);
        string message = string.Empty;
        
        message = string.Format("gptf|{0}|{1}|{2}|{3}|{4}|", temp[0],temp[1],temp[2],temp[3],temp[4]);
        SendData(clientHostId, connectionId, message);
    }

    private void SendPlayersToFight(int clientHostId, int connectionId, string[] msg)
    {
        ArrayList PlayersToFightList;
        string message = string.Empty;


        PlayersToFightList = SqlTest.GetPlayersToFight(msg[1], int.Parse(msg[2]));
        if (PlayersToFightList.Count == 2 || PlayersToFightList.Count == 3)
            message = "rptf|" + PlayersToFightList[0] + "|" + PlayersToFightList[1] + "|";
        else if (PlayersToFightList.Count == 4 || PlayersToFightList.Count == 5)
            message = "rptf|" + PlayersToFightList[0] + "|" + PlayersToFightList[1] + "|" + PlayersToFightList[2] + "|" + PlayersToFightList[3] + "|";
        else if (PlayersToFightList.Count >= 6)
            message = "rptf|" + PlayersToFightList[0] + "|" + PlayersToFightList[1] + "|" + PlayersToFightList[2] + "|" + PlayersToFightList[3] + "|" + PlayersToFightList[4] + "|" + PlayersToFightList[5] + "|";
        else
        {
            Debug.Log("Error in PlayersToFight()"); message = "x";
        }
        
        SendData(clientHostId, connectionId, message);
    }

    private void UpdatePlayerStatsOnDB(string[] msg)
    {
        string name = msg[1];
        int hp = int.Parse(msg[2]);
        int dmg = int.Parse(msg[3]);
        int def = int.Parse(msg[4]);
        int lvl = int.Parse(msg[5]);
        int exp = int.Parse(msg[6]);



        SqlTest.UpdateStatsQuery(name, hp, dmg, def, lvl, exp);

    }

    bool IsUserAuhenticated(int clientHostId, int connectionId, string[] msg)
    {
        bool isAuthenticated=false;

        if (Clients.Count != 0)
        {
            try
            {
                string fetchedUsername = FetchUser(msg[1]).username;
                if (FetchUser(msg[1]) != null && fetchedUsername == msg[1])
                {
                    Debug.Log(fetchedUsername + " is already logged-in");
                    SendData(clientHostId, FetchUser(fetchedUsername).connectionId, "auth|loggedin|");
                    isAuthenticated = true;
                }
                else isAuthenticated = false;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        return isAuthenticated;
    }
    


    void Login(int clientHostId, int connectionId, string[] msg)
    {
        string authenticationCheck;
        //CreateAccount(hostId, connectionId, msg);
        MyClient tempClient;
            
            auth = new Authentication();
            authenticationCheck = auth.CheckUsernameAndPassword(clientHostId, connectionId, msg);

            switch (authenticationCheck)
            {
            case "userAndPassOk":

                Clients.Add(tempClient = new MyClient(clientHostId, connectionId, msg[1], true));
                SendData(clientHostId, tempClient.connectionId, "auth|success|"+tempClient.username+"|");

                break;
            case "userOrPassFalse":
                SendData(clientHostId, connectionId, "auth|userOrpassFalse|");
                break;
               
            case "CannotProcessRequest":

                break;
            }
        Debug.Log(Clients.Count + " : users connected");
    }

    // TODO ?
    private int GenerateSessionId()
    {
        int sessionId = 0;
        int min = 1346879521;
        int max = min * 2;
        System.Random randomNumber = new System.Random();
        
        sessionId = randomNumber.Next(min, max);
        
        return sessionId;
    }

    MyClient FetchUser(string username)
    {
        var user = Clients.Find(u => u.username == username);
        return user;
    }
    MyClient FetchUser(int index)
    {
        var user = Clients[index];
        return user;
    }
   
    private void OnApplicationQuit()
    {
        NetworkTransport.Shutdown();
    }
}




