using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;
using System.Data;
using MySql.Data;
using System;

public class SqlTest : MonoBehaviour
{

    public static MySqlConnection conn;
    static MySqlCommand cmd;

    public static string username = string.Empty;
    public static string password = string.Empty;
    public static string connStr = "server=localhost;user=root;database=grinding_grounds;port=3306;password=sadou4596";

    public static void AuthenticationQuery( string Username, string Password)
    {
        try
        {
            conn = new MySqlConnection(connStr);
            conn.Open();
            string query = "SELECT acc_password FROM users where acc_name = @Username and acc_password = @Password;";
            cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", Username);
            cmd.Parameters.AddWithValue("@Password", Password);

            if (cmd.ExecuteScalar() == null || cmd.ExecuteScalar().ToString() != Password)
                password = "0";
            else
            {
                password = cmd.ExecuteScalar().ToString();
                username = Username;
            }

            conn.Close();

        }
        catch (Exception ex){
            Debug.Log(ex.ToString());
        }
    }

    public static void UpdateStatsQuery(string username, int hpValue, int dmgValue, int defValue, int lvlValue, int expValue)
    {
        try
        {
            conn = new MySqlConnection(connStr);
            conn.Open();
            string query = "UPDATE stats " +
                           "SET hp = @hpValue, dmg = @dmgValue, def = @defValue, lvl = @lvlValue, exp = @expValue " +
                           "WHERE p_name = + @username";


            cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hpValue", hpValue);
            cmd.Parameters.AddWithValue("@dmgValue", dmgValue);
            cmd.Parameters.AddWithValue("@defValue", defValue);
            cmd.Parameters.AddWithValue("@lvlValue", lvlValue);
            cmd.Parameters.AddWithValue("@expValue", expValue);

            cmd.ExecuteScalar();
            conn.Close();

        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    internal static ArrayList GetPlayersToFight(string name, int lvl)
    {
        MySqlDataReader reader;
        ArrayList PlayersToFightList = new ArrayList();
        
        try
        {
            conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "select p_name, lvl from stats " +
                       "where lvl between @minLvl and @maxLvl and p_name not in (@name) " +
                       "ORDER BY RAND() " +
                       "LIMIT 3 ;";

#region // TODOOO query for different players each time based on lvl (and rank?)
            l:
            System.Random r1 = new System.Random();
            System.Random r2 = new System.Random();
            System.Random r3 = new System.Random();
            int randLvl1 = 0;
            int randLvl2 = 0;
            int randLvl3 = 0;

            do
            {
                randLvl1 = r1.Next(lvl - 5, lvl + 5);
                randLvl2 = r2.Next(lvl - 5, lvl + 5);
                randLvl3 = r3.Next(lvl - 5, lvl + 5);

            } while (randLvl1 <= 0 || randLvl2 <= 0 || randLvl3 <= 0);

            //Debug.Log(randLvl1);
            //Debug.Log(randLvl2);
            //Debug.Log(randLvl3);

            #endregion

            cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@r1", randLvl1);
            cmd.Parameters.AddWithValue("@r2", randLvl2);
            cmd.Parameters.AddWithValue("@r3", randLvl3);
            cmd.Parameters.AddWithValue("@minLvl", lvl);
            cmd.Parameters.AddWithValue("@maxLvl", lvl + 5);
            reader = cmd.ExecuteReader();
            if (reader == null)
                goto l;

            while(reader.Read())
            {
                
                PlayersToFightList.Add(reader["p_name"]);
                PlayersToFightList.Add(reader["lvl"]);
            }

            //for (int i = 0; i < PlayersToFightList.Count; i++)
            //{
            //    Debug.Log(PlayersToFightList[i]);
            //}
            conn.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return PlayersToFightList; 
    }

    internal static ArrayList GetPlayerToFight(string name)
    {
        MySqlDataReader reader;
        ArrayList PlayerToFight = new ArrayList();

        try
        {
            conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "select p_name,hp,dmg,def,lvl from stats " +
                       "where p_name = @name;";

            cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);

            reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                PlayerToFight.Add(reader["p_name"]);
                PlayerToFight.Add(reader["hp"]);
                PlayerToFight.Add(reader["dmg"]);
                PlayerToFight.Add(reader["def"]);
                PlayerToFight.Add(reader["lvl"]);
            }
            conn.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return PlayerToFight;
    }
}