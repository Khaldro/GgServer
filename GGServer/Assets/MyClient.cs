using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyClient {

    public bool authenticated = false;
    public string username { get; set; }
    public int hostId { get; set; }
    public int userId { get; set; }
    public int statsId { get; set; }
    public int connectionId { get; set; }
    public int sessionId { get; set; }
    public int ipAdress;
    public int port;

#region stats

    public int health { get; set; }
    public int damage { get; set; }
    public int defense { get; set; }

    #endregion stats

    public MyClient(int HostId, int ConnectionId, string Username, bool Authenticated)
    {
        this.hostId = HostId;
        this.connectionId = ConnectionId;
        this.username = Username;
        this.authenticated = Authenticated;
    }
}