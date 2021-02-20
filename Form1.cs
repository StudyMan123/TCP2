using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using TCPCilent;
using System.Collections;

namespace TCPCall
{
    public partial class Form1 : Form
    {
        private Socket mysocket = null;
        public const int TCPBufferSize = 1460;//缓存的最大数据个数
        public byte[] TCPBuffer = new byte[TCPBufferSize];//缓存数据的数组

        public Form1()
        {
            InitializeComponent();
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (BtnConnect.Text == "连接")
            {             
                if (string.IsNullOrEmpty(textIP.Text) == false && string.IsNullOrEmpty(textPort.Text) == false)
                {
                    try
                    {
                        IPAddress ipAddress = IPAddress.Parse(textIP.Text);//获取IP地址
                        int Port = Convert.ToInt32(textPort.Text);          //获取端口号

                        mysocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        //使用 BeginConnect 异步连接
                        mysocket.BeginConnect(ipAddress, Port, new AsyncCallback(ConnectedCallback), mysocket);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("IP地址或端口号错误!", "提示");
                    }
                }
                else
                {
                    MessageBox.Show("IP地址或端口号错误!", "提示");
                }
            }
            else
            {
                try
                {
                    BtnConnect.Text = "连接";
                    mysocket.BeginDisconnect(false, null, null);
                    ReceiveText.AppendText("服务器断开");
                }
                catch (Exception) { }
            }

        }
        void ConnectedCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndConnect(ar);
                socket.BeginReceive(TCPBuffer, 0, TCPBufferSize, 0, new AsyncCallback(ReadCallback), socket);

                Invoke((new Action(() =>
                {
                    ReceiveText.AppendText("成功连接服务器\n");
                    BtnConnect.Text = "断开";
                })));
            }
            catch (Exception e)
            {
                Invoke((new Action(() =>
                {
                    ReceiveText.AppendText("连接失败:" + e.ToString());
                })));
            }
        }
        void ReadCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;//获取链接的Socket
            int CanReadLen = socket.EndReceive(ar);//结束异步读取回调,获取读取的数据个数

            if (CanReadLen > 0)
            {
                string str = Encoding.Default.GetString(TCPBuffer, 0, CanReadLen);//Byte值根据ASCII码表转为 String
                Invoke((new Action(() => //C# 3.0以后代替委托的新方法
                {
                   if(radioBtn1.Checked)
                    {
                        ReceiveText.AppendText(byteToHexStr(TCPBuffer, CanReadLen));
                    }
                   else
                    {
                        ReceiveText.AppendText(Encoding.Default.GetString(TCPBuffer, 0, CanReadLen));
                    }
                })));

                //设置异步读取数据,接收的数据缓存到TCPBuffer,接收完成跳转ReadCallback函数
                socket.BeginReceive(TCPBuffer, 0, TCPBufferSize, 0, new AsyncCallback(ReadCallback), socket);
            }
            else//异常
            {
                Invoke((new Action(() => //C# 3.0以后代替委托的新方法
                {
                    BtnConnect.Text = "连接";
                    ReceiveText.AppendText("\n异常断开\n");//对话框追加显示数据
                })));
                try
                {
                    mysocket.BeginDisconnect(false, null, null);//断开连接
                }
                catch (Exception) { }
            }
        }
        public static string byteToHexStr(byte[] bytes,int Len)
        {
            string returnStr = "";
            try
            {
                if (bytes != null)
                {
                    for (int i = 0; i < Len; i++)
                    {
                        returnStr += bytes[i].ToString("X2");
                        returnStr += " ";//两个16进制用空格隔开,方便看数据
                    }
                }
                return returnStr;
            }
            catch (Exception)
            {
                return returnStr;
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string str = SendText.Text.ToString();
            try
            {
                if(str.Length>0)
                {
                    if (radioBtnsend.Checked)//选择16进制发送
                    {
                        byte[] byteHex = strToToHexByte(str);
                        mysocket.BeginSend(byteHex, 0, byteHex.Length, 0, null, null); //发送数据
                    }
                    else
                    {
                        byte[] byteArray = Encoding.Default.GetBytes(str);//Str 转为 Byte值
                        mysocket.BeginSend(byteArray, 0, byteArray.Length, 0, null, null); //发送数据
                    }
                }   
            }
            catch(Exception)
            {

            }
        }
        private static byte[] strToToHexByte(String hexString)
        {
            int i;
            hexString = hexString.Replace(" ", "");//清除空格
            if ((hexString.Length % 2) != 0)//奇数个
            {
                byte[] returnBytes = new byte[(hexString.Length + 1) / 2];
                try
                {
                    for (i = 0; i < (hexString.Length - 1) / 2; i++)
                    {
                        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                    }
                    returnBytes[returnBytes.Length - 1] = Convert.ToByte(hexString.Substring(hexString.Length - 1, 1).PadLeft(2, '0'), 16);
                }
                catch
                {
                    MessageBox.Show("含有非16进制字符", "提示");
                    return null;
                }
                return returnBytes;
            }
            else
            {
                byte[] returnBytes = new byte[(hexString.Length) / 2];
                try
                {
                    for (i = 0; i < returnBytes.Length; i++)
                    {
                        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                    }
                }
                catch
                {
                    MessageBox.Show("含有非16进制字符", "提示");
                    return null;
                }
                return returnBytes;
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ReceiveText.Clear();
        }
    }
}
