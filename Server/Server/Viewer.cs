﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using System.Threading.Tasks;
using System.Threading;

namespace Server
{
    public partial class Viewer : Form
    {
        //public static int pastClientsCount;
        //public static int currentClientsCount; // 현재 접속 클라이언트 수
        public int pastClientsCount { get; set; }
        public int currentClientsCount { get; set; } // 현재 접속 클라이언트 수

        public static FullViewer fullViewer;

        //public static Dictionary<string, Student> clientsList;
        public Dictionary<string, Student> clientsList
        {
            get;
            set;
        }


        public Dictionary<string, PictureBox> clientsPicture
        {
            get;set;
        }

        public String dir;
        public class Student
        {
            public string info;
            public Image img;
        }

        public static System.Windows.Forms.Timer renderingTimer;
        public static System.Windows.Forms.Timer focusingTimer;

       

        Action[] actions;

        public Viewer()
        {
            InitializeComponent();

            pastClientsCount = 0;
            currentClientsCount = 0;

            clientsList = new Dictionary<string, Student>(); // 클라이언트 리스트
            clientsPicture = new Dictionary<string, PictureBox>(); // 클라이언트 이미지

            renderingTimer = new System.Windows.Forms.Timer();
            //renderingTimer.Tick += new EventHandler(IterateShowViews);
            renderingTimer.Interval = 500;
            renderingTimer.Start();

            focusingTimer = new System.Windows.Forms.Timer();

            this.ShowInTaskbar = false;

            //List<Action> aavv = new List<Action>();
            //actions = aavv.ToArray();
            ////aavv
            //Parallel.Invoke(actions);

            //aavv. = new Action(() => {
            //    Task.Run(() => { 

            //    }).ContinueWith((_) => {

            //    });

            //});
            

            List<string> list = new List<string>(clientsList.Keys);
            panels = new List<Panel>();
            
            

            Task.Run(() =>
            {
                while (true)
                {

                    Console.WriteLine("currentClientsCount" + currentClientsCount+ "/// clientsList.Count : " + clientsList.Count + "\n");
                    
                    if (panels.Count != 0)
                    {
                        Console.WriteLine("panels.Count" + panels.Count);
                    }

                    //for(int i = 0; i < clientsList.Count; i++)
                    //{
                    //    foreach (string key in clientsList.Keys)
                    //    {
                    //        Student stu = clientsList[key] as Student;
                    //        clientsPicture[key].Image = stu.img;
                    //    }
                    //}


                    Thread.Sleep(500);
                }

            });
            

        }

        List<Panel> panels;
        public void InsertPanel(String clientAddr)
        {
            Student stu = clientsList[clientAddr] as Student;

            Panel panClient = new Panel();

            Label lblClient = new Label
            {
                Width = 160,
                Height = 20,
                Location = new Point(5, 30),
                Text = stu.info // 학번(이름)
            };

            PictureBox pbClient = new PictureBox
            {
                Name = stu.info,
                Width = 160,
                Height = 120,
                Location = new Point(5, 50),
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = stu.img // 캡처 이미지
            };

            panClient.Controls.Add(lblClient);
            panClient.Controls.Add(pbClient);

            panClient.Size = new Size(170, 170);
            panClient.Location = new Point(0, 0);
            panClient.Name = clientAddr;

            panels.Add(panClient);
        }

       
        

        public void GetPicture(PictureBox box, MouseEventArgs e)
        {
            var filePath = string.Empty;
            if (e.Button == MouseButtons.Right)
            {
                using (FolderBrowserDialog fileDialog = new FolderBrowserDialog())
                {
                    fileDialog.SelectedPath = "C:\\";

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = fileDialog.SelectedPath;

                        Bitmap bmp = new Bitmap(box.Image);
                        String str = box.Name.ToString().Trim('\0');
                        bmp.Save($"{filePath}\\{str}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
        }

        public void FullPicture(Student sendStu)
        { 
            focusingTimer.Tick += new EventHandler((sender, e) => InterateFocusView(sendStu.img));
            focusingTimer.Interval = 500;
            focusingTimer.Start();

            fullViewer = new FullViewer(sendStu);
            fullViewer.ShowDialog();

            renderingTimer.Interval = 700;
        }

        public Panel AddClientPanel(string key)
        {
            Student stu = clientsList[key] as Student;

            Panel panClient = new Panel();
            

            Label lblClient = new Label
            {
                Width = 160,
                Height = 20,
                Location = new Point(5, 30),
                Text = stu.info // 학번(이름)
            };

            PictureBox pbClient = new PictureBox
            {
                Name = stu.info, 
                Width = 160,
                Height = 120,
                Location = new Point(5, 50),
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = stu.img // 캡처 이미지
            };

            void customMouseEvent(object sender, MouseEventArgs e) => GetPicture(pbClient, e);
            //void customEvent(object sender, EventArgs e) => FullPicture(stu);
            
            pbClient.MouseDown += customMouseEvent;
            //pbClient.DoubleClick += customEvent;


            if (clientsPicture.ContainsKey(key)) clientsPicture[key] = pbClient;
            else clientsPicture.Add(key, pbClient);

            panClient.Controls.Add(lblClient);
            panClient.Controls.Add(pbClient);

            panClient.Size = new Size(170, 170);
            panClient.Location = new Point(0, 0);

            return panClient;
        }

        private void IterateShowViews(object sender, EventArgs e)
        {
            try
            {
                List<string> list = new List<string>(clientsList.Keys);

                if (pastClientsCount == currentClientsCount)
                {
                    foreach (string key in clientsList.Keys)
                    {
                        Student stu = clientsList[key] as Student;
                        clientsPicture[key].Image = stu.img;
                    }
                }
                else if (pastClientsCount < currentClientsCount)
                {
                    ++pastClientsCount;

                    //clientsViewPanel.Controls.Clear();
                    //foreach (string key in list)
                    //{
                    //    clientsViewPanel.Controls.Add(AddClientPanel(key));
                    //}
                }
                else if (pastClientsCount > currentClientsCount)
                {
                    --pastClientsCount;

                    //clientsViewPanel.Controls.Clear();
                    //foreach (string key in list)
                    //{
                    //    clientsViewPanel.Controls.Add(AddClientPanel(key));
                    //}
                }
            }
            catch (Exception)
            {
                return ;
            }
        }

        private void InterateFocusView(Image sendImg)
        {
            FullViewer.focusStudent.img = sendImg;
        }

        private void BtnAllSave_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;
            if (pastClientsCount == 0)
            {
                MessageBox.Show("접속한 학생이 없습니다.", "화면 캡처 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                using (FolderBrowserDialog fileDialog = new FolderBrowserDialog())
                {
                    fileDialog.SelectedPath = "C:\\";

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = fileDialog.SelectedPath;

                        foreach (string key in clientsPicture.Keys)
                        {
                            String str = clientsPicture[key].Name.ToString().Trim('\0');
                            Bitmap bmp = new Bitmap(clientsPicture[key].Image);
                            bmp.Save($"{filePath}\\{str}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }                
            }
        }
    }
}
