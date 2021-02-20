using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;

namespace TCPCilent
{
    public class Rdbs
    {
        ArrayList arrDatalist = new ArrayList();//存储需要发送的数据
        ArrayList arrSendDataList = new ArrayList();//存储改变了值的数据

        private TcpClient client;//声明TCP客户端
        private ThreadStart threadStart;//声明一个线程
        private Thread client_th;
        private string sip;
        private int iPort;
        //构造函数进行数据的初始化
        public void Rdb(string strip, ArrayList list, int Port)
        {
            arrDatalist = list;
            sip = strip;
            iPort = Port;
            connect_s();
        }

        //连接服务器
        private void connect_s()
        {
            try
            {
                client = new TcpClient(sip, iPort);
                threadStart = new ThreadStart(AcceptMsg);
                client_th = new Thread(threadStart);
                client_th.Start();
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        //接收数据方法，在程序运行的时候开启一个线程进行数据的接收
        private void AcceptMsg()
        {
            NetworkStream ns = client.GetStream();
            //字组处理
            while (true)
            {
                try
                {
                    byte[] bytes = new byte[4096];
                    byte[] sendBytes = new byte[4096];
                    NetworkStream sendStream1 = client.GetStream();
                    int bytesread = ns.Read(bytes, 0, bytes.Length);
                    string msg = Encoding.UTF8.GetString(bytes, 0, bytesread);
                    for (int i = 0; i < arrDatalist.Count; i++)
                    {
                        string strItemData = (string)arrDatalist[i];
                        string[] Data = strItemData.Split('|');
                        string[] DataReceive = msg.Split('|');

                        if (Data[0].ToString() == DataReceive[1].ToString() && DataReceive[0].ToString() == "val")
                        {
                            arrDatalist.RemoveAt(i);
                            string strNewData = DataReceive[1] + "|" + DataReceive[2];
                            arrDatalist.Add(strNewData);
                            sendBytes = Encoding.UTF8.GetBytes("ret|" + DataReceive[1] + "|ok!");
                            sendStream1.Write(sendBytes, 0, sendBytes.Length);
                            sendStream1.Flush();
                        }
                    }
                    ns.Flush();
                }

                catch (System.Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
        }

        public void Sendmessage()
        {
            if (client == null)
                return;
            NetworkStream sendStream = client.GetStream();
            Byte[] sendBytes;
            if (arrSendDataList.Count > 0)
            {
                for (int i = 0; i < arrSendDataList.Count; i++)
                {
                    string message = arrSendDataList[i].ToString();
                    arrSendDataList.RemoveAt(i);
                    sendBytes = Encoding.UTF8.GetBytes(message);
                    sendStream.Write(sendBytes, 0, sendBytes.Length);
                    sendStream.Flush();
                }
            }
        }

        //修改原始数据里面的值并发送数据
        public void ModiData(string strName, string value)
        {
            try
            {
                int iCount = arrDatalist.Count;
                if (iCount > 0)
                {
                    for (int i = 0; i < iCount; i++)
                    {
                        string strItemData = (string)arrDatalist[i];
                        string[] Data = strItemData.Split('|');
                        if (Data[0].ToString() == strName)
                        {
                            arrDatalist.RemoveAt(i);
                            string strNewData = Data[0] + "|" + value;
                            arrDatalist.Add(strNewData);
                            arrSendDataList.Add("val|" + strNewData);
                            break;
                        }
                    }
                    Sendmessage();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        //退出整个应用
        public void Exit()
        {
            if (client != null)
            {
                client.Close();
            }
        }
    }
}