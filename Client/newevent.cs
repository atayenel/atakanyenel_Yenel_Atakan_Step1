﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ClientSide
{
    public partial class newevent : Form
    {
        public newevent()
        {
            InitializeComponent();
        }
        private Form1 mainForm = null;
        public newevent(Form callingForm)
        {
           mainForm = callingForm as Form1;
           InitializeComponent();
        }
        private void createButton()
        {
            if (txtDescription.Text == "" || txtTitle.Text == "" || txtPlace.Text == "" ){
                MessageBox.Show("FILL ALL THE FIELDS");
            }
            else{
            string date = dtpDate.Value.ToShortDateString();
            string place = txtPlace.Text;
            string description = txtDescription.Text;
            string organizer = txtOrganizer.Text;
            string title = txtTitle.Text;
            this.mainForm.setIsItEvent("%" + date + "%" + title + "%" + place + "%" + description + "%" + organizer + "%");
            MessageBox.Show("EVENT CREATED:" + title);
            this.mainForm.sendButton();
            clear();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            createButton();
            this.ActiveControl = button3;
        }

        private void clear()
        {
            txtDescription.Clear();
            txtPlace.Clear();
            txtTitle.Clear();
            dtpDate.Value = DateTime.Now;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clear();
        }

        private void newevent_Load(object sender, EventArgs e)
        {
            txtOrganizer.Text = this.mainForm.getIsItOwner();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtOrganizer_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = button1;
        }
    }
}
