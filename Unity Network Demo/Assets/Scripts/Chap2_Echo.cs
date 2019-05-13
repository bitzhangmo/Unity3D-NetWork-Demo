using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;


public class Chap2_Echo : MonoBehaviour {

	// 定义套接字
	Socket socket;
	// UGUI
	public InputField inputField;
	public Text text;
	// 接收缓冲区
	byte[] readBuff = new byte[1024];
	string recvStr = "";

	// 点击连接按钮
	public void Connection()
	{
		// Socket
		socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
		// Connect
		socket.BeginConnect("127.0.0.1",8888,ConnectCallback,socket);
	}
	
	// Connect 回调
	public void ConnectCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket) ar.AsyncState;
			socket.EndConnect(ar);
			Debug.Log("Socket Connect Succ");
			socket.BeginReceive(readBuff,0,1024,0,ReceiveCallback,socket);
		}
		catch (SocketException ex)
		{
			Debug.Log("Socket Connect fail" + ex.ToString());
		}
	}

	// 点击发送按钮
	public void Send()
	{
		// Send
		string sendStr = inputField.text;
		byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
		//socket.Send(sendBytes);
		socket.BeginSend(sendBytes,0,sendBytes.Length,0,SendCallback,socket);

		// Reveice
		//byte[] readBuff = new byte[1024];
		//int count = socket.Receive(readBuff);
		//string recvStr = System.Text.Encoding.Default.GetString(readBuff,0,count);

		//text.text = recvStr;

		// Close
		//socket.Close();
	}

	public void SendCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket) ar.AsyncState;
			int count = socket.EndSend(ar);
			Debug.Log("Socket Send Succ" + count);
		}
		catch(SocketException ex)
		{
			Debug.Log("Socket Send Fail" + ex.ToString());
		}
	}
	public void ReceiveCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket) ar.AsyncState;
			int count = socket.EndReceive(ar);
			recvStr = System.Text.Encoding.Default.GetString(readBuff,0,count);

			socket.BeginReceive(readBuff,0,1024,0,ReceiveCallback,socket);
		}
		catch (SocketException ex)
		{
			Debug.Log("Socket Receive fail" + ex.ToString());
		}
	}

	public void Update()
	{
		if(socket == null)
		{
			return;
		}

		if(socket.Poll(0,SelectMode.SelectRead))
		{
			byte[] readBuff = new byte[1024];
			int count = socket.Receive(readBuff);
			string recvStr = System.Text.Encoding.Default.GetString(readBuff,0,count);
			text.text = recvStr;
		}
		//text.text = recvStr;
	}

}
