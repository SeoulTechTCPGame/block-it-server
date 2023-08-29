using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class SqlServerInfo : MonoBehaviour
{
    // 연결 정보 가져오기
    public static string GetConnectionString()
    {
        string jsonFilepath = @"Assets/ConnectionInfo.json";
        string strConn = string.Empty;
        try
        {
            string jsonText = System.IO.File.ReadAllText(jsonFilepath);
            JObject jsonData = JObject.Parse(jsonText);

            string dbAddress = (string)jsonData["DB"]["Address"];
            string dbPortNumber = (string)jsonData["DB"]["Port"];
            string dbName = (string)jsonData["DB"]["DBName"];
            string dbId = (string)jsonData["DB"]["ID"];
            string dbPassword = (string)jsonData["DB"]["Password"];

            strConn = "Server=" + dbAddress + "," + dbPortNumber + ";Database=" + dbName + ";Uid=" + dbId + ";Pwd=" + dbPassword + ";";
            Debug.Log(strConn);

            return strConn;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return strConn;
        }
    }
}
