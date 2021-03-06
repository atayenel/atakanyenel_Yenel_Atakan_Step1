﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Net.Sockets;

//SERVER SIDE

namespace CS408_Step1_Server
{
    public partial class Server : Form
    {
        class client
        {
            public string name;
            public Socket clisoc;
            //public int attending;
            public List<string> friendsList = new List<string>();
            internal void setname(string strclientname)
            {
                name = strclientname;
            }
            internal void setsocket(Socket yeni)
            {
                clisoc = yeni;
            }

            internal Socket getsocket()
            {
                return clisoc;
            }
            internal string getname()
            {
                return name;
            }
            internal string getstringfriendsList(int a)
            {
                return friendsList[a];
            }
            internal int getfriendsListCountc()
            {
                return friendsList.Count;
            }
            public void addFriend(string newfriend)
            {
                friendsList.Add(newfriend);
            }
            public bool isItFriend(string nameOf)
            {
                return friendsList.Contains(nameOf);
            }

        };

        DateTime Time;
        List<client> clientarray = new List<client>();
        List<events> eventsarray = new List<events>();
        events tempe = new events();
        Thread thraccept;
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket n;

        public Server()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
        }

        public int getFriendsListCount(int a)
        {
            return clientarray[a].getfriendsListCountc();
        }

        public string getFriendsListItem(int a, int b)
        {
            return clientarray[a].getstringfriendsList(b);
        }

        public int searchClientID(string un)
        {
            int cc = clientarray.Count;
            for (int i = 0; i < cc; i++)
            {
                if (clientarray[i].getname() == un)
                {
                    return i;
                }
            }
            return -1;
        }

        public Socket get_me_socket(int client_ID)
        {
            return clientarray[client_ID].getsocket();
        }

        public void add_friends_2ways(string c1, string c2)
        {
            int c1_ID = searchClientID(c1);
            int c2_ID = searchClientID(c2);

            if ((clientarray[c1_ID].isItFriend(c2) == false) && (clientarray[c2_ID].isItFriend(c1) == false))
            {
                clientarray[c1_ID].addFriend(c2);
                clientarray[c2_ID].addFriend(c1);
            }

            Socket c1_socket = get_me_socket(c1_ID);
            Socket c2_socket = get_me_socket(c2_ID);

            string new_friends = "@" + c1 + "@" + c2 + "@";

            byte[] buffer = new byte[64];
            buffer = Encoding.Default.GetBytes(new_friends);
            c1_socket.Send(buffer);
            c2_socket.Send(buffer);
        }
        public Socket searchClient(string un)
        {
            int cc = clientarray.Count;
            for (int i = 0; i < cc; i++)
            {
                if (clientarray[i].getname() == un)
                {
                    return clientarray[i].getsocket();
                }
            }
            return null;
        }

        public bool isItFriend_server(string nameOf, string sender)
        {
            int ID_client = searchClientID(sender);
            return clientarray[ID_client].isItFriend(nameOf);
        }

        // function for START. With this function the server starts listening to the port that is given by the user.
        // It is handled in the try/cathch method to prevent crashing of the system.
        // If an error occurs a message box will appear and inform the server administrator about the error
        private void start_Click(object sender, EventArgs e)
        {
            if (start.Text == "Start!")
                try
                {
                    s.Bind(new IPEndPoint(IPAddress.Any, Convert.ToInt32(textBox1.Text)));
                    s.Listen(Convert.ToInt32(textBox1.Text));
                    thraccept = new Thread(new ThreadStart(Accept));
                    thraccept.Start();
                    textBox1.Enabled = false;
                    start.Text = "Stop!";
                    richTextBox1.Text += "Server is now listening at port " + textBox1.Text + ".\r\n";
                }
                catch
                {
                    MessageBox.Show("ERROR: SERVER IS UNABLE TO START");
                }

                // if a user wants to close the server a messagebox appears. It is the same procedure s with the server closing.
            // the procedure is differnet if there are clients connected and if there is none
            else
            {
                int clientnumber = clientarray.Count;
                if (clientnumber > 0)
                {
                    DialogResult DR = MessageBox.Show("There are " + clientnumber + " client(s) connected to server.\n Are you sure ?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (DR == DialogResult.Yes)
                    {
                        foreach (client c in clientarray)
                        {
                            c.getsocket().Close();
                        }
                        clientarray.Clear();
                        s.Close();
                        s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        richTextBox1.Text += "Server stopped listening at port " + textBox1.Text + ".\r\n";
                        start.Text = "Start!";
                        textBox1.Enabled = true;
                    }
                    else
                    {
                    }
                }
                else if (clientnumber == 0)
                {
                    DialogResult DR = MessageBox.Show("Are you sure ?", "Server ShutDown", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (DR == DialogResult.Yes)
                    {
                        start.Text = "Start!";
                        textBox1.Enabled = true;
                        s.Close();                                 //Shut the listening Socket !!!
                        s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Redefine the listening socket so it unbinds
                        richTextBox1.Text += "Server stopped listening at port " + textBox1.Text + ".\r\n";
                    }
                }
            }
        }

        // communication with the clients
        private void Accept()
        {
            try
            {
                while (true)
                {
                    n = s.Accept();
                    Thread namereceive;
                    namereceive = new Thread(new ParameterizedThreadStart(Receivename));
                    namereceive.Start(n);
                }
            }
            catch
            {
                // does something happen here?
            }
        }

        private void Receivename(object o)
        {
            try
            {
                Socket yeni = (Socket)o;
                byte[] clientname = new byte[64];
                yeni.Receive(clientname);
                string strclientname = Encoding.Default.GetString(clientname);
                strclientname = strclientname.Substring(0, strclientname.IndexOf("\0"));
                if (checkname(strclientname))
                {
                    clientarray.Add(new client());
                    clientarray[clientarray.Count - 1].setname(strclientname);
                    clientarray[clientarray.Count - 1].setsocket(yeni);
                    string serveranswer = "1";
                    byte[] send = Encoding.Default.GetBytes(serveranswer);
                    yeni.Send(send);
                    Time = DateTime.Now;
                    richTextBox1.Text += "-> " + strclientname + " has connected at " + Time + "." + "\r\n";
                    byte[] sendmessage = new byte[64];
                    string joinedmsg = strclientname + " has joined the conversation.";
                    sendmessage = Encoding.Default.GetBytes(joinedmsg);
                    foreach (client c in clientarray)
                    {
                        if (c.getsocket() != yeni)
                        {
                            c.getsocket().Send(sendmessage);
                        }
                        sendmessage = Encoding.Default.GetBytes("§" + strclientname + "§");
                        c.getsocket().Send(sendmessage);
                    }
                    bool condition = true;
                    while (condition)
                    {
                        byte[] buffer2 = new byte[64];
                        yeni.Receive(buffer2);
                        string newmessage = Encoding.Default.GetString(buffer2);
                        int pos = clientarray.IndexOf(clientarray.Find(client => client.getsocket() == yeni));

                        if (check_symbol(ref newmessage) == 2) //message
                        {

                            string clientsendername = clientarray[pos].getname();
                            sendmessage = Encoding.Default.GetBytes(newmessage);


                            for (int k = 0; k < clientarray.Count; k++)
                            {

                                string any = clientarray[k].getname();
                                bool friend = isItFriend_server(any, clientsendername);
                                // &&
                                if (friend == true)
                                    clientarray[k].getsocket().Send(sendmessage);
                                else if (k == pos)
                                {
                                    Time = DateTime.Now;
                                    richTextBox1.Text = richTextBox1.Text + "-> " + clientsendername + " sent a message at " + Time + ".\r\n";
                                }
                            }
                        }
                        else if (check_symbol(ref newmessage) == 1) // event
                        {
                            string a;
                            string b = newmessage;
                            int index1 = 0;
                            int index2 = 0;
                            string[] event_info = new string[5];
                            for (int i = 0; i < 4; i++)
                            {
                                index1 = b.IndexOf("%");
                                a = b.Substring(index1 + 1);
                                index2 = a.IndexOf("%");
                                event_info[i] = b.Substring(1, index2);
                                b = b.Substring(index2 + 1);
                            }
                            b = b.Substring(1);
                            index1 = b.IndexOf("%");
                            event_info[4] = b.Substring(0, index1);

                            //store all into into a new event in eventsarray
                            events tempe = new events();
                            tempe.setDate(event_info[0]);
                            tempe.setTitle(event_info[1]);
                            tempe.setPlace(event_info[2]);
                            tempe.setDesc(event_info[3]);
                            tempe.setOrganizer(event_info[4]);
                            eventsarray.Add(tempe);

                            //richTextBox1.Text = richTextBox1.Text + "Events counter: " + eventsarray.Count.ToString() + "\r\n";
                            richTextBox1.Text = richTextBox1.Text + "Event " + event_info[1] + " has been added to List." + "\r\n\r\n";
                            foreach (client c in clientarray)
                            {
                                c.getsocket().Send(buffer2);
                            }
                            //iff frinds only
                            //isItFriend_server
                            byte[] buffer33 = Encoding.Default.GetBytes("#" + event_info[4] + " just created an new event!  ");
                            //get that event (event count-1?)
                            int thisEvent = eventsarray.Count-1;
                            foreach (client c in clientarray)
                            {
                                if (c.getsocket() != yeni && isItFriend_server(event_info[4], c.getname())== true)
                                {
                                    c.getsocket().Send(buffer33);
                                    //add c.getname() into it Not reply list of this event
                                    eventsarray[thisEvent].addNotReplyList(c.getname());
                                }
                            }
                        }
                        else if (check_symbol(ref newmessage) == 3) // attendance(symbol: &)
                        {

                            int i1 = 0;
                            int i2 = 0;
                            string A;
                            string B = newmessage;
                            string[] atte_rec = new string[3];
                            for (int i = 0; i < 2; i++)
                            {
                                i1 = B.IndexOf("&");
                                A = B.Substring(i1 + 1);
                                i2 = A.IndexOf("&");
                                atte_rec[i] = B.Substring(1, i2);
                                B = B.Substring(i2 + 1);
                            }
                            B = B.Substring(1);
                            i1 = B.IndexOf("&");
                            atte_rec[2] = B.Substring(0, i1);
                            //convert event id into int
                            int eID = Convert.ToInt32(atte_rec[0]);
                            bool exist = false;
                            bool exist1 = false;
                            bool exist2 = false;
                            bool exist3 = false;

                            exist1 = eventsarray[eID].existGL(atte_rec[1]);
                            exist2 = eventsarray[eID].existNGL(atte_rec[1]);
                            exist3 = eventsarray[eID].existNRL(atte_rec[1]);
                            exist = exist1 || exist2 || exist3;


                            if (exist == true)
                            {
                                byte[] buffer = new byte[64];
                                buffer = Encoding.Default.GetBytes(newmessage);
                                foreach (client c in clientarray)
                                {
                                    c.getsocket().Send(buffer);
                                }
                                //remove that username from all instance of that event
                                eventsarray[eID].removeGoingList(atte_rec[1]);
                                eventsarray[eID].removeNotGoingList(atte_rec[1]);
                                eventsarray[eID].removeNotReplyList(atte_rec[1]);
                                //decide where the username should be store according to event and answer
                                if (atte_rec[2] == "1") //going
                                {
                                    eventsarray[eID].addGoingList(atte_rec[1]);
                                }
                                else if (atte_rec[2] == "0") //not going
                                {
                                    eventsarray[eID].addNotGoingList(atte_rec[1]);
                                }
                                else if (atte_rec[2] == "-1") //not reply
                                {
                                    eventsarray[eID].addNotReplyList(atte_rec[1]);
                                }
                                else
                                {
                                    MessageBox.Show("Something's wrong!");
                                }
                                //send notificatino back to organizer
                                //Someone just responded to your event!
                                string replyTo = eventsarray[eID].getOrganizer();
                                byte[] buffer20 = new byte[64];
                                buffer20 = Encoding.Default.GetBytes("#Someone just responded to your event!  ");
                                Socket iney = searchClient(replyTo);
                                iney.Send(buffer20);
                            }
                            else
                            {
                                byte[] buffer356 = new byte[64];
                                buffer356 = Encoding.Default.GetBytes("#You are not invited  ");
                                yeni.Send(buffer356);
                            }
                        }
                        else if (check_symbol(ref newmessage) == 4) // event request(symbol: $)
                        {
                            //MessageBox.Show("[Remove this before sumbit] Sending Event list: " + clientarray.Count);
                            for (int i = 0; i < eventsarray.Count; i++)
                            {
                                //Recieved a request of event lists, so server will send them
                                //"%" + date + "%" + title + "%" + place + "%" + description + "%" + organizer + "%";
                                string sendThis = "%" + eventsarray[i].getDate() + "%" + eventsarray[i].getTitle() + "%" + eventsarray[i].getPlace() + "%" + eventsarray[i].getDesc() + "%" + eventsarray[i].getOrganizer() + "%";
                                byte[] buffer = new byte[64];
                                buffer = Encoding.Default.GetBytes(sendThis);
                                yeni.Send(buffer);
                                //we forgot to send goingList, notGoingList and notReplyList
                                //use 3 more for loop
                                int glc = eventsarray[i].getGoingListCount();
                                int nglc = eventsarray[i].getNotGoingListCount();
                                int nrlc = eventsarray[i].getNotReplyListCount();
                                //encode each of them like newly added attendance reply
                                for (int j = 0; j<glc; j++)
                                {
                                    sendThis = "&" + i + "&" + eventsarray[i].getGoingList(j) + "&1&";
                                    buffer = Encoding.Default.GetBytes(sendThis);
                                    yeni.Send(buffer);
                                }
                                for (int j = 0; j<nglc; j++)
                                {
                                    sendThis = "&" + i + "&" + eventsarray[i].getNotGoingList(j) + "&0&";
                                    buffer = Encoding.Default.GetBytes(sendThis);
                                    yeni.Send(buffer);
                                }
                                for (int j = 0; j<nrlc; j++)
                                {
                                    sendThis = "&" + i + "&" + eventsarray[i].getNotReplyList(j) + "&-1&";
                                    buffer = Encoding.Default.GetBytes(sendThis);
                                    yeni.Send(buffer);
                                }
                                //"&"+{event id}"&"{username}&{yes or no}"&"
                            }
                            //MessageBox.Show("[Remove this before sumbit] Sending complete client list: " + clientarray.Count);
                            for (int i = 0; i < clientarray.Count; i++)
                            {
                                string event_request = "^" + clientarray[i].getname()+ "^";
                                //MessageBox.Show(event_request);
                                byte[] buffer = new byte[64];
                                buffer = Encoding.Default.GetBytes(event_request);
                                yeni.Send(buffer);
                            }
                            //friends list and request list should be empty at first
                        }
                        else if (check_symbol(ref newmessage) == 5) // add friends(symbol: @)
                        {
                            //decode
                            int i1 = 0;
                            int i2 = 0;
                            string A;
                            string B = newmessage;
                            string[] addfri = new string[2];
                            i1 = B.IndexOf("@");
                            A = B.Substring(i1 + 1);
                            i2 = A.IndexOf("@");
                            addfri[0] = B.Substring(1, i2);
                            B = B.Substring(i2 + 2);
                            i1 = B.IndexOf("@");
                            addfri[1] = B.Substring(0, i1);
                            //go to c2 (addfri[1])
                            int c2ID = searchClientID(addfri[1]);
                            int FLCount = getFriendsListCount(c2ID);

                            //add to request list
                            //send message to that client immediately
                            //send message to that client
                            byte[] buffer64 = new byte[64];
                            buffer64 = Encoding.Default.GetBytes("#Someone just sent you a friend request!  ");
                            Socket iney2 = searchClient(addfri[0]);
                            add_friends_2ways(addfri[0], addfri[1]);
                            iney2.Send(buffer64);
                            iney2.Send(buffer2);
                            }
                        else if (check_symbol(ref newmessage) == 7) // update all client list
                        {
                            byte[] buffer = new byte[64];
                            foreach (client c in clientarray)
                            {
                                c.getsocket().Send(buffer);
                            }
                        }
                        else
                        {
                            Time = DateTime.Now;
                            richTextBox1.Text = richTextBox1.Text + "-> " + clientarray[pos].getname() + " has disconnected from the server at " + Time + ".\r\n ";
                            newmessage = clientarray[pos].getname() + " has left the conversation.";
                            sendmessage = Encoding.Default.GetBytes(newmessage);
                            clientarray[pos].getsocket().Close();
                            //somethings wrong here?
                            clientarray.RemoveAt(pos);
                            foreach (client c in clientarray)
                            {
                                c.getsocket().Send(sendmessage);
                            }
                            condition = false;
                        }
                    }
                    Thread.CurrentThread.Abort();
                }
                else
                {
                    string serveranswer = "0";
                    byte[] send = Encoding.Default.GetBytes(serveranswer);
                    yeni.Send(send);
                    yeni.Close();
                }
            }
            catch
            {

            }
        }

        //return what the first symbol is/what is the purpose of message
        int check_symbol(ref string message)
        {
            if (message.ElementAt(0) == '%') // event
            {
                message = message.Substring(0, message.Length - 2);
                return 1;
            }
            else if (message.ElementAt(0) == '#') //message
            {
                message = message.Substring(0, message.Length - 2);
                return 2;
            }
            else if (message.ElementAt(0) == '&') //attendance
            {
                message = message.Substring(0, message.Length - 2);
                return 3;
            }
            else if (message.ElementAt(0) == '$') // request
            {
                return 4;
            }
            else if (message.ElementAt(0) == '@') //add friend
            {
                return 5;
            }
            else if (message.ElementAt(0) == '^') // get usernames from server (reply from server)
            {
                return 6;
            }
            else if (message.ElementAt(0) == '§') // add new client to all client list
            {
                return 7;
            }
            return 0;
        }

        public bool friendsOrNot(string client1, string client2)
        {
            //most sending need to check if this one is true first
            return true;
        }

        private bool checkname(string name)
        {
            bool notfound = true;
            for (int f = 0; f < clientarray.Count; f++)
            {
                if (clientarray[f].getname() == name)
                {
                    notfound = false;
                    return notfound;
                }
            }
            return notfound;
        }
        // When a user tries to shut down the server, we implemented a safety mechanism. It inform the user how many clinets are connected and if he/she
        // really wants to shut down the server. When the server shuts down all the clients also shut down.
        private void Fom1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int clientnumber = clientarray.Count;
            if (clientnumber > 0)
            {
                DialogResult DR = MessageBox.Show("There are " + clientnumber + " client(s) connected to server.\n Are you sure ?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (DR == DialogResult.Yes)
                {

                    System.Environment.Exit(1);
                }
                else
                {

                    e.Cancel = true;
                }
            }
            else if (clientnumber == 0)
            {
                DialogResult DR = MessageBox.Show("Are you sure ?", "Server ShutDown", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DR == DialogResult.Yes)
                {
                    System.Environment.Exit(1);
                }
                else
                    e.Cancel = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.AcceptButton = start;
        }
    }
}
