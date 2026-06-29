using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public struct GameState {
    int connectedPlayers;
    Vector3 position1;
    Vector3 position2;
    Vector3 position3;
    Vector3 position4;
}

public class NetworkManager : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;
    BinaryReader reader;
    BinaryWriter writer;
    UdpClient udpClient;
    public string serverIP;
    public int port = 12555;
    public int udpPort = 12555;
    public TextMeshProUGUI text;
    private int messageIndex = 0;
    string[] messageList = {"Howdydoo", "Hello there", "Third Time's the charm"};
    public Vector3 position = new Vector3(0,0,0);
    public Transform player_pos;
    int fu_count;
    bool first_location = true;
    private bool should_flip = false;
    private int fixed_input_count;
    public GameObject playerCharacter;
    [SerializeField]
    InputAction spacebar;
    [SerializeField]
    InputAction localTextUpdate;
    public TMP_Text lobbyName;
    public bool connectedToLobby;
    public string connectedLobby;
    public int is_host;
    public int host_number;
    public int client_number;
    public string[] lobbyList;

    // Start is called before the first frame update
    void Start()
    {
        localTextUpdate.Enable();
        spacebar.Enable();
    }
    public byte[] FormHostMessage(int host)
    {
        byte[] message = new byte[256];
        message[0] = 0x02;
        message[1] = (byte)host;
        udpClient.Send(message, message.Length);
        return message;
    }
    public byte[] FormClientMessage(int host, int client)
    {
        byte[] message = new byte[256];
        message[0] = 0x02;
        message[1] = (byte)host;
        message[2] = (byte)client;
        udpClient.Send(message, message.Length);
        return message;
    }
    public void ConnectToServerAsHost(){
        try{
            if(udpClient == null)
            {
                udpClient = new UdpClient();
                udpClient.Connect(serverIP, udpPort);
            }
            Debug.Log("connected to server");
            UDPInitialConnect(lobbyName.text);
            
        } catch (Exception e) {
            Debug.LogError($"Could not connect to server: {e.Message}");
        }
    }
    public void GetLobbyList(){
        try{
            if(udpClient == null)
            {
                udpClient = new UdpClient();
                udpClient.Connect(serverIP, udpPort);
            }
            Debug.Log("connected to server");
            LobbyRequestMessage();
            
        } catch (Exception e) {
            Debug.LogError($"Could not connect to server: {e.Message}");
        }
    }
    public void ConnectToLobby(int lobby_id)
    {
        byte[] message = new byte[3] {1, 0, (byte)lobby_id};
        udpClient.Send(message, message.Length);
    }

    string ShowBytesAsString(byte[] bytes){
        string message = "";
        for (int i = 0; i < bytes.Length; i++){
            if (i != bytes.Length - 1){
                message += $"{bytes[i]:X2} ";
            } else {
                message += $"{bytes[i]:X2}";
            }
        }

        return message;

    }
    private void SendString(string message)
    {
        if (stream != null && client.Connected){
            int message_size = message.Length;
            byte[] message_size_in_bytes = BitConverter.GetBytes(message_size);
            byte[] bytes_message = Encoding.UTF8.GetBytes(message);
            byte[] prepended_bytes_message = new byte[bytes_message.Length + 5];
            prepended_bytes_message[0] = 0x01;
            Buffer.BlockCopy(message_size_in_bytes, 0, prepended_bytes_message, 1, message_size_in_bytes.Length);
            Buffer.BlockCopy(bytes_message, 0, prepended_bytes_message, 5, bytes_message.Length);
            Debug.Log($"Sending {prepended_bytes_message.Length}");
            writer.Write(prepended_bytes_message);
            writer.Flush();
            Debug.Log("Sent: " + message + " as " + ShowBytesAsString(prepended_bytes_message));
        }
        else {
            Debug.LogError("Socket connection is not established");
        }
    }
    private void UDPInitialConnect(string _lobbyName)
    {
        _lobbyName = _lobbyName.Trim(new char[] { ' ', '​', '‌', '‍', '﻿' });
        int hostBit = 1;
        byte[] lobby_name_in_bytes = Encoding.UTF8.GetBytes(_lobbyName);
        byte[] bytes_message = new byte[2 + lobby_name_in_bytes.Length + 1];
        bytes_message[0] = 0x01;
        bytes_message[1] = BitConverter.GetBytes(hostBit)[0];
        Buffer.BlockCopy(lobby_name_in_bytes, 0, bytes_message, 2, lobby_name_in_bytes.Length);
        bytes_message[bytes_message.Length-1] = 0x00;
        udpClient.Send(bytes_message, bytes_message.Length);
        Debug.Log("UDP Sent: " + _lobbyName + " as " + ShowBytesAsString(bytes_message));
    }
    private void LobbyRequestMessage()
    {
        byte[] bytes_message = new byte[] { 1, 2};
        udpClient.Send(bytes_message, bytes_message.Length);
    }
    private void SendUdpString(string message)
    {
        if (udpClient != null) {
            int message_size = message.Length;
            byte[] message_size_in_bytes = BitConverter.GetBytes(message_size);
            byte[] bytes_message = Encoding.UTF8.GetBytes(message);
            byte[] prepended_bytes_message = new byte[bytes_message.Length + 1];
            prepended_bytes_message[0] = 0x02; //2 is the signifer for a string incoming
            Buffer.BlockCopy(bytes_message, 0, prepended_bytes_message, 1, bytes_message.Length);
            udpClient.Send(prepended_bytes_message, prepended_bytes_message.Length);
            Debug.Log("UDP Sent: " + message + " as " + ShowBytesAsString(prepended_bytes_message));
        }
        else {
            Debug.LogError("UDP socket is not established");
        }
    }
    private void RequestSpawnCharacter(byte[] context_for_spawn){
        if (stream != null && client.Connected){
            byte[] buffer = new byte[6];
            // Use 0x02 as a signifier code the server will interpret as a request to spawn a character.
            Buffer.BlockCopy(new byte[] { 0x02 }, 0, buffer, 0, 1);
            // Provide additional context in the main block of data using the byte 0x01 to indicate this is an initial spawn
            byte[] length_in_bytes = BitConverter.GetBytes(context_for_spawn.Length);
            Buffer.BlockCopy(length_in_bytes, 0, buffer, 1, length_in_bytes.Length);
            Buffer.BlockCopy(context_for_spawn, 0, buffer, 5, context_for_spawn.Length);
            byte[] printable_buffer = new byte[buffer.Length];
            Array.Copy(buffer, printable_buffer, buffer.Length);
            writer.Write(buffer);
            writer.Flush();
            Debug.Log("Sent: " + ShowBytesAsString(buffer));
            Debug.Log("Printable buffer: " + ShowBytesAsString(printable_buffer));
        }
        else {
            Debug.LogError("Socket connection is not established");
        }
    }
    private void RequestMoveCharacter(Vector3 pos)
    {
        if (stream != null && client.Connected){
            byte[] buffer = new byte[17];
            Buffer.BlockCopy(new byte[] { 0x03 }, 0, buffer, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(12), 0, buffer, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buffer, 5, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buffer, 9, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, buffer, 13, 4);
            byte[] printable_buffer = new byte[buffer.Length];
            Array.Copy(buffer, printable_buffer, buffer.Length);
            // if (BitConverter.IsLittleEndian){
            //     Array.Reverse(buffer, 5, 4);
            //     Array.Reverse(buffer, 9, 4);
            //     Array.Reverse(buffer, 13, 4);
            // }
            writer.Write(buffer);
            writer.Flush();
            Debug.Log("Sent: " + ShowBytesAsString(buffer));
            Debug.Log("Printable buffer: " + ShowBytesAsString(printable_buffer));
        }
        else {
            Debug.LogError("Socket connection is not established");
        }
    }

    private void OnApplicationQuit(){
        if (stream != null){
            writer.Close();
            reader.Close();
            stream.Close();
        }
        if (client != null){
            client.Close();
        }
        udpClient?.Close();
    }
    // Update is called once per frame
    void Update()
    {
        if(udpClient != null && udpClient.Available > 0)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.Receive(ref remoteEP);
            Debug.Log($"Bytes: {ShowBytesAsString(data)}");
            //Acknowledge Connection Message
            if(data[0] == 0)
            {
                //Is Server Acknowledge
                if(data[1] == 0)
                {
                    if(data.Length > 19)
                    {
                        is_host = 0;
                        host_number = data[2];
                        client_number = data[3];
                        connectedLobby = Encoding.UTF8.GetString(data, 4, data.Length - 4).TrimEnd('\0');
                    }
                    else
                    {
                        is_host = 1;
                        host_number = data[2];
                        connectedLobby = Encoding.UTF8.GetString(data, 3, data.Length - 3).TrimEnd('\0');
                    }
                    connectedToLobby = true;
                }


                //Is requesting Lobby List Acknowledge
                if(data[1] == 2)
                {
                    lobbyList = new string[4];
                    for(int i = 0; i < 4; i++)
                    {
                        int lobby_string_size = 16;
                        int offset = (i*16) + 2;
                        if(data[offset] != 0)
                            lobbyList[i] = Encoding.UTF8.GetString(data, offset, lobby_string_size);
                    }
                }
            }
            else if(data[0] == 2)
            {

                Debug.Log("0x02 message received: " + ShowBytesAsString(data));
            }
        }
    }
    void FixedUpdate()
    {
    }


}

