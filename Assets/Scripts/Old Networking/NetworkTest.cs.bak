// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.IO;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using TMPro;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public struct GameState {
//     int connectedPlayers;
//     Vector3 position1;
//     Vector3 position2;
//     Vector3 position3;
//     Vector3 position4;
// }

// public class NetworkTest : MonoBehaviour
// {
//     TcpClient client;
//     NetworkStream stream;
//     BinaryReader reader;
//     BinaryWriter writer;
//     public string serverIP = "192.168.0.59";
//     public int port = 12555;
//     public TextMeshProUGUI text;
//     private int messageIndex = 0;
//     string[] messageList = {"Howdydoo", "Hello there", "Third Time's the charm"};
//     public Vector3 position = new Vector3(0,0,0);
//     public Transform player_pos;
//     int fu_count;
//     bool first_location = true;
//     private bool should_flip;
//     private int fixed_input_count;
//     public GameObject playerCharacter;
//     [SerializeField]
//     InputAction spacebar;
//     [SerializeField]
//     InputAction localTextUpdate;

//     IEnumerator RequestText() {
//         yield return new WaitForSeconds(2);
//         text.text = "hi";
//     }
//     // Start is called before the first frame update
//     void Start()
//     {
//         localTextUpdate.Enable();
//         spacebar.Enable();
//         ConnectToServer();
//     }
//     void ConnectToServer(){
//         try{
//             client = new TcpClient(serverIP,port);
//             stream = client.GetStream();
//             reader = new BinaryReader(stream);
//             writer = new BinaryWriter(stream);
//             Debug.Log("connected to server");
//             SendString("hi");
//             Debug.Log("Reading Initial Data...");
//             byte[] buffer = new byte[32];
//             // int bytesRead = stream.Read(buffer, 0, buffer.Length);
//             // if (bytesRead > 0) {
//             //     writer.Write(new byte[] { 0x00 });
//             //     writer.Flush();
//             //     int messageIdentifier = buffer[0];
//             //     Debug.Log(messageIdentifier);
//             // }
            
//         } catch (Exception e) {
//             Debug.LogError($"Could not connect to server: {e.Message}");
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (spacebar.WasPressedThisFrame()) {
//             messageIndex += 1;
//             SendString(messageList[messageIndex % 3]);
//         }
//         if (localTextUpdate.WasPressedThisFrame())
//         {
//             messageIndex += 1;
//             text.text = messageList[messageIndex % 3];
//         }
//         // if(Input.GetKeyDown(KeyCode.B)){
//         //     fixed_input_count += 1;
//         //     Debug.Log("Spawn");
//         //     RequestSpawnCharacter(new byte[]{ 0x01 });
//         // }
//         // if(Input.GetKeyDown(KeyCode.G)){
//         //     fixed_input_count += 1;
//         //     Debug.Log("Spawn");
//         //     RequestSpawnCharacter(new byte[]{ 0x02 });
//         // }
//         if (stream != null && stream.DataAvailable)
//         {
//             Debug.Log("Reading");
//             byte[] buffer = new byte[4096]; // Assume no larger than 4096
//             int bytesRead = stream.Read(buffer, 0, buffer.Length);
//             if (bytesRead > 0)
//             {
//                 int messageIdentifier = buffer[2];
//                 int length = BitConverter.ToInt32(buffer, 3);
//                 if (messageIdentifier == 0){
//                     Debug.Log($"Bytes read: {bytesRead}");
//                     Debug.Log($"{length} bytes remaining");
//                     Debug.Log($"Bytes: {ShowBytesAsString(buffer)}");
//                     Debug.Log(messageIdentifier);
//                     //Show error message
//                     Debug.Log("ERROR");
//                     string newText = Encoding.UTF8.GetString(buffer, 11, bytesRead - 11);
//                     Debug.Log(newText);
//                 }
//                 if (messageIdentifier == 1){
//                     Debug.Log($"Bytes read: {bytesRead}");
//                     Debug.Log($"{length} bytes remaining");
//                     Debug.Log($"Bytes: {ShowBytesAsString(buffer)}");
//                     Debug.Log(messageIdentifier);
//                     string newText = Encoding.UTF8.GetString(buffer, 11, bytesRead - 11);
//                     Debug.Log(newText);
//                     text.text = newText;
//                 }
//                 if (messageIdentifier == 2){
//                     Debug.Log($"Bytes read: {bytesRead}");
//                     Debug.Log($"{length} bytes remaining");
//                     Debug.Log($"Bytes: {ShowBytesAsString(buffer)}");
//                     Debug.Log(messageIdentifier);
//                     //Show error message
//                     Debug.Log("SUCCESS");
//                     position.x = BitConverter.ToSingle(buffer, 15);
//                     position.y = BitConverter.ToSingle(buffer, 19);
//                     position.z = BitConverter.ToSingle(buffer, 23);
//                     GameObject spawnee = Instantiate(playerCharacter, position, Quaternion.identity);
//                 }
//                 else if(messageIdentifier == 3) {
//                     Debug.Log($"Getting vector from buffer: {ShowBytesAsString(buffer)}");
//                     position.x = BitConverter.ToSingle(buffer, 11);
//                     position.y = BitConverter.ToSingle(buffer, 15);
//                     position.z = BitConverter.ToSingle(buffer, 19);
//                     player_pos.position = position;
//                 }
//             }
//         }
//     }
//     void FixedUpdate()
//     {
//         if(should_flip){
//             fu_count++;
//             if (fu_count % 15 == 0) {
//                 first_location = !first_location;
//                 if (first_location){
//                     messageIndex += 1;
//                     Debug.Log("Sending vector");
//                     RequestMoveCharacter(new Vector3(3.5f, 0.0f, 1.0f));
//                 }
//                 else{
//                     messageIndex += 1;
//                     Debug.Log("Sending vector");
//                     RequestMoveCharacter(new Vector3(-3.5f, 0.0f, -1.0f));
//                 }
//             }
//         }
//     }

//     string ShowBytesAsString(byte[] bytes){
//         string message = "";
//         for (int i = 0; i < bytes.Length; i++){
//             if (i != bytes.Length - 1){
//                 message += $"{bytes[i]:X2} ";
//             } else {
//                 message += $"{bytes[i]:X2}";
//             }
//         }

//         return message;

//     }

//     private void SendString(string message)
//     {
//         if (stream != null && client.Connected){
//             int message_size = message.Length;
//             byte[] message_size_in_bytes = BitConverter.GetBytes(message_size);
//             byte[] bytes_message = Encoding.UTF8.GetBytes(message);
//             byte[] prepended_bytes_message = new byte[bytes_message.Length + 5];
//             prepended_bytes_message[0] = 0x01;
//             Buffer.BlockCopy(message_size_in_bytes, 0, prepended_bytes_message, 1, message_size_in_bytes.Length);
//             Buffer.BlockCopy(bytes_message, 0, prepended_bytes_message, 5, bytes_message.Length);
//             Debug.Log($"Sending {prepended_bytes_message.Length}");
//             writer.Write(prepended_bytes_message);
//             writer.Flush();
//             Debug.Log("Sent: " + message + " as " + ShowBytesAsString(prepended_bytes_message));
//         }
//         else {
//             Debug.LogError("Socket connection is not established");
//         }
//     }
//     private void RequestSpawnCharacter(byte[] context_for_spawn){
//         if (stream != null && client.Connected){
//             byte[] buffer = new byte[6];
//             // Use 0x02 as a signifier code the server will interpret as a request to spawn a character.
//             Buffer.BlockCopy(new byte[] { 0x02 }, 0, buffer, 0, 1);
//             // Provide additional context in the main block of data using the byte 0x01 to indicate this is an initial spawn
//             byte[] length_in_bytes = BitConverter.GetBytes(context_for_spawn.Length);
//             Buffer.BlockCopy(length_in_bytes, 0, buffer, 1, length_in_bytes.Length);
//             Buffer.BlockCopy(context_for_spawn, 0, buffer, 5, context_for_spawn.Length);
//             byte[] printable_buffer = new byte[buffer.Length];
//             Array.Copy(buffer, printable_buffer, buffer.Length);
//             writer.Write(buffer);
//             writer.Flush();
//             Debug.Log("Sent: " + ShowBytesAsString(buffer));
//             Debug.Log("Printable buffer: " + ShowBytesAsString(printable_buffer));
//         }
//         else {
//             Debug.LogError("Socket connection is not established");
//         }
//     }
//     private void RequestMoveCharacter(Vector3 pos)
//     {
//         if (stream != null && client.Connected){
//             byte[] buffer = new byte[17];
//             Buffer.BlockCopy(new byte[] { 0x03 }, 0, buffer, 0, 1);
//             Buffer.BlockCopy(BitConverter.GetBytes(12), 0, buffer, 1, 4);
//             Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buffer, 5, 4);
//             Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buffer, 9, 4);
//             Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, buffer, 13, 4);
//             byte[] printable_buffer = new byte[buffer.Length];
//             Array.Copy(buffer, printable_buffer, buffer.Length);
//             // if (BitConverter.IsLittleEndian){
//             //     Array.Reverse(buffer, 5, 4);
//             //     Array.Reverse(buffer, 9, 4);
//             //     Array.Reverse(buffer, 13, 4);
//             // }
//             writer.Write(buffer);
//             writer.Flush();
//             Debug.Log("Sent: " + ShowBytesAsString(buffer));
//             Debug.Log("Printable buffer: " + ShowBytesAsString(printable_buffer));
//         }
//         else {
//             Debug.LogError("Socket connection is not established");
//         }
//     }

//     private void OnApplicationQuit(){
//         if (stream != null){
//             writer.Close();
//             reader.Close();
//             stream.Close();
//         }
//         if (client != null){
//             client.Close();
//         }
//     }
// }

// // public class ChangeText : MonoBehaviour
// // {
// //     TcpClient client;
// //     NetworkStream stream;
// //     BinaryReader reader;
// //     BinaryWriter writer;
// //     public string serverIP = "192.168.0.59";
// //     public int port = 12544;
// //     public TextMeshProUGUI text;
// //     public GameObject plane;
// //     public GameObject player;

// //     // Start is called before the first frame update
// //     void Start()
// //     {
// //         ConnectToServer();
// //     }
// //     void ConnectToServer(){
// //         try{
// //             client = new TcpClient(serverIP,port);
// //             stream = client.GetStream();
// //             reader = new BinaryReader(stream);
// //             writer = new BinaryWriter(stream);
// //             Debug.Log("connected to server");
// //         } catch (Exception e) {
// //             Debug.LogError($"Could not connect to server: {e.Message}");
// //         }
// //     }

// //     // Update is called once per frame
// //     void Update()
// //     {
// //         if (stream != null && stream.DataAvailable)
// //         {
// //             byte[] buffer = new byte[12];
// //             int bytesRead = stream.Read(buffer, 0, buffer.Length);

// //             if (bytesRead == buffer.Length)
// //             {
// //                 if (BitConverter.IsLittleEndian){
// //                     Array.Reverse(buffer, 0, 4);
// //                     Array.Reverse(buffer, 4, 4);
// //                     Array.Reverse(buffer, 8, 4);
// //                 }
                
// //                 float x = BitConverter.ToSingle(buffer, 0);
// //                 float y = BitConverter.ToSingle(buffer, 4);
// //                 float z = BitConverter.ToSingle(buffer, 8);
                
// //                 Vector3 new_position = new Vector3(x,y,z);
// //                 Debug.Log("Vector received from server: " + new_position);
// //                 player.transform.position = new_position;
// //             }
// //         }

// //         // RequestMoveCharacter(player);

        
// //     }


// //     private void RequestMoveCharacter(GameObject player)
// //     {
// //         if (stream != null && client.Connected){
// //             byte[] buffer = new byte[12];
// //             Buffer.BlockCopy(BitConverter.GetBytes(player.transform.position.x), 0, buffer, 0, 4);
// //             Buffer.BlockCopy(BitConverter.GetBytes(player.transform.position.y), 0, buffer, 4, 4);
// //             Buffer.BlockCopy(BitConverter.GetBytes(player.transform.position.z), 0, buffer, 8, 4);
// //             byte[] printable_buffer = new byte[buffer.Length];
// //             Array.Copy(buffer, printable_buffer, buffer.Length);
// //             if (BitConverter.IsLittleEndian){
// //                 Array.Reverse(buffer, 0, 4);
// //                 Array.Reverse(buffer, 4, 4);
// //                 Array.Reverse(buffer, 8, 4);
// //             }
// //             writer.Write(buffer);
// //             writer.Flush();
// //             Debug.Log("Sent: " + printable_buffer);
// //         }
// //         else {
// //             Debug.LogError("Socket connection is not established");
// //         }
// //     }

// //     private void OnApplicationQuit(){
// //         if (stream != null){
// //             writer.Close();
// //             reader.Close();
// //             stream.Close();
// //         }
// //         if (client != null){
// //             client.Close();
// //         }
// //     }
// // }
