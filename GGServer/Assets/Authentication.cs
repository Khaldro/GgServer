using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class Authentication {

    SqlTest mySqlTest;

	public string CheckUsernameAndPassword(int recHostId, int connectionId, string[] msg)
    {
        //mySqlTest = GameObject.Find("MySQL").GetComponent<SqlTest>();

        string stringToReturn = string.Empty;
        string username = msg[1];
        string password = msg[2];
        bool usernameOk;
        bool passwordOk;

        SqlTest.AuthenticationQuery(username, password);
        

        if (username == SqlTest.username)
            usernameOk = true;
        else usernameOk = false;

        if (password == SqlTest.password)
            passwordOk = true;
        else passwordOk = false;
        

        if (usernameOk & password == SqlTest.password)
            stringToReturn = "userAndPassOk";
        else if (!usernameOk || !passwordOk)
            stringToReturn = "userOrPassFalse";

        return stringToReturn;
    }
}
