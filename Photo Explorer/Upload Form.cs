﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Photo_Explorer
{
    public partial class Upload_Form : Form
    {
        //Global variables
        List<String> Photo_Path = new List<String>();

        public Upload_Form()
        {
            InitializeComponent();
        }

        MySqlConnection con = new MySqlConnection(Properties.Resources.connectionString);

        private void Browse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                foreach (String file in openFileDialog.FileNames)
                {
                    String path = Path.GetFullPath(file);
                    String correctedPath = path.Replace(@"\", @"\\");
                    Photo_Path.Add(correctedPath);
                    lb_load.Visible = true;
                }
            }
        }

        private async void Upload_Click(object sender, EventArgs e)
        {
            int albumID = 0;

            lb_load.Visible = false;

            pb_Loading.Image = Properties.Resources._91;
            pb_Loading.SizeMode = PictureBoxSizeMode.AutoSize;
            pb_Loading.BackColor = Color.Transparent;
            pb_Loading.BringToFront();
            pb_Loading.Visible = true;

            try
            {
               if (tb_albumName.Text == "")
               {
                  MessageBox.Show("Album name is empty!");
               }
               else
               {
                  await Task.Run(() =>
                  {
                      //Insert the Album
                      MySqlCommand cmd = new MySqlCommand("Insert into Album (AlbumName) values('" + tb_albumName.Text + "' )", con);
                      cmd.CommandTimeout = 60;
                      con.Open();
                      cmd.CommandType = CommandType.Text;
                      cmd.ExecuteNonQuery();
                      con.Close();

                      //Get the last Album ID
                      cmd = new MySqlCommand("Select LAST_INSERT_ID();", con);
                      cmd.CommandTimeout = 60;
                      con.Open();
                      cmd.CommandType = CommandType.Text;
                      cmd.ExecuteNonQuery();

                      MySqlDataReader rdr = cmd.ExecuteReader();
                      while (rdr.Read())
                      {
                          albumID = Convert.ToInt32(rdr["LAST_INSERT_ID()"]);
                      }
                      con.Close();

                      //Upload Photos for the Album with ID above
                      for (int i = 0; i < Photo_Path.Count; i++)
                      {
                          cmd = new MySqlCommand("Insert into Photo (PhotoData, AlbumID) values('" + Photo_Path[i] + "','" + albumID + "' )", con);
                          cmd.CommandTimeout = 60;
                          con.Open();
                          cmd.CommandType = CommandType.Text;
                          cmd.ExecuteNonQuery();
                          con.Close();
                      }
                  });
                        
                  MessageBox.Show("Album have been uploaded!");

                  this.Close();
               }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }
}