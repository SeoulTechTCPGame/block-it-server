using UnityEngine;
using Mirror;
using System.Data.SqlClient;
using System;

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
        NetworkServer.RegisterHandler<RequestProfileImageUploadMessage>(OnReceiveUserProfileImage);
        NetworkServer.RegisterHandler<RequestChangeUserNameMessage>(OnReceiveChangeUserNameRequest);
    }

    private void OnReceiveUserDataRequest(NetworkConnection conn, RequestUserDataMessage msg)
    {
        ResponseUserDataMessage ResponseUserData = GetUserDataFromSQL(msg.userId);
        conn.Send(ResponseUserData);
        Debug.Log("����� UID: " + msg.userId + " ������ ���� ��û ����");
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

                        if (!reader.IsDBNull(3))
                        {
                            data.profileImage = (byte[])reader["profilepic"];
                            Debug.Log("����� UID: " + userId + "�̹��� ���� ����");
                        }
                        else
                        {
                            data.profileImage = null;
                            Debug.Log("����� UID: " + userId + "�̹��� ����");
                        }
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
                Debug.Log("UID: " + msg.userId + ", ����� �̸�: " + msg.nickname + " ����");
            }
        }

        conn.Send(new ResponseUserSignUpMessage { success = true });
    }

    private void OnReceiveUserProfileImage(NetworkConnection conn, RequestProfileImageUploadMessage msg)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE [User] SET profilePic = @ImageData WHERE id = @userId";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ImageData", msg.image);
                command.Parameters.AddWithValue("@userId", msg.userId);
                command.ExecuteNonQuery();
                Debug.Log("UID: " + msg.userId + ", ������ �̹��� ����");
            }
        }

        conn.Send(new ResponseProfileImageUploadMessage { success = true });
    }

    private void OnReceiveChangeUserNameRequest(NetworkConnection conn, RequestChangeUserNameMessage msg)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(*) FROM [User] WHERE name = @newName";
            using (SqlCommand nameCheckCommand = new SqlCommand(query, connection))
            {
                nameCheckCommand.Parameters.AddWithValue("@newName", msg.nickname);
                int count = (int)nameCheckCommand.ExecuteScalar();
                if (count > 0)
                {
                    Debug.Log("UID: " + msg.userId + "�� ���� ��û�� �г���(" + msg.nickname + ") �ߺ�");
                    conn.Send(new ResponseChangeUserNameMessage { success = true, isDupcliate = true });
                    return;
                }
            }

            query = "UPDATE [User] SET name = @newName WHERE id = @userId";
            using (SqlCommand updateCommand = new SqlCommand(query, connection))
            {
                updateCommand.Parameters.AddWithValue("@userId", msg.userId);
                updateCommand.Parameters.AddWithValue("@newName", msg.nickname);
                updateCommand.ExecuteNonQuery();
                Debug.Log("UID: " + msg.userId + "�� �г����� " + msg.nickname + "�� ����");
            }
            conn.Send(new ResponseChangeUserNameMessage { success = true, isDupcliate = false });
        }
    }
}
