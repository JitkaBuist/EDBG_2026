using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Energie.Car;
using System.Configuration;
using System.Data.SqlClient;

namespace EDSNBatch
{
    public partial class EDSNBatch : Form
    {
        private CarShared carShared;
        public EDSNBatch()
        {
            InitializeComponent();
            timer1.Enabled = true;

            carShared = new CarShared();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Interval = 14400000;

            string ConnString = System.Configuration.ConfigurationManager.ConnectionStrings["KlantConfig"].ConnectionString;
            SqlConnection connKC = new SqlConnection(ConnString);
            connKC.Open();

            
            string strSql = "select * from klantconfig where actief=1";
            SqlCommand cmd = new SqlCommand(strSql, connKC);
            DataTable dtKlantConfig = new DataTable();
            SqlDataAdapter daKlantConfig = new SqlDataAdapter(cmd);
            daKlantConfig.Fill(dtKlantConfig);

            foreach (DataRow drKlantConfig in dtKlantConfig.Rows)
            {
                SqlConnection conn = new SqlConnection(drKlantConfig["ConnString"].ToString());
                conn.Open();

                try
                {
                    strSql = "select PV, LV from Messages.dbo.Instellingen";
                    cmd = new SqlCommand(strSql, conn);
                    DataTable dtInstellingen = new DataTable();
                    SqlDataAdapter daInstellingen = new SqlDataAdapter(cmd);
                    daInstellingen.Fill(dtInstellingen);
                    Boolean blnPV = (Boolean)dtInstellingen.Rows[0]["PV"];
                    Boolean blnLV = (Boolean)dtInstellingen.Rows[0]["LV"];

                    Energie.Car.SwitchClient switchClient = new Energie.Car.SwitchClient(drKlantConfig["Klantconfig"].ToString());
                    Energie.Car.MasterdataClient masterdataClient = new Energie.Car.MasterdataClient(drKlantConfig["Klantconfig"].ToString());
                    Energie.Car.VerbruikClient verbruikClient = new Energie.Car.VerbruikClient(drKlantConfig["Klantconfig"].ToString());

                    int nrRecords = 100;
                    if (blnLV)
                    {

                        while (nrRecords >= 90)
                        {
                            txtLog.Text += "\n" + "Losses";
                            switchClient.GetLoss(false, "", true, out nrRecords);
                        }
                        //nrRecords = 100;
                        //while (nrRecords == 100)
                        //{
                        //    txtLog.Text += "Gains";
                        //    //switchClient.GetGains(false, "", true, out nrRecords);
                        //}
                        nrRecords = 100;
                        String strErrorTekst = "";
                        while (nrRecords >= 90)
                        {
                            txtLog.Text += "\n" + "Masterdata";
                            strErrorTekst = masterdataClient.RequestMasterdataUpdate(false, true, out nrRecords);
                            if (strErrorTekst != "")
                            {
                                txtLog.Text += "\n" + strErrorTekst;
                            }
                        }
                        nrRecords = 100;
                        while (nrRecords >= 90)
                        {
                            txtLog.Text += "\n" + "Meterstanden";
                            verbruikClient.RequestMeterReading(false, "", true, out nrRecords, out strErrorTekst);
                            if (strErrorTekst != "")
                            {
                                txtLog.Text += "\n" + strErrorTekst;
                            }
                        }

                        if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour <= 24)
                        {
                            strSql = "SELECT EAN13_Code FROM EnergieDB.dbo.Netbeheerders";
                            cmd = new SqlCommand(strSql, conn);
                            DataTable dtNetbeheerders = new DataTable();
                            SqlDataAdapter daNetbeheerders = new SqlDataAdapter(cmd);
                            daNetbeheerders.Fill(dtNetbeheerders);

                            foreach (DataRow drNetbeheerders in dtNetbeheerders.Rows)
                            {
                                strErrorTekst = verbruikClient.OphalenRejections("", drNetbeheerders["EAN13_Code"].ToString(), true);
                                if (strErrorTekst != "")
                                {
                                    txtLog.Text += "\n" + strErrorTekst;
                                }
                                strErrorTekst = verbruikClient.OphalenDispuut("", drNetbeheerders["EAN13_Code"].ToString(), true);
                                if (strErrorTekst != "")
                                {
                                    txtLog.Text += "\n" + strErrorTekst;
                                }
                            }

                            strSql = "SELECT MeterstandId FROM Car.dbo.MeterStanden_Header where verstuurd = 0 and beginD<getdate()";
                            cmd = new SqlCommand(strSql, conn);
                            DataTable dtMeterstanden = new DataTable();
                            SqlDataAdapter daMeterstanden = new SqlDataAdapter(cmd);
                            daMeterstanden.Fill(dtMeterstanden);

                            foreach (DataRow drMeterstanden in dtMeterstanden.Rows)
                            {
                                VastGesteldeStand vastGesteldeStand = null;
                                verbruikClient.VastGesteldeStand("", vastGesteldeStand, true, (int)drMeterstanden["MeterstandId"], out strErrorTekst);
                                if (strErrorTekst != "")
                                {
                                    txtLog.Text += "\n" + strErrorTekst;
                                }
                            }

                            switchClient.GetReject(false, true);
                        }
                    }

                    if (blnPV)
                    {
                        nrRecords = 100;
                        while (nrRecords >= 90)
                        {
                            switchClient.GetLoss(true, "", true, out nrRecords);
                        }
                        nrRecords = 100;
                        while (nrRecords >= 90)
                        {
                            switchClient.GetGains(true, "", true, out nrRecords);
                        }
                        nrRecords = 100;
                        while (nrRecords >= 90)
                        {
                            masterdataClient.RequestMasterdataGain(true, true, out nrRecords);
                        }
                        nrRecords = 100;
                        while (nrRecords >= 90)
                        {
                            masterdataClient.RequestMasterdataUpdate(true, true, out nrRecords);
                        }
                    }

                }
                catch(Exception ex)
                {
                    txtLog.Text += "Error in tick: " + ex.Message;
                    carShared.SchrijfLog("Error in tick: " + ex.Message, 10, -1, KC.App_ID);
                }
                conn.Close();
            }

            connKC.Close();

            
            timer1.Enabled = true;
        }
    }
}
