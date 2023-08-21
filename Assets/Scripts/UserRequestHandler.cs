using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Data.SqlClient;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class UserInfoHandler : NetworkBehaviour
{
    private string connectionString = SqlServerInfo.GetConnectionString();

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<RequestUserDataMessage>(OnReceiveUserDataRequest);
        NetworkServer.RegisterHandler<RequestUserSignUpMessage>(OnReceiveUserSignUpRequest);
    }

    private void OnReceiveUserDataRequest(NetworkConnection conn, RequestUserDataMessage msg)
    {
        ResponseUserDataMessage ResponseUserData = GetUserDataFromSQL(msg.userId);
        conn.Send(ResponseUserData);
        Debug.Log("사용자 UID: " + msg.userId + " 유저의 정보 요청 받음");
    }

    private ResponseUserDataMessage GetUserDataFromSQL(string userId)
    {
        ResponseUserDataMessage data = new ResponseUserDataMessage();

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

    private void OnReceiveUserSignUpRequest(NetworkConnection conn, RequestUserSignUpMessage msg)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "INSERT INTO [User] (id, name) VALUES (@Id, @Name)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", msg.userId);
                command.Parameters.AddWithValue("@Name", msg.nickname);
                command.ExecuteNonQuery();
                Debug.Log("사용자 UID: " + msg.userId + ", 사용자 이름: " + msg.nickname + " 가입");
            }
        }

        conn.Send(new ResponseUserSignUpMessage { success = true });
    }
}
