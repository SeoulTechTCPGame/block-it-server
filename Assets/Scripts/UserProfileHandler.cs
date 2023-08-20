using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Data.SqlClient;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class UserProfileHandler : NetworkBehaviour
{
    private string connectionString = SqlServerInfo.GetConnectionString();

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<RequestUserProfileMessage>(OnReceiveDataRequest);
    }

    private void OnReceiveDataRequest(NetworkConnection conn, RequestUserProfileMessage msg)
    {
        ResponseUserProfileMessage ResponseUserProfile = GetUserDataFromSQL(msg.userId);
        conn.Send(ResponseUserProfile);
        Debug.Log(msg.userId + "유저의 정보 요청 받음");
    }

    private ResponseUserProfileMessage GetUserDataFromSQL(string userId)
    {
        ResponseUserProfileMessage data = new ResponseUserProfileMessage();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = $"SELECT name, playcount, wincount, profilepic FROM [User] WHERE id = @userId";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        data.nickname = reader.GetString(0);
                        data.playCount = reader.GetInt32(1);
                        data.winCount = reader.GetInt32(2);
                        data.profilePicturePath = reader.GetString(3);
                    }
                }
            }
        }

        return data;
    }
}
