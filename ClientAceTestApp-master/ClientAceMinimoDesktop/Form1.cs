using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kepware.ClientAce.OpcDaClient;
using MySql.Data.MySqlClient;

namespace ClientAceMinimoDesktop
{
    public partial class Form1 : Form
    {
        DaServerMgt daServerMgt = new Kepware.ClientAce.OpcDaClient.DaServerMgt();
        ConnectInfo connectInfo = new Kepware.ClientAce.OpcDaClient.ConnectInfo();
        static string connString = "Data Source=localhost;Database=mydb;userid=root;Password=4dmin95";
        MySqlConnection conn = new MySqlConnection(connString);


        public Form1()
        {
            InitializeComponent();

            // Event handler
            daServerMgt.DataChanged += DaServerMgt_DataChanged;
        }

        private void DaServerMgt_DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            try
            {
                foreach (ItemValueCallback itemValue in itemValues)
                {
                    if (itemValue.ResultID.Succeeded)
                    {
                        realtime.Text += itemValue.TimeStamp + ": " + itemValue.ClientHandle + " - " + itemValue.Value + Environment.NewLine;
                        realtime.SelectionStart = realtime.TextLength;
                        realtime.ScrollToCaret();

                        if (itemValue.ClientHandle.ToString() == "PlantStatus")
                        {
                            conn.Open();
                            string insert = "INSERT INTO plantStatus(status, date) VALUES(" + itemValue.Value + ", \"" + itemValue.TimeStamp.ToString("yyyy-MM-dd hh:mm:ss") + "\")";
                            MySqlCommand comm = new MySqlCommand(insert, conn);
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }

                        if (itemValue.ClientHandle.ToString() == "PiecesProducted")
                        {
                            conn.Open();
                            string insert = "INSERT INTO piecesProducted(quantity, date) VALUES(" + itemValue.Value + ", \"" + itemValue.TimeStamp.ToString("yyyy-MM-dd hh:mm:ss") + "\")";
                            MySqlCommand comm = new MySqlCommand(insert, conn);
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }

                        if (itemValue.ClientHandle.ToString() == "PiecesDischarged")
                        {
                            conn.Open();
                            string insert = "INSERT INTO piecesDischarged(quantity, date) VALUES(" + itemValue.Value + ", \"" + itemValue.TimeStamp.ToString("yyyy-MM-dd hh:mm:ss") + "\")";
                            MySqlCommand comm = new MySqlCommand(insert, conn);
                            comm.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    else
                    {
                        debugBox.Text += "Errore";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataChanged exception. Reason: {0}", ex);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectInfo.LocalId = "en";
            connectInfo.KeepAliveTime = 5000;
            connectInfo.RetryAfterConnectionError = true;
            connectInfo.RetryInitialConnection = true;
            connectInfo.ClientName = "Demo ClientAceC-Sharp DesktopApplication";
            bool connectFailed;
            string url = "opcda://127.0.0.1/Kepware.KEPServerEX.V6/{7BC0CC8E-482C-47CA-ABDC-0FE7F9C6E729}";
            int clientHandle = 1;

            // Connessione al server
            try
            {
                daServerMgt.Connect(url, clientHandle, ref connectInfo, out connectFailed);
                if (connectFailed)
                {
                    debugBox.Text = "Connect failed" + Environment.NewLine;
                }
                else
                {
                    debugBox.Text = "Connected to Server " + url + " Succeeded." + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                debugBox.Text = ex.ToString();
            }

            AggiornaDati();

            // Tag a cui mi voglio sottoscrivere
            ItemIdentifier[] items = new ItemIdentifier[4];
            items[0] = new ItemIdentifier
            {
                ItemName = "its-iot-device.Device1.PlantStatus",
                ClientHandle = "PlantStatus"
            };
            items[1] = new ItemIdentifier
            {
                ItemName = "Simulation Examples.Functions.Ramp1",
                ClientHandle = "Ramp1"
            };
            items[2] = new ItemIdentifier
            {
                ItemName = "its-iot-device.Device1.PiecesProducted",
                ClientHandle = "PiecesProducted"
            };
            items[3] = new ItemIdentifier
            {
                ItemName = "its-iot-device.Device1.PiecesDischarged",
                ClientHandle = "PiecesDischarged"
            };

            int serverSubscription;
            ReturnCode returnCode;

            // Parametri di sottoscrizione agli eventi
            int clientSubscription = 1;
            bool active = true;
            int updateRate = 1000;
            int revisedUpdateRate;
            float deadband = 0;

            try
            {
                // Sottoscrizione agli eventi change data
                returnCode = daServerMgt.Subscribe(clientSubscription,
                active,
                updateRate,
                out revisedUpdateRate,
                deadband,
                ref items,
                out serverSubscription);
            }
            catch (Exception ex)
            {
                debugBox.Text = ex.ToString();
            }

        }

        private void AggiornaDati()
        {
            // Aggiorno a mano i valori di due tag

            int maxAge = 0;
            Kepware.ClientAce.OpcDaClient.ItemIdentifier[] OPCItems = new Kepware.ClientAce.OpcDaClient.ItemIdentifier[4];
            Kepware.ClientAce.OpcDaClient.ItemValue[] OPCItemValues = null;

            OPCItems[0] = new Kepware.ClientAce.OpcDaClient.ItemIdentifier();
            OPCItems[0].ItemName = "its-iot-device.Device1.PlantStatus";
            OPCItems[0].ClientHandle = 1;

            OPCItems[1] = new Kepware.ClientAce.OpcDaClient.ItemIdentifier();
            OPCItems[1].ItemName = "Simulation Examples.Functions.Ramp1";
            OPCItems[1].ClientHandle = 2;

            OPCItems[2] = new Kepware.ClientAce.OpcDaClient.ItemIdentifier();
            OPCItems[2].ItemName = "its-iot-device.Device1.PiecesProducted";
            OPCItems[2].ClientHandle = 3;

            OPCItems[3] = new Kepware.ClientAce.OpcDaClient.ItemIdentifier();
            OPCItems[3].ItemName = "its-iot-device.Device1.PiecesDischarged";
            OPCItems[3].ClientHandle = 4;

            label3.Text = OPCItems[0].ItemName;
            label6.Text = OPCItems[1].ItemName;
            label6.Text = OPCItems[2].ItemName;
            label6.Text = OPCItems[3].ItemName;

            try
            {
                daServerMgt.Read(maxAge, ref OPCItems, out OPCItemValues);

                if (OPCItems[0].ResultID.Succeeded & OPCItemValues[0].Quality.IsGood)
                {
                    label4.Text = OPCItemValues[0].Value.ToString();
                }
                else
                {
                    debugBox.Text += OPCItems[0].ResultID.Description;
                }

                if (OPCItems[1].ResultID.Succeeded & OPCItemValues[1].Quality.IsGood)
                {
                    label5.Text = OPCItemValues[1].Value.ToString();
                }
                else
                {
                    debugBox.Text += OPCItems[1].ResultID.Description;
                }
                if (OPCItems[2].ResultID.Succeeded & OPCItemValues[2].Quality.IsGood)
                {
                    label5.Text = OPCItemValues[2].Value.ToString();
                }
                else
                {
                    debugBox.Text += OPCItems[2].ResultID.Description;
                }
                if (OPCItems[3].ResultID.Succeeded & OPCItemValues[3].Quality.IsGood)
                {
                    label5.Text = OPCItemValues[3].Value.ToString();
                }
                else
                {
                    debugBox.Text += OPCItems[3].ResultID.Description;
                }
            }
            catch (Exception ex)
            {
                debugBox.Text += ex.ToString();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            AggiornaDati();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            conn.Close();
        }
    }
}
