﻿using System;
using System.Drawing;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Client
{
    public partial class Client : Form
    {
        private const int MOSHPORT = 53178;
        private const string SERVER_IP = "127.0.0.1";

        private Socket socketServer;
        private bool isConnected = false;

        private static Action mainAction;

        private static SignalObj standardSignalObj;

        private CmdProcessController cmdProcessController;

        private delegate void ScreenOnDelegate(int imgSize, Byte[] recvData, double isOpacity);
        private delegate void ScreenOffDelegate(double isOpacity);

        private static Byte[] recvData;
        private static Byte[] sendData;

        public Client()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            while (!isConnected)
            {
                try
                {
                    socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), MOSHPORT);
                    socketServer.Connect(serverEndPoint);

                    // 작업표시줄 상에서 프로그램이 표시되지 않도록 설정
                    // 개인 테스트 과정에서 불편하므로 커밋할 때는 주석처리 해주세요.
                    // this.ShowInTaskbar = false;

                    // 받은 이미지를 풀스크린으로 띄우는 설정
                    // 개인 테스트 과정에서 불편하므로 커밋할 때는 주석처리 해주세요.
                    /*FormBorderStyle = FormBorderStyle.None;*/
                    WindowState = FormWindowState.Maximized;
                    screenImage.Width = Screen.PrimaryScreen.Bounds.Width;
                    screenImage.Height = Screen.PrimaryScreen.Bounds.Height;

                    // 화면 폼을 가장 맨 위로
                    // 개인 테스트 과정에서 불편하므로 커밋할 때는 주석처리 해주세요.
                    // TopMost = true;

                    isConnected = true;
                }
                catch (SocketException)
                {
                    isConnected = false; // 연결이 안 되면 대기상태 유지
                }
            }

            cmdProcessController = new CmdProcessController();

            recvData = new Byte[327675]; // 327,675 Byte = 65,535 Byte * 5
            sendData = Encoding.UTF8.GetBytes("recv");

            Opacity = 0;

            // 이런식으로 구현 기능에 대한 메소드를 추가하기 위해 아래와 같이 람다식으로 작성
            InsertAction(() => ControllingProcessing());
            InsertAction(() => ImageProcessing());

            Task.Run(()=> MainTask());
        }

        public void InsertAction(Action action)
        {
            mainAction += action;
        }

        public SignalObj ByteToObject(byte[] buffer)
        {
            string jsonData = "";
            SignalObj signal;

            jsonData = Encoding.Default.GetString(buffer);
            signal = JsonConvert.DeserializeObject<SignalObj>(jsonData);
            return signal;
        }

        public SignalObj ReceiveObject()
        {
            try
            {
                socketServer.Send(sendData);
                socketServer.Receive(recvData);

                return ByteToObject(recvData);

            }
            catch (SocketException)
            {
                socketServer.Close();
                if (cmdProcessController.NowCtrlStatus) cmdProcessController.QuitProcess();
                this.Invoke(new MethodInvoker(() => { Dispose(); }));

                return null;
            }
        }

        public void MainTask()
        {
            while (true)
            {
                try
                {
                    using (standardSignalObj = ReceiveObject())
                    {
                        if (standardSignalObj != null) mainAction();
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (JsonReaderException)
                {
                    break;
                }
                finally
                {
                    Array.Clear(recvData, 0, recvData.Length);
                }
            }
        }

        public void ControllingProcessing()
        {
            cmdProcessController.CtrlStatusEventCheck(standardSignalObj.IsServerControlling);
        }

        public void ImageProcessing()
        {
            if (standardSignalObj.ServerScreenData != null)
            {
                // 이미지를 받아서 여기서 버퍼를 설정하는 부분
                this.Invoke(new ScreenOnDelegate(OutputDelegate),
                    standardSignalObj.ServerScreenData.Length, standardSignalObj.ServerScreenData, 1);
                
            }
            else
            {
                this.Invoke(new ScreenOffDelegate(OpacityDelegate), 0);
            }
        }

        public void OpacityDelegate(double isOpacity)
        {
            Opacity = isOpacity;
        }

        public void OutputDelegate(int imgSize, Byte[] recvData, double isOpacity)
        {
            Opacity = isOpacity;

            using (MemoryStream pre_ms = new MemoryStream(recvData))
            {
                using (MemoryStream post_ms = new MemoryStream())
                {
                    using (DeflateStream ds = new DeflateStream(pre_ms, CompressionMode.Decompress))
                    {
                        try
                        {
                            ds.CopyTo(post_ms);
                        }
                        finally
                        {
                            ds.Close();
                        }
                    }
                    imageSize.Text = imgSize.ToString();
                    screenImage.Image = Image.FromStream(post_ms);

                    post_ms.Close();
                }
                pre_ms.Close();
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            socketServer.Close();
            if (cmdProcessController.NowCtrlStatus) cmdProcessController.QuitProcess();
            this.Invoke(new MethodInvoker(() => { Dispose(); }));
        }
    }
}
