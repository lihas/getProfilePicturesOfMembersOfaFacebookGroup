using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using LitJson;
using System.IO;
using System.Runtime.InteropServices;

namespace getProfilePicturesOfMembersOfaFacebookGroup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Control[] imgArray;  //global member contains array of imageboxes to be used while displaying and saving images
        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("init");
            //clearing the panel and list view
            panel1.Controls.Clear();
            listView1.Items.Clear();       

            string groupid = groupId.Text;
            string accesstoken = accessToken.Text;
            if (groupid == "" || accesstoken == "") { MessageBox.Show("groupId or authToken Missing"); return; }
            string url = "https://graph.facebook.com/"+groupid+"/members?access_token="+accesstoken;
            WebClient wc=new WebClient();
            string resJson="";
            try
            {
                resJson = wc.DownloadString(url);
            }
            catch (System.Net.WebException exptn) { Console.WriteLine("exception occured, check access_token and/or net connection. Message:{0}", exptn.Message); MessageBox.Show("Exception occured. It might be due to expired access_token.", "Something's Wrong!"); return; }
            
            JsonData jd = JsonMapper.ToObject(resJson);
            //jd[0] ->data // jd[0][i][0] ->name jd[0][i][1] ->id jd[i][j][2] ->is administrator?
            //jd[1] ->paging //jd[1][0] -> some url given by fb //doesn't concern me as of now
            JsonData data=jd[0];
            int nomembers=data.Count; //nomembers =no of members // max has to be 500(as per FB docs) but as of now >500 can be seen
            string profilePicUrl = "";
            int nocols = (int)((double)nomembers*50/(double)panel1.Width); //no of rows of table layout
            int norows = (int)Math.Ceiling((double)nomembers/(double)nocols);
            Console.WriteLine("columns: {0} rows: {1} noOfMembers {2}",nocols,norows,nomembers);
            TableLayoutPanel tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.BackColor = Color.AliceBlue;
           
            tableLayoutPanel1.Width = panel1.Width-(10/100)*panel1.Width; //10 % less than pannel height to prevent appearance of scroll bars
            tableLayoutPanel1.Height = norows*(50+6); //size of each image=50, margin=6 thus total=56           
            tableLayoutPanel1.ColumnCount = nocols;
            tableLayoutPanel1.RowCount = norows;
            panel1.Controls.Add(tableLayoutPanel1);

            imgArray = new Control[nomembers];
            for (int i = 0,j=-1; i < nomembers; i++)
            {
                

                profilePicUrl = "http://graph.facebook.com/" + data[i][1] + "/picture"; //might break in future since https is not used
                {
                    ListViewItem li= new ListViewItem();
                    li.Text=(string)data[i][0];
                    li.Name = (string)data[i][1];
                    li.Tag = i;
                    
                    listView1.Items.Add(li);
                    
                }
              
                PictureBox p = new PictureBox();
                p.Width = 50;
                p.Height = 50;
                p.ImageLocation = profilePicUrl;
                imgArray[i] = p;
            }
            tableLayoutPanel1.Controls.AddRange(imgArray);
            Console.WriteLine("row: {0} col: {1}",tableLayoutPanel1.RowCount,tableLayoutPanel1.ColumnCount);
            
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
           Console.WriteLine("saving images");
           if (folderPath.Text == "") { MessageBox.Show("destination folder not set.", "Error!"); return; }
           if (listView1.Items.Count == 0) { MessageBox.Show("Nothing to save"); return; }
           foreach( ListViewItem itm in listView1.Items){
               if (itm.Checked)
               {

                   Image im;
                   //the following if-else block is based on the fact that the PictureBox element loads image only if it is in visible area, thus to save bandwidth following code gets image from PictureBox if it has been loaded else  it will fetch from facebook over http thus it is not necessary to load all images at once or to fetch required images again while saving
                   if (((PictureBox)imgArray[(int)itm.Tag]).Image != null)
                   {
                       im = ((PictureBox)imgArray[(int)itm.Tag]).Image;
                   }
                   else
                   {
                    im=Image.FromStream(new MemoryStream((new WebClient()).DownloadData("http://graph.facebook.com/" + itm.Name + "/picture"))); //needs correction since image is being dwnlded 2nd time
                   }
                   im.Save(folderPath.Text + "\\" + itm.Text+".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

               }               
           }
          MessageBox.Show("Images Saved.");
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem itm in listView1.Items) { itm.Checked = true;}
        }

        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem itm in listView1.Items) {itm.Checked = false;}
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();//#allocating console for easy debugging
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
   
    }

    
}
