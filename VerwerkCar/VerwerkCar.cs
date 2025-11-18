using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Net.Mail;

namespace nl.Energie.VerwerkCar
{
    public partial class VerwerkCar
    {

        //Hallo Aanpassing
        //public const string connString_Norm = "context connection=true";
        //public string strMessage_Name;

        public const int LEVEL_Info = 1;
        public const int LEVEL_Warning = 3;
        public const int LEVEL_WarningICT = 5;
        public const int LEVEL_Critical = 7;
        public const int LEVEL_Alert = 10;
        //public int BerichtId;

        private Bericht bericht = new Bericht();
        private Masterdata masterdata = new Masterdata();
        private HoofdKenmerk hoofdKenmerk = new HoofdKenmerk();
        private Meter meter = new Meter();
        private FysiekeKenmerk fysiekeKenmerk = new FysiekeKenmerk();
        private Register register = new Register();
        //private Relatie relatie = new Relatie();
        private MasterdataDossier masterdataDossier = new MasterdataDossier();

        public VerwerkCar(int pBerichtId, out bool succes, out string strError, string ConnString)
        {
            String HoofdPV = "";
            String HoofdLV = "";
            //string CertPV;
            //string CertPVPassword;
            //string CertLV;
            //string CertLVPassword;
            //string XMLPath;
            //string FTPUser;
            //string FTPPassword;
            //string FTPServer;
            //int KlantIdElk;
            //int KlantIdGas;
            //int App_ID;

            int BerichtId = pBerichtId;
            //SqlPipe pipe = SqlContext.Pipe;
            //string strMessage_Name = "";
            strError = "";
            succes = true;
            SqlConnection conn = null;
            try
            {


                conn = new SqlConnection(ConnString);
                conn.Open();



            }
            catch (Exception ex)
            {
                strError = "Fout bij openen Connectie" + ex.Message;
                succes = false;
            }

            try
            {


                string str_SQL = "SELECT KlantConfig FROM Messages.dbo.Instellingen";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = str_SQL;
                String strKlantConfig = cmd.ExecuteScalar().ToString();

                //string strSql = "SELECT ConnString,HoofdPV,HoofdLV,CertPV,CertPVPassword,CertLV,CertLVPassword,XMLPath,FTPUser \n";
                //strSql += ", FTPPassword, FTPServer, KlantIdElk, KlantIdGas FROM KlantConfig.dbo.KlantConfig WHERE KlantConfig = @KlantConfig \n";
                //cmd = new SqlCommand(strSql, conn);
                //cmd.Parameters.AddWithValue("@KlantConfig", strKlantConfig);
                //SqlDataReader rdr = cmd.ExecuteReader();
                //while (rdr.Read())
                //{
                //    HoofdPV = rdr.GetInt64(1);
                //    HoofdLV = rdr.GetInt64(2);
                //    CertPV = rdr.GetString(3);
                //    CertPVPassword = rdr.GetString(4);
                //    CertLV = rdr.GetString(5);
                //    CertLVPassword = rdr.GetString(6);
                //    XMLPath = rdr.GetString(7);
                //    FTPUser = rdr.GetString(8);
                //    FTPPassword = rdr.GetString(9);
                //    FTPServer = rdr.GetString(10);
                //    KlantIdElk = rdr.GetInt32(11);
                //    KlantIdGas = rdr.GetInt32(12);
                //}


                str_SQL = "SELECT CAST(HoofdPV as varchar) , CAST(HoofdLV as varchar) FROM EnergieDB.dbo.KlantConfig WHERE KlantConfig=@KlantConfig";
                cmd = conn.CreateCommand();
                cmd.CommandText = str_SQL;
                cmd.Parameters.AddWithValue("@KlantConfig", strKlantConfig);
                SqlDataReader rdrInstellingen = cmd.ExecuteReader();

                try
                {
                    while (rdrInstellingen.Read())
                    {
                        HoofdPV = rdrInstellingen.GetString(0);
                        HoofdLV = rdrInstellingen.GetString(1);
                    }

                    rdrInstellingen.Close();
                }
                catch (Exception ex)
                {
                    rdrInstellingen.Close();
                    succes = false;
                    strError = "Fout bij inlezen Klantconfig (rdr) :" + ex.Message;

                    WriteLog(strError, LEVEL_WarningICT, ref conn, BerichtId);
                }

            }
            catch (Exception ex)
            {
                succes = false;
                strError = "Fout bij inlezen instellingen :" + ex.Message;

                WriteLog(strError, LEVEL_WarningICT, ref conn, BerichtId);
            }


            try
            {
                //Lees Edine
                //SendMail("converter@edmij.nl", "pieter.mink@edmij.nl", "Status 6 Delfor", @"Status 6 delfor op tralala leveringsdag jetzt <br>PV : Ons" , false, "EDMIJ-SVR001", 25, ref succes, ref strError, ref conn, inbox_ID);
                if (succes == true)
                {
                    //strError = "Succes";
                    string str_SQL = "SELECT Inkomend,Datum,Onderwerp,Bericht,Type,BerichtID_Sender,Verwerkt,Fout FROM Car.dbo.Berichten WHERE Bericht_ID=@Bericht_ID";
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = str_SQL;
                    cmd.Parameters.AddWithValue("@Bericht_ID", BerichtId);


                    SqlDataReader rdr = cmd.ExecuteReader();
                    try
                    {

                        while (rdr.Read())
                        {
                            bericht.Bericht_ID = BerichtId;
                            bericht.Inkomend = rdr.GetBoolean(0);
                            bericht.Datum = rdr.GetDateTime(1);
                            bericht.Onderwerp = rdr.GetString(2);
                            bericht.strBericht = rdr.GetString(3);
                            bericht.Type = rdr.GetInt32(4);
                            bericht.BerichtID_Sender = rdr.GetString(5);
                            bericht.Verwerkt = rdr.GetBoolean(6);
                            bericht.Fout = rdr.GetBoolean(7);
                            succes = true;
                        }
                        rdr.Close();
                    }

                    catch (Exception ex)
                    {
                        rdr.Close();
                        succes = false;
                        strError = "Fout bij lezen bericht: " + BerichtId.ToString() + " " + ex.Message;

                        WriteLog(strError, LEVEL_WarningICT, ref conn, BerichtId);
                    }
                    if (succes == true)
                    {

                        switch (bericht.Type)
                        {
                            case 1:
                                VerwerkGain(ref conn, HoofdPV, BerichtId);
                                break;
                            case 2:
                                VerwerkGain(ref conn, HoofdPV, BerichtId);
                                break;
                            case 6:
                                VerwerkMasterdata(ref conn, BerichtId);
                                break;
                            case 7:
                                VerwerkLoss(ref conn, BerichtId, HoofdPV);
                                break;
                        }
                    }
                    else
                    {
                        if (strError == "")
                        {
                            strError = "Fout bij lezen edine";
                            succes = false;
                            WriteLog(strError, LEVEL_WarningICT, ref conn, BerichtId);
                        }

                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                strError = "Onbekende fout in ProcessMessages : " + ex.Message;
                WriteLog(strError, LEVEL_WarningICT, ref conn, BerichtId);
                succes = false;
            }

        }

        public void VerwerkMasterdata(ref SqlConnection conn, int BerichtId)
        {
            DataTable dtVorigeMasterdata = null;
            SqlDataAdapter daVorigeMasterdata = null;
            SqlCommandBuilder cbVorigeMasterdata = null;

            string strSql = "select Masterdata.*, Bemeteringtypes.id as BemeteringsTypeId, FysiekeStatus.FysiekeStatusId as FysiekeStatusId, Capaciteitscodes.Id CapaciteitsId \n";
            strSql += ",Cast(CASE WHEN Masterdata.AdministratieveStatusMeter='AAN'  then 1 else 0 end as bit) as bitAdministratieveStatusSlimmeMeter \n";
            strSql += ",Cast(CASE WHEN Masterdata.LeveringsStatus='ACT'  THEN 1 else 0 end as bit) as bitLeveringsStatus \n";
            strSql += ",LeveringsRichting.Id as LeveringsRichtingId, AllocatieMethode.AllocatieMethodeId as AllocatieMethodeId \n";
            strSql += ",Cast(CASE WHEN Masterdata.TypeMeter='SLM' THEN 1 else 0 END as bit) as SlimmeMeter\n";
            strSql += ",Cast(CASE WHEN Masterdata.UitleesbaarheidSlimmeMeter='SMU' then 1 else 0 end as bit) as bitUitleesbaarheidSlimmeMeter \n";
            strSql += ",Cast(CASE WHEN Masterdata.TemperatuurCorrectie='1' then 1 else 0 end as bit) as bitTemperatuurCorrectie \n";
            strSql += ",Case when ISNULL(Gain.Gain_ID, 0)>0 then Cast(1 as bit)  else cast(0 as bit)  end as  LV_Actief \n";
            strSql += "from Car.dbo.Masterdata join EnergieDB.dbo.Bemeteringtypes on Masterdata.WijzeVanBemetering=Bemeteringtypes.CarCode \n";
            strSql += "left join EnergieDB.dbo.Capaciteitscodes on Capaciteitscodes.EAN13_Code=Masterdata.Capaciteitstariefcode \n";
            strSql += "left join EnergieDB.code.FysiekeStatus on FysiekeStatus.CarId=Masterdata.FysiekeStatus \n";
            strSql += "left join EnergieDB.code.LeveringsRichting on LeveringsRichting.CarId=Masterdata.LeveringsRichting \n";
            strSql += "left join EnergieDB.code.AllocatieMethode on AllocatieMethode.CARId=Masterdata.AllocatieMethode \n";
            strSql += "left join Car.dbo.Gain on Gain.EAN18_Code=Masterdata.EAN18_Code and Gain.Datum=Masterdata.DatumMutatie \n";
            strSql += "where Masterdata.Bericht_ID=@Bericht_ID";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Bericht_ID", BerichtId);
            DataTable dtMasterdata = new DataTable();
            SqlDataAdapter daMasterData = new SqlDataAdapter(cmd);
            daMasterData.Fill(dtMasterdata);

            if (dtMasterdata.Rows.Count > 0)
            {
                foreach (DataRow drMasterdata in dtMasterdata.Rows)
                {
                    strSql = "SELECT Tewerk_ID,Meeteenheid,Meetrichting,AantalTelwielen,TariefZone,Factor, LeveringsRichting.Id as LeveringsRichtingId \n";
                    strSql += "FROM Car.dbo.MasterdataTelwerken \n";
                    strSql += "left join EnergieDb.dbo.LeveringsRichting on LeveringsRichting.CARCode = MasterdataTelwerken.Meetrichting \n";
                    strSql += "where Masterdata_ID = @Masterdata_ID";
                    cmd = new SqlCommand(strSql, conn);
                    cmd.Parameters.AddWithValue("@Masterdata_ID", drMasterdata["Masterdata_ID"]);
                    DataTable dtMasterdataTelwerken = new DataTable();
                    SqlDataAdapter daMasterdataTelwerken = new SqlDataAdapter(cmd);
                    daMasterdataTelwerken.Fill(dtMasterdataTelwerken);

                    strSql = "SELECT Datum, Dossier, Reden, Referentie FROM Car.dbo.MasterdataMutation where Masterdata_ID = @Masterdata_ID";
                    cmd = new SqlCommand(strSql, conn);
                    cmd.Parameters.AddWithValue("@Masterdata_ID", drMasterdata["Masterdata_ID"]);
                    DataTable dtMasterdataMutation = new DataTable();
                    SqlDataAdapter daMasterdataMutation = new SqlDataAdapter(cmd);
                    daMasterdataMutation.Fill(dtMasterdataMutation);

                    DataRow drVorigeMasterdata = null;
                    DataRow drHoofdKenmerk = null;
                    DataRow drFysiekeKenmerk = null;
                    DataRow drMeter = null;
                    DataRow drDossier = null;

                    int intAansluitingType = 1;
                    if (drMasterdata["ProductSoort"].ToString() == "GAS") { intAansluitingType = 5; }
                    DataRow drAansluiting = LeesAansluitingen(ref conn, (Int64)drMasterdata["EAN18_Code"], drMasterdata["LeverancierEAN"].ToString(), intAansluitingType);

                    drVorigeMasterdata = LeesVorigeMasterdata(ref conn, (Int64)drMasterdata["EAN18_Code"], (DateTime)drMasterdata["DatumMutatie"], ref dtVorigeMasterdata, ref daVorigeMasterdata, ref cbVorigeMasterdata);
                    Boolean blnMasterdataInsert = true;
                    if (drVorigeMasterdata != null)
                    {
                        if ((DateTime)drVorigeMasterdata["StartDatum"] == (DateTime)drMasterdata["DatumMutatie"]) { blnMasterdataInsert = false; }
                    }

                    DataRow drSwtich = LeesSwitch(ref conn, (int)drAansluiting["ID"], (DateTime)drMasterdata["DatumMutatie"]);

                    masterdata.EAN18Code = (Int64)drMasterdata["EAN18_Code"];
                    masterdata.StartDatum = (DateTime)drMasterdata["DatumMutatie"];
                    masterdata.AansluitingId = (int)drAansluiting["ID"];

                    VulHoofdKenmerk(ref conn, drMasterdata);
                    hoofdKenmerk.Update = false;
                    VulFysiekeKenmerk(ref conn, drMasterdata, BerichtId);
                    fysiekeKenmerk.Update = false;
                    VulMeter(ref conn, drMasterdata);
                    meter.Update = false;

                    if (drVorigeMasterdata == null)
                    {
                        masterdata.PVActief = false;
                        masterdata.LVActief = (Boolean)drMasterdata["LV_Actief"];
                        //Masterdata.AccountId = (int)drMasterdata["AccountId"];
                        hoofdKenmerk.Update = false;
                        fysiekeKenmerk.Update = false;
                        if (drSwtich != null)
                        {
                            masterdata.RelatieId = (int)drSwtich["Relatie_ID"];
                            masterdata.PortfolioId = (int)drSwtich["Portfolio_ID"];
                        }
                        else
                        {
                            masterdata.RelatieId = LeesAccount(ref conn, (Int64)drMasterdata["LeverancierEAN"]);
                            //Todo default portfolio?
                            masterdata.PortfolioId = -1;
                        }
                    }
                    else
                    {
                        //masterdata.AdresId = (int)drVorigeMasterdata["AdresId"];
                        masterdata.PVActief = (Boolean)drVorigeMasterdata["PVActief"];
                        masterdata.LVActief = (Boolean)drVorigeMasterdata["LVActief"];
                        masterdata.RelatieId = (int)drVorigeMasterdata["RelatieId"];
                        masterdata.PortfolioId = (int)drVorigeMasterdata["PortfolioId"];
                        drHoofdKenmerk = LeesVorigeHoofdKenmerk(ref conn, (int)drVorigeMasterdata["HoofdKenmerkId"]);
                        if (drHoofdKenmerk != null)
                        {
                            if ((int)drHoofdKenmerk["NetbeheerderId"] != hoofdKenmerk.NetbeheerderId ||
                                (int)drHoofdKenmerk["NetgebiedId"] != hoofdKenmerk.NetgebiedId ||
                                (int)drHoofdKenmerk["PV"] != hoofdKenmerk.PV ||
                                (int)drHoofdKenmerk["LV"] != hoofdKenmerk.LV ||
                                //(Int64)drHoofdKenmerk["MV"] != hoofdKenmerk.MV ||
                                (Int32)drHoofdKenmerk["Product"] != hoofdKenmerk.Product ||
                                (byte)drHoofdKenmerk["FactuurMaand"] != hoofdKenmerk.FactuurMaand ||
                                (int)drHoofdKenmerk["ContractedCapacity"] != hoofdKenmerk.ContractedCapacity ||
                                (string)drHoofdKenmerk["MaxConsumption"] != hoofdKenmerk.MaxConsumption ||
                                (Boolean)drHoofdKenmerk["Residential"] != hoofdKenmerk.Residential ||
                                (Boolean)drHoofdKenmerk["Complex"] != hoofdKenmerk.Complex ||
                                drHoofdKenmerk["MarketSegment"].ToString() != hoofdKenmerk.MarketSegment ||
                                (String)drHoofdKenmerk["Straat"] != hoofdKenmerk.Straat ||
                                (String)drHoofdKenmerk["Huisnummer"] != hoofdKenmerk.Huisnummer ||
                                (String)drHoofdKenmerk["Toevoeging"] != hoofdKenmerk.Toevoeging ||
                                (String)drHoofdKenmerk["Postcode"] != hoofdKenmerk.Postcode ||
                                (String)drHoofdKenmerk["Plaats"] != hoofdKenmerk.Plaats)
                            {
                                hoofdKenmerk.Update = true;
                            }
                        }
                        else
                        {
                            hoofdKenmerk.Update = false;
                        }

                        //drFysiekeKenmerk = LeesVori
                    }


                    if (hoofdKenmerk.Update)
                    {

                        UpdateHoofdKenmerk(ref conn, BerichtId);
                    }
                    else
                    {
                        AanmakenHoofdKenmerk(ref conn, BerichtId);
                    }
                    masterdata.HoofdKenmerkId = hoofdKenmerk.HoofdKenmerkId;


                    if (drVorigeMasterdata != null)
                    {
                        drFysiekeKenmerk = LeesVorigeFysiekeKenmerk(ref conn, (int)drVorigeMasterdata["FysiekeKenmerkID"]);
                    }

                    if (drFysiekeKenmerk != null)
                    {
                        Int16 inttest = drFysiekeKenmerk.IsNull("CapTarCode") == true ? (Int16)0 : (Int16)drFysiekeKenmerk["CapTarCode"];

                        if ((byte)drFysiekeKenmerk["BemeteringsType"] != fysiekeKenmerk.BemeteringsType ||
                                (String)drFysiekeKenmerk["Profiel"] != fysiekeKenmerk.Profiel ||
                                inttest != fysiekeKenmerk.CapTarCode ||
                                (Int32)drFysiekeKenmerk["FysiekeStatusId"] != fysiekeKenmerk.FysiekeKenmerkID ||
                                (Boolean)drFysiekeKenmerk["AdminStatusSmartMeter"] != fysiekeKenmerk.AdminStatusSmartMeter ||
                                (Boolean)drFysiekeKenmerk["LeveringsStatus"] != fysiekeKenmerk.LeveringsStatus ||
                                (Int32)drFysiekeKenmerk["LeveringsRichtingId"] != fysiekeKenmerk.LeveringsRichtingId ||
                                (Int64)drFysiekeKenmerk["SJVPeak"] != fysiekeKenmerk.SJVPeak ||
                                (Int64)drFysiekeKenmerk["SJVOffPeak"] != fysiekeKenmerk.SJVOffPeak ||
                                (Int64)drFysiekeKenmerk["SJIPeak"] != fysiekeKenmerk.SJIPeak ||
                                (Int64)drFysiekeKenmerk["SJIOffPeak"] != fysiekeKenmerk.SJIOffPeak)
                        {
                            fysiekeKenmerk.Update = true;
                        }
                    }
                    else
                    {
                        fysiekeKenmerk.Update = false;
                    }

                    if (fysiekeKenmerk.Update)
                    {
                        if (!blnMasterdataInsert)
                        {
                            fysiekeKenmerk.BusinessType = drFysiekeKenmerk.IsNull("BusinessType") ? -1 : (int)drFysiekeKenmerk["BusinessType"];
                            fysiekeKenmerk.PowerSystemType = drFysiekeKenmerk.IsNull("PowerSystemType") ? -1 : (int)drFysiekeKenmerk["PowerSystemType"];
                            UpdateFysiekeKenmerk(ref conn, BerichtId);
                        }
                        else
                        {
                            AanmakenFysiekeKenmerk(ref conn, BerichtId);
                        }
                        masterdata.FysiekeKenmerkID = fysiekeKenmerk.FysiekeKenmerkID;
                    }

                    if (drVorigeMasterdata != null)
                    {
                        drMeter = LeesVorigeMeter(ref conn, (int)drVorigeMasterdata["MeterId"]);
                    }

                    if (drMeter != null)
                    {
                        if ((String)drMeter["MeterNummer"] != meter.MeterNummer ||
                                (Boolean)drMeter["SlimmeMeter"] != meter.SlimmeMeter ||
                                (Boolean)drMeter["Uitleesbaar"] != meter.Uitleesbaar ||
                                (Boolean)drMeter["TempCorrectie"] != meter.TempCorrectie ||
                                (Int32)drMeter["AantalRegisters"] != meter.AantalRegisters)
                        {
                            meter.Insert = true;
                        }
                    }
                    else
                    {
                        meter.Insert = true;
                    }

                    if (meter.Insert)
                    {

                        AanmakenMeter(ref conn, BerichtId);

                    }
                    masterdata.MeterID = meter.MeterId;

                    foreach (DataRow drTelwerk in dtMasterdataTelwerken.Rows)
                    {
                        VulRegister(ref conn, drTelwerk, meter.MeterId);

                        DataRow drRegister = LeesVorigeRegister(ref conn, meter.MeterId, drTelwerk["Tewerk_ID"].ToString());

                        if (drRegister != null)
                        {
                            if ((String)drRegister["TariefZone"] != register.TariefType ||
                                    (int)drRegister["LeveringsRichtingId"] != register.LeveringsRichting ||
                                    (int)drRegister["AantalTelwielen"] != register.AantalTelwielen ||
                                    (decimal)drRegister["Factor"] != register.Factor)
                            {
                                register.Insert = true;
                            }
                        }
                        else
                        {
                            register.Insert = true;
                        }

                        if (register.Insert)
                        {
                            AanmakenRegister(ref conn, BerichtId);
                        }
                    }



                    masterdata.VerwerktDT = DateTime.Now;
                    if (drVorigeMasterdata != null)
                    {
                        masterdata.EindDatum = (DateTime)drVorigeMasterdata["EindDatum"];

                        if ((DateTime)drVorigeMasterdata["StartDatum"] == masterdata.StartDatum)
                        {
                            UpdateMasterData(ref conn, BerichtId);
                        }
                        else
                        {
                            drVorigeMasterdata["EindDatum"] = masterdata.StartDatum;
                            daVorigeMasterdata.Update(dtVorigeMasterdata);
                            AanmakenMasterData(ref conn, BerichtId);
                        }

                    }
                    else
                    {
                        masterdata.EindDatum = new DateTime(2078, 12, 31);
                        AanmakenMasterData(ref conn, BerichtId);
                    }

                    //Dossier
                    foreach (DataRow drMasterMutation in dtMasterdataMutation.Rows)
                    {
                        VulDossier(drMasterMutation, drMasterdata);
                        if (drVorigeMasterdata != null)
                        {
                            drDossier = LeesVorigDossier(ref conn, (Int64)drMasterdata["EAN18_Code"], (DateTime)drMasterMutation["Datum"], drMasterMutation["Dossier"].ToString(), (String)drMasterMutation["Referentie"]);
                        }

                        if (drDossier != null)
                        {
                            if ((String)masterdataDossier.Referentie != (String)drDossier["Referentie"])
                            {
                                masterdataDossier.Update = true;
                            }
                        }
                        else
                        {
                            masterdataDossier.Insert = true;
                        }

                        if (masterdataDossier.Insert)
                        {

                            AanmakenDossier(ref conn, BerichtId);

                        }
                        if (masterdataDossier.Update)
                        {
                            UpdateDossier(ref conn, BerichtId);
                        }
                    }
                }
            }

        }

        public void VulHoofdKenmerk(ref SqlConnection conn, DataRow drMasterdata)
        {
            hoofdKenmerk.NetbeheerderId = LeesNetbeheerder(ref conn, (Int64)drMasterdata["NetbeheerderEAN"]);
            hoofdKenmerk.NetgebiedId = LeesNetgebied(ref conn, (Int64)drMasterdata["NetgebiedEAN"]);
            hoofdKenmerk.PV = LeesPV(ref conn, (Int64)drMasterdata["ProgrammaverantwoordelijkeEAN"]);
            hoofdKenmerk.LV = LeesLV(ref conn, (Int64)drMasterdata["LeverancierEAN"]);
            hoofdKenmerk.MV = LeesMV(ref conn, (Int64)drMasterdata["MeetverantwoordelijkeEAN"]);
            hoofdKenmerk.Product = 1;
            if (drMasterdata["ProductSoort"].ToString() == "GAS") { hoofdKenmerk.Product = 2; }
            Int16 intFactuurMaand = -1;
            Int16.TryParse(drMasterdata["Afrekenmaand"].ToString(), out intFactuurMaand);
            hoofdKenmerk.FactuurMaand = intFactuurMaand;
            int intContractedCapacity = -1;
            int.TryParse(drMasterdata["ContractCapaciteit"].ToString(), out intContractedCapacity);
            hoofdKenmerk.ContractedCapacity = intContractedCapacity;
            hoofdKenmerk.MaxConsumption = drMasterdata["Maxverbruik"].ToString();

            hoofdKenmerk.Residential = false;
            if (drMasterdata["VerbruikSegment"].ToString() == "J") { hoofdKenmerk.Residential = true; }
            hoofdKenmerk.Complex = false;
            if (drMasterdata["Complexbepaling"].ToString() == "J") { hoofdKenmerk.Complex = true; }

            hoofdKenmerk.MarketSegment = drMasterdata["VerbruikSegment"].ToString();
            hoofdKenmerk.Straat = (String)drMasterdata["Straatnaam"];
            hoofdKenmerk.Huisnummer = (String)drMasterdata["Huisnummer"];
            hoofdKenmerk.Toevoeging = (String)drMasterdata["HuisnummerToevoeging"];
            hoofdKenmerk.Postcode = (String)drMasterdata["Postcode"];
            hoofdKenmerk.Plaats = (String)drMasterdata["Woonplaats"];
        }

        public void VulFysiekeKenmerk(ref SqlConnection conn, DataRow drMasterdata, int BerichtId)
        {
            fysiekeKenmerk.BemeteringsType = (Byte)drMasterdata["BemeteringsTypeId"];
            fysiekeKenmerk.Profiel = drMasterdata["ProfielCategorie"].ToString().Substring(1);
            if (!drMasterdata.IsNull("CapaciteitsId"))
            {
                fysiekeKenmerk.CapTarCode = (Int16)drMasterdata["CapaciteitsId"];
            }
            else
            {
                if (!drMasterdata.IsNull("Capaciteitstariefcode"))
                {
                    fysiekeKenmerk.CapTarCode = AanmakenCapacieitsCode(ref conn, BerichtId, (Int64)drMasterdata["Capaciteitstariefcode"]);
                }
            }
            fysiekeKenmerk.FysiekeStatusId = (int)drMasterdata["FysiekeStatusId"];
            fysiekeKenmerk.AdminStatusSmartMeter = (Boolean)drMasterdata["bitAdministratieveStatusSlimmeMeter"];
            fysiekeKenmerk.LeveringsStatus = (Boolean)drMasterdata["bitLeveringsStatus"];
            fysiekeKenmerk.LeveringsRichtingId = (int)drMasterdata["LeveringsRichtingId"];
            if (!drMasterdata.IsNull("SJVNormaal"))
            {
                fysiekeKenmerk.SJVPeak = (int)drMasterdata["SJVNormaal"];
            }
            else
            {
                fysiekeKenmerk.SJVPeak = 0;
            }
            if (!drMasterdata.IsNull("SJVLaag"))
            {
                fysiekeKenmerk.SJVOffPeak = (int)drMasterdata["SJVLaag"];
            }
            else
            {
                fysiekeKenmerk.SJVOffPeak = 0;
            }

            if (!drMasterdata.IsNull("SJINormaal"))
            {
                fysiekeKenmerk.SJIPeak = (int)drMasterdata["SJINormaal"];
            }
            else
            {
                fysiekeKenmerk.SJIPeak = 0;
            }
            if (!drMasterdata.IsNull("SJILaag"))
            {
                fysiekeKenmerk.SJIOffPeak = (int)drMasterdata["SJILaag"];
            }
            else
            {
                fysiekeKenmerk.SJIOffPeak = 0;
            }

            fysiekeKenmerk.AllocatieMethode = (int)drMasterdata["AllocatieMethodeId"];
        }

        public void VulMeter(ref SqlConnection conn, DataRow drMasterdata)
        {
            meter.MeterNummer = (String)drMasterdata["Meternummer"];
            meter.SlimmeMeter = (Boolean)drMasterdata["SlimmeMeter"];
            meter.Uitleesbaar = (Boolean)drMasterdata["bitUitleesbaarheidSlimmeMeter"];
            meter.TempCorrectie = (Boolean)drMasterdata["bitTemperatuurCorrectie"];
            meter.AantalRegisters = (int)drMasterdata["AantalTelwerken"];
        }

        public void VulRegister(ref SqlConnection conn, DataRow drMasterdataTelwerken, int meterId)
        {
            register.MeterId = meterId;
            register.RegisterId = (string)drMasterdataTelwerken["Tewerk_ID"];
            register.TariefType = (string)drMasterdataTelwerken["TariefZone"];
            register.LeveringsRichting = (int)drMasterdataTelwerken["LeveringsRichtingId"];
            register.AantalTelwielen = (int)drMasterdataTelwerken["AantalTelwielen"];
            decimal decFactor = 0;
            decimal.TryParse(drMasterdataTelwerken["Factor"].ToString(), out decFactor);
            register.Factor = (decFactor);
        }
        public void VulDossier(DataRow drMasterdataMutation, DataRow drMasterdata)
        {

            masterdataDossier.EAN18Code = (Int64)drMasterdata["EAN18_Code"];
            masterdataDossier.StartDatum = (DateTime)drMasterdataMutation["Datum"];
            masterdataDossier.Dossier = (String)drMasterdataMutation["Dossier"];
            masterdataDossier.Referentie = (String)drMasterdataMutation["Referentie"];
            masterdataDossier.Reden = (String)drMasterdataMutation["Reden"];
        }

        public void VerwerkGain(ref SqlConnection conn, String HoofdPV, int BerichtId)
        {
            String strSql = "SELECT Gain_ID,Datum,Ontvanger,EAN18_Code,Product,NB,PV,LV,Dossier,Reden,Referentie,OudeLV FROM Car.dbo.Gain where Bericht_ID=@Bericht_ID";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Bericht_ID", BerichtId);
            DataTable dtGain = new DataTable();
            SqlDataAdapter daGain = new SqlDataAdapter(cmd);
            daGain.Fill(dtGain);

            if (dtGain.Rows.Count > 0)
            {
                DataRow drGain = dtGain.Rows[0];
                strSql = "SELECT * FROM EnergieDB.masterdata.Masterdata where EAN18Code=@EAN18Code and StartDatum<=@Datum and EindDatum>@Datum";
                cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@EAN18Code", drGain["EAN18_Code"]);
                cmd.Parameters.AddWithValue("@Datum", drGain["Datum"]);
                DataTable dtMD = new DataTable();
                SqlDataAdapter daMD = new SqlDataAdapter(cmd);
                daMD.Fill(dtMD);
                SqlCommandBuilder cmbMD = new SqlCommandBuilder(daMD);
                cmbMD.GetInsertCommand();
                cmbMD.GetUpdateCommand();

                Boolean blnPV = false;
                if (HoofdPV == drGain["Ontvanger"].ToString()) { blnPV = true; }

                if (dtMD.Rows.Count > 0)
                {
                    DataRow drMD = dtMD.Rows[0];
                    if ((DateTime)drMD["StartDatum"] == (DateTime)drGain["Datum"])
                    {
                        //Update bestaand record
                        if (blnPV)
                        {
                            drMD["PVActief"] = true;
                        }
                        else
                        {
                            drMD["LVActief"] = true;
                        }
                    }
                    else
                    {
                        //Aanmaken nieuw record
                        drMD["EindDatum"] = drGain["Datum"];
                        DataRow drMDNieuw = dtMD.NewRow();
                        drMDNieuw["EAN18Code"] = drMD["EAN18Code"];
                        drMDNieuw["StartDatum"] = drGain["Datum"];
                        drMDNieuw["EindDatum"] = drMD["EindDatum"];
                        drMDNieuw["AansluitingId"] = drMD["AansluitingId"];
                        if (blnPV)
                        {
                            drMDNieuw["PVActief"] = true;
                            drMDNieuw["LVActief"] = drMD["LVActief"];
                        }
                        else
                        {
                            drMDNieuw["PVActief"] = drMD["PVActief"];
                            drMDNieuw["LVActief"] = true;
                        }
                        drMDNieuw["RelatieId"] = drMD["RelatieId"];
                        drMDNieuw["PortfolioId"] = drMD["PortfolioId"];
                        drMDNieuw["HoofdKenmerkId"] = drMD["HoofdKenmerkId"];
                        drMDNieuw["MeterID"] = drMD["MeterID"];
                        drMDNieuw["FysiekeKenmerkID"] = drMD["FysiekeKenmerkID"];
                        drMDNieuw["VerwerktDT"] = DateTime.Now;
                    }
                    daMD.Update(dtMD);
                }
                else
                {
                    //Gain zonder masterdata
                    //TODO log
                }
            }

        }

        public void VerwerkLoss(ref SqlConnection conn, int BerichtId, String HoofdPV)
        {
            String strSql = "SELECT Loss_ID,Datum,Ontvanger,EAN18_Code,Product,NB,PV,LV,OudeLV,Dossier,Reden,Referentie,OudePV FROM Car.dbo.Loss WHERE Bericht_ID=@Bericht_ID";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Bericht_ID", BerichtId);
            DataTable dtLoss = new DataTable();
            SqlDataAdapter daLoss = new SqlDataAdapter(cmd);
            daLoss.Fill(dtLoss);

            if (dtLoss.Rows.Count > 0)
            {
                DataRow drLoss = dtLoss.Rows[0];
                strSql = "SELECT * FROM EnergieDB.masterdata.Masterdata where EAN18Code=@EAN18Code and StartDatum<=@Datum and EindDatum>@Datum";
                cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@EAN18Code", drLoss["EAN18_Code"]);
                cmd.Parameters.AddWithValue("@Datum", drLoss["Datum"]);
                DataTable dtMD = new DataTable();
                SqlDataAdapter daMD = new SqlDataAdapter(cmd);
                daMD.Fill(dtMD);
                SqlCommandBuilder cmbMD = new SqlCommandBuilder(daMD);
                cmbMD.GetInsertCommand();
                cmbMD.GetUpdateCommand();

                Boolean blnPV = false;
                if (HoofdPV == drLoss["Ontvanger"].ToString()) { blnPV = true; }

                if (dtMD.Rows.Count > 0)
                {
                    DataRow drMD = dtMD.Rows[0];
                    if ((DateTime)drMD["StartDatum"] == (DateTime)drLoss["Datum"])
                    {
                        //Update bestaand record
                        if (blnPV)
                        {
                            drMD["PVActief"] = false;
                        }
                        else
                        {
                            drMD["LVActief"] = false;
                        }
                    }
                    else
                    {
                        //Aanmaken nieuw record
                        drMD["EindDatum"] = drLoss["Datum"];
                        DataRow drMDNieuw = dtMD.NewRow();
                        drMDNieuw["EAN18Code"] = drMD["EAN18Code"];
                        drMDNieuw["StartDatum"] = drLoss["Datum"];
                        drMDNieuw["EindDatum"] = drMD["EindDatum"];
                        drMDNieuw["AansluitingId"] = drMD["AansluitingId"];
                        if (blnPV)
                        {
                            drMDNieuw["PVActief"] = false;
                            drMDNieuw["LVActief"] = drMD["LVActief"];
                        }
                        else
                        {
                            drMDNieuw["PVActief"] = drMD["PVActief"];
                            drMDNieuw["LVActief"] = false;
                        }
                        drMDNieuw["RelatieId"] = drMD["RelatieId"];
                        drMDNieuw["PortfolioId"] = drMD["PortfolioId"];
                        drMDNieuw["HoofdKenmerkId"] = drMD["HoofdKenmerkId"];
                        drMDNieuw["MeterID"] = drMD["MeterID"];
                        drMDNieuw["FysiekeKenmerkID"] = drMD["FysiekeKenmerkID"];
                        drMDNieuw["VerwerktDT"] = DateTime.Now;
                    }
                    daMD.Update(dtMD);
                }
                else
                {
                    //Loss zonder masterdata
                    //TODO log
                }
            }

        }

        public bool SendMail(
            string from,
        string recipients,
        string subject,
        string messageBody,
        bool highPriority,
        string smtpServer,
        int port,
        ref bool succes,
        ref string strError,
        ref SqlConnection conn,
        int inbox_ID)
        {
            try
            {
                //using (MailMessage mailMessage = new MailMessage(from, recipients))
                //{
                //    mailMessage.Subject = subject;
                //    mailMessage.Body = messageBody;
                //    mailMessage.IsBodyHtml = true;

                //    if (highPriority) mailMessage.Priority = MailPriority.High;

                //    SmtpClient smtpClient = new SmtpClient(smtpServer, port);
                //    //smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                //    //smtpClient.Credentials = new System.Net.NetworkCredential(@"Energie\Beheerder", "3n3rg!3");



                //    //smtpClient.EnableSsl = true;

                //    smtpClient.Timeout = 5000; // 5 seconds
                //    smtpClient.Send(mailMessage);
                //}

                string str_SQL = "msdb.dbo.sp_send_dbmail";
                //"@profile_name, " +
                //"@recipients, " +
                //"@subject, " +
                //"@body";

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = str_SQL;
                cmd.Parameters.AddWithValue("@profile_name", "SQLService");
                cmd.Parameters.AddWithValue("@recipients", recipients);
                cmd.Parameters.AddWithValue("@subject", subject);
                cmd.Parameters.AddWithValue("@body", messageBody);
                cmd.Parameters.AddWithValue("@body_format", "HTML");

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    succes = false;
                    strError = "Fout bij verzenden e-mail  : " + ex.Message;
                    WriteLog(strError, LEVEL_WarningICT, ref conn, inbox_ID);

                }
                catch { }

                return false;
            }
        }

        public DataRow LeesAansluitingen(ref SqlConnection conn, Int64 EAN18_Code, String Naam, int AansluitingType)
        {
            String strSql = "select * from EnergieDB.dbo.aansluitingen where EAN18_Code=@EAN18_Code";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN18_Code", EAN18_Code);
            DataTable dtAansluitingen = new DataTable();
            SqlDataAdapter daAansluitingen = new SqlDataAdapter(cmd);
            daAansluitingen.Fill(dtAansluitingen);

            if (dtAansluitingen.Rows.Count == 0)
            {
                strSql = "INSERT INTO EnergieDB.dbo.Aansluitingen \n";
                strSql += "(EAN18_Code \n";
                strSql += ",Aansluitingtype_ID \n";
                strSql += ",Naam) \n";
                strSql += "VALUES \n";
                strSql += "(@EAN18_Code \n";
                strSql += ",@Aansluitingtype_ID \n";
                strSql += ",@Naam)";
                cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@EAN18_Code", EAN18_Code);
                cmd.Parameters.AddWithValue("@Aansluitingtype_ID", AansluitingType);
                cmd.Parameters.AddWithValue("@Naam", Naam);
                cmd.ExecuteNonQuery();

                strSql = "select * from EnergieDB.dbo.aansluitingen where EAN18_Code=@EAN18_Code";
                cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@EAN18_Code", EAN18_Code);
                dtAansluitingen = new DataTable();
                daAansluitingen = new SqlDataAdapter(cmd);
                daAansluitingen.Fill(dtAansluitingen);
            }

            return dtAansluitingen.Rows[0];
        }

        private DataRow LeesVorigeMasterdata(ref SqlConnection conn, Int64 EAN18_Code, DateTime DatumMutatie, ref DataTable dtVorigeMasterdata, ref SqlDataAdapter daVorigeMasterdata, ref SqlCommandBuilder cbVorigeMasterdata)
        {
            String strSql = "SELECT Top 1 * FROM EnergieDb.masterdata.Masterdata where EAN18Code=@EAN18Code and StartDatum<=@StartDatum order by StartDatum desc";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN18Code", EAN18_Code);
            cmd.Parameters.AddWithValue("@StartDatum", DatumMutatie);
            dtVorigeMasterdata = new DataTable();
            daVorigeMasterdata = new SqlDataAdapter(cmd);
            daVorigeMasterdata.Fill(dtVorigeMasterdata);
            cbVorigeMasterdata = new SqlCommandBuilder(daVorigeMasterdata);
            cbVorigeMasterdata.GetUpdateCommand();

            if (dtVorigeMasterdata.Rows.Count > 0)
            {
                return dtVorigeMasterdata.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private DataRow LeesSwitch(ref SqlConnection conn, int AansluitingId, DateTime MutatieDatum)
        {
            String strSql = "SELECT *  FROM EnergieDB.dbo.SwitchBerichten where Aansluiting_ID=@Aansluiting_ID and Contract_Start_DT=@Contract_Start_DT";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Aansluiting_ID", AansluitingId);
            cmd.Parameters.AddWithValue("@Contract_Start_DT", MutatieDatum);
            DataTable dtSwitch = new DataTable();
            SqlDataAdapter daSwitch = new SqlDataAdapter(cmd);
            daSwitch.Fill(dtSwitch);
            if (dtSwitch.Rows.Count > 0)
            {
                return dtSwitch.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private int LeesNetbeheerder(ref SqlConnection conn, Int64 NetbeheerderEAN)
        {
            String strSql = "SELECT ID FROM EnergieDB.dbo.Netbeheerders WHERE EAN13_Code=@EAN13_Code";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN13_Code", NetbeheerderEAN);
            DataTable dtNetbeheerder = new DataTable();
            SqlDataAdapter daNetbeheerder = new SqlDataAdapter(cmd);
            daNetbeheerder.Fill(dtNetbeheerder);
            if (dtNetbeheerder.Rows.Count > 0)
            {
                return (int)dtNetbeheerder.Rows[0]["ID"];
            }
            else
            {
                return -1;
            }
        }

        private int LeesNetgebied(ref SqlConnection conn, Int64 NetGebiedEAN)
        {
            String strSql = "SELECT ID FROM EnergieDB.dbo.Netgebieden WHERE EAN18_Code=@EAN18_Code";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN18_Code", NetGebiedEAN);
            DataTable dtNetgebied = new DataTable();
            SqlDataAdapter daGbied = new SqlDataAdapter(cmd);
            daGbied.Fill(dtNetgebied);
            if (dtNetgebied.Rows.Count > 0)
            {
                return (int)dtNetgebied.Rows[0]["ID"];
            }
            else
            {
                return -1;
            }
        }

        private int LeesPV(ref SqlConnection conn, Int64 PVEan)
        {
            String strSql = "SELECT ID FROM EnergieDB.dbo.PVs WHERE EAN13_Code=@EAN13_Code";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN13_Code", PVEan);
            DataTable dtPV = new DataTable();
            SqlDataAdapter daPV = new SqlDataAdapter(cmd);
            daPV.Fill(dtPV);
            if (dtPV.Rows.Count > 0)
            {
                return (int)dtPV.Rows[0]["ID"];
            }
            else
            {
                return -1;
            }
        }

        private int LeesAccount(ref SqlConnection conn, Int64 LVEAN)
        {
            String strSql = "select Accounts.ID from Leveranciers \n";
            strSql += "join Portfolios on Portfolios.Leverancier_ID = Leveranciers.ID \n";
            strSql += "join klanten on klanten.Portfolio_ID = Portfolios.ID \n";
            strSql += "join Accounts on Accounts.Klant_ID = Klanten.ID";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN13_Code", LVEAN);
            DataTable dtAccount = new DataTable();
            SqlDataAdapter daAccount = new SqlDataAdapter(cmd);
            daAccount.Fill(dtAccount);
            if (dtAccount.Rows.Count > 0)
            {
                return (int)dtAccount.Rows[0]["ID"];
            }
            else
            {
                return -1;
            }
        }

        private int LeesLV(ref SqlConnection conn, Int64 LVEan)
        {
            String strSql = "SELECT ID FROM EnergieDB.dbo.Leveranciers WHERE EAN13_Code=@EAN13_Code";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN13_Code", LVEan);
            DataTable dtLV = new DataTable();
            SqlDataAdapter daLV = new SqlDataAdapter(cmd);
            daLV.Fill(dtLV);
            if (dtLV.Rows.Count > 0)
            {
                return (int)dtLV.Rows[0]["ID"];
            }
            else
            {
                return -1;
            }
        }

        private int LeesMV(ref SqlConnection conn, Int64 MVEan)
        {
            String strSql = "SELECT ID FROM EnergieDB.dbo.MVs WHERE EAN13_Code=@EAN13_Code";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN13_Code", MVEan);
            DataTable dtMV = new DataTable();
            SqlDataAdapter daMV = new SqlDataAdapter(cmd);
            daMV.Fill(dtMV);
            if (dtMV.Rows.Count > 0)
            {
                return (int)dtMV.Rows[0]["ID"];
            }
            else
            {
                return -1;
            }
        }

        private DataRow LeesVorigeHoofdKenmerk(ref SqlConnection conn, int hoofdKenmerkId)
        {
            String strSql = "SELECT * FROM EnergieDB.masterdata.HoofdKenmerk where HoofdKenmerkId=@HoofdKenmerkId";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@HoofdKenmerkId", hoofdKenmerkId);
            DataTable dtHoofdKenmerk = new DataTable();
            SqlDataAdapter daHoofdKenmerk = new SqlDataAdapter(cmd);
            daHoofdKenmerk.Fill(dtHoofdKenmerk);

            if (dtHoofdKenmerk.Rows.Count > 0)
            {
                return dtHoofdKenmerk.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private DataRow LeesVorigeFysiekeKenmerk(ref SqlConnection conn, int fysiekeKenmerkId)
        {
            String strSql = "SELECT * FROM EnergieDB.masterdata.FysiekeKenmerk where FysiekeKenmerkID=@FysiekeKenmerkID";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@FysiekeKenmerkID", fysiekeKenmerkId);
            DataTable dtFysiekeKenmerk = new DataTable();
            SqlDataAdapter daFysiekeKenmerk = new SqlDataAdapter(cmd);
            daFysiekeKenmerk.Fill(dtFysiekeKenmerk);

            if (dtFysiekeKenmerk.Rows.Count > 0)
            {
                return dtFysiekeKenmerk.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private DataRow LeesVorigeMeter(ref SqlConnection conn, int meterId)
        {
            String strSql = "SELECT * FROM EnergieDB.masterdata.Meter where MeterId=@MeterId";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@MeterId", meterId);
            DataTable dtMeter = new DataTable();
            SqlDataAdapter daMeter = new SqlDataAdapter(cmd);
            daMeter.Fill(dtMeter);

            if (dtMeter.Rows.Count > 0)
            {
                return dtMeter.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private DataRow LeesVorigeRegister(ref SqlConnection conn, int meterId, string registerId)
        {
            String strSql = "SELECT Register.*, LeveringsRichting.Id as LeveringsRichtingId FROM EnergieDB.masterdata.Register join EnergieDB.dbo.LeveringsRichting on LeveringsRichting.Id=Register.LeveringsRichting where MeterId=@MeterId and RegisterId=@RegisterId";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@MeterId", meterId);
            cmd.Parameters.AddWithValue("@RegisterId", registerId);
            DataTable dtRegister = new DataTable();
            SqlDataAdapter daRegister = new SqlDataAdapter(cmd);
            daRegister.Fill(dtRegister);

            if (dtRegister.Rows.Count > 0)
            {
                return dtRegister.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private DataRow LeesVorigDossier(ref SqlConnection conn, Int64 eanCode, DateTime startDatum, String dossier, String reden)
        {
            String strSql = "SELECT * FROM EnergieDB.masterdata.Dossier where EAN18Code=@EAN18Code and StartDatum=@StartDatum and Dossier=@Dossier and Reden=@Reden";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN18Code", eanCode);
            cmd.Parameters.AddWithValue("@StartDatum", startDatum);
            cmd.Parameters.AddWithValue("@Dossier", dossier);
            cmd.Parameters.AddWithValue("@Reden", reden);
            DataTable dtDossier = new DataTable();
            SqlDataAdapter daDossier = new SqlDataAdapter(cmd);
            daDossier.Fill(dtDossier);

            if (dtDossier.Rows.Count > 0)
            {
                return dtDossier.Rows[0];
            }
            else
            {
                return null;
            }
        }
        public string getMessageID(ref SqlConnection conn)
        {
            SqlCommand cmdGetID = new SqlCommand("[Messages].[dbo].[p_getMessageNumber]", conn);
            cmdGetID.CommandType = CommandType.StoredProcedure;
            cmdGetID.Parameters.Add("@Messagenumber", SqlDbType.VarChar, 14);
            cmdGetID.Parameters["@Messagenumber"].Direction = ParameterDirection.Output;
            cmdGetID.ExecuteNonQuery();
            string messageID;
            messageID = (string)cmdGetID.Parameters["@Messagenumber"].Value;
            return messageID;
        }

        public int getOffsetMessageOut(DateTime utcDate, ref SqlConnection conn)
        {
            /*
             * Note: It is not permitted to send two different time zones within one message.
             * Note: All dates in a message shall be given with the same time zone. E. g. both “Message date” and
             *       “Processing start date” should be given with the same time zone.
             *
             * The moment of preparation/transmission of the message (message date) defines the choice 
             * between summer- and wintertime 
             */

            utcDate = DateTime.Parse(utcDate.ToShortDateString());
            int offs = 0;
            string SQLstatement;

            //  Voor offset nemen we (NL_DT - UTC_DT) van het begiin van de dag van het E-programma) 
            SQLstatement = "select @offset = DATEDIFF(hh, utc_dt,NL_DT) from ENERGIEDB.dbo.Tijden " +
                           "where NL_DT = @date";
            SqlCommand cmdGetOffset = new SqlCommand(SQLstatement, conn);
            cmdGetOffset.Parameters.AddWithValue("@date", utcDate);
            cmdGetOffset.Parameters.Add(new SqlParameter("@offset", SqlDbType.Int));
            cmdGetOffset.Parameters["@offset"].Direction = ParameterDirection.Output;
            cmdGetOffset.ExecuteNonQuery();
            offs = (int)cmdGetOffset.Parameters["@offset"].Value;
            return offs;
        }

        public void WriteLog(string omschrijving, int LevelID, ref SqlConnection conn, int bericht_ID)
        {
            try
            {

                SqlCommand cmdLog = new SqlCommand();
                cmdLog.Connection = conn;
                String strSql = "INSERT INTO Car.dbo.Log \n";
                strSql += "(TimeStamp \n";
                strSql += ", App_ID \n";
                strSql += ", Omschrijving \n";
                strSql += ", FoutLevel \n";
                strSql += ", Bericht_ID) \n";
                strSql += "VALUES \n";
                strSql += "(@TimeStamp \n";
                strSql += ", @App_ID \n";
                strSql += ", @Omschrijving \n";
                strSql += ", @FoutLevel \n";
                strSql += ", @Bericht_ID)";
                cmdLog.CommandText = strSql;
                cmdLog.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                cmdLog.Parameters.AddWithValue("@App_ID", 2);
                string strDescription = omschrijving;
                if (strDescription.Length > 500) { strDescription = strDescription.Substring(0, 500); }

                cmdLog.Parameters.AddWithValue("@Omschrijving", strDescription);
                cmdLog.Parameters.AddWithValue("@FoutLevel", LevelID);
                cmdLog.Parameters.AddWithValue("@Bericht_ID", bericht_ID);
                cmdLog.ExecuteNonQuery();

            }
            catch // (Exception ex)
            {
                //EventLog eventlog = new EventLog("Application");
                //eventlog.Source = "Energie App";
                //eventlog.WriteEntry("WriteLog : " + ex.Message, EventLogEntryType.Error, 0);
            }

        }
        //private EventLog EventLog
        //{

        //    get
        //    {
        //        if (eventlog == null)
        //        {
        //            eventlog = new EventLog("Application");
        //        }
        //        return eventlog;
        //    }
        //}
        private void LogEvent(string message, EventLogEntryType type)
        {
            EventLog eventlog = new EventLog("Application");
            eventlog.Source = "ProcessInbox";
            eventlog.WriteEntry(message, type);
        }
        private int AanmakenAansluiting(long eancode, string naam, int aansluitingTypeID, ref SqlConnection conn, int inboxID)
        {
            try
            {
                string SQLStatement = "INSERT INTO ENERGIEDB.dbo.Aansluitingen " +
                                      "(EAN18_Code " +
                                      ",Aansluitingtype_ID " +
                                      ",Naam) " +
                                      "VALUES " +
                                      "(@EAN18_Code " +
                                      ",@Aansluitingtype_ID " +
                                      ",@Naam) " +
                                      "SELECT SCOPE_IDENTITY()";
                SqlCommand cmd = new SqlCommand(SQLStatement, conn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 12000;
                cmd.Parameters.AddWithValue("@EAN18_Code", eancode);

                cmd.Parameters.AddWithValue("@Aansluitingtype_ID", aansluitingTypeID);

                cmd.Parameters.AddWithValue("@Naam", naam);


                return int.Parse(cmd.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken aansluiting : " + ex.Message, LEVEL_Critical, ref conn, inboxID);
                return -1;
            }
        }

        //private  int AanmakenAdres(string straat, string huisnummer, string toevoeging, string postcode, string plaats, ref SqlConnection conn)
        //{
        //    try
        //    {
        //        string SQLStatement = "INSERT INTO [ENERGIEDB].[dbo].[Adressen] " +
        //                            "([Straat] " +
        //                            ",[Huisnummer] " +
        //                            ",[Toevoeging] " +
        //                            ",[Postcode] " +
        //                            ",[Plaats] " +
        //                            ",[Land_Code]) " +
        //                            "VALUES " +
        //                            "(@Straat " +
        //                            ",@Huisnummer " +
        //                            ",@Toevoeging " +
        //                            ",@Postcode " +
        //                            ",@Plaats " +
        //                            ",@Land_Code) " +
        //                            "SELECT SCOPE_IDENTITY()";
        //        SqlCommand cmd = new SqlCommand(SQLStatement, conn);
        //        cmd.CommandType = CommandType.Text;
        //        cmd.CommandTimeout = 12000;
        //        cmd.Parameters.AddWithValue("@Straat", straat);
        //        cmd.Parameters.AddWithValue("@Huisnummer", huisnummer);
        //        cmd.Parameters.AddWithValue("@Toevoeging", toevoeging);
        //        cmd.Parameters.AddWithValue("@Postcode", postcode);
        //        cmd.Parameters.AddWithValue("@Plaats", plaats);
        //        cmd.Parameters.AddWithValue("@Land_Code", "NLD");

        //        return int.Parse(cmd.ExecuteScalar().ToString());


        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog("Fout bij aanmaken adres : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
        //        return -1;
        //    }
        //}

        //private int AanmakenRelatie(ref SqlConnection conn, int BerichtId)
        //{
        //    try
        //    {
        //        int Relatie_ID = -1;
        //        String str_SQL = "INSERT INTO ENERGIEDB.dbo.Relaties " +
        //            "(Naam " +
        //            ",Korte_Naam " +
        //            ",Naamregel1 " +
        //            ",Naamregel2 " +
        //            ",Naamregel3 " +
        //            ",GeboorteDatum " +
        //            ",IBAN " +
        //            ",Voornaam " +
        //            ",Achternaam " +
        //            ",Email " +
        //            ",Tussenvoegsel " +
        //            ",NieuwEmail " +
        //            ",Straat " +
        //            ",Plaats " +
        //            ",Huisnummer " +
        //            ",Toevoeging) " +
        //            "VALUES " +
        //            "(@Naam " +
        //            ",@Korte_Naam " +
        //            ",@Naamregel1 " +
        //            ",@Naamregel2 " +
        //            ",@Naamregel3 " +
        //            ",@Adres_ID " +
        //            ",@GeboorteDatum " +
        //            ",@IBAN " +
        //            ",@Voornaam " +
        //            ",@Achternaam " +
        //            ",@Email " +
        //            ",@Tussenvoegsel " +
        //            ",@NieuwEmail " +
        //            ",@Straat " +
        //            ",@Plaats " +
        //            ",@Huisnummer " +
        //            ",@Toevoeging) " +
        //            "select ID FROM ENERGIEDB.dbo.Relaties  " +
        //            "WHERE ID=SCOPE_IDENTITY()";
        //        SqlCommand cmd = conn.CreateCommand();
        //        cmd.CommandText = str_SQL;
        //        cmd.Parameters.AddWithValue("@Naam", relatie.Naam);
        //        cmd.Parameters.AddWithValue("@Korte_Naam", relatie.Korte_Naam);
        //        cmd.Parameters.AddWithValue("@Naamregel1", relatie.Naamregel1);
        //        cmd.Parameters.AddWithValue("@Naamregel2", relatie.Naamregel2);
        //        cmd.Parameters.AddWithValue("@Naamregel3", relatie.Naamregel3);
        //        cmd.Parameters.AddWithValue("@GeboorteDatum", relatie.GeboorteDatum);
        //        cmd.Parameters.AddWithValue("@IBAN", relatie.IBAN);
        //        cmd.Parameters.AddWithValue("@Voornaam", relatie.Voornaam);
        //        cmd.Parameters.AddWithValue("@Achternaam", relatie.Achternaam);
        //        cmd.Parameters.AddWithValue("@Email", relatie.Email);
        //        cmd.Parameters.AddWithValue("@Tussenvoegsel", relatie.Tussenvoegsel);
        //        cmd.Parameters.AddWithValue("@NieuwEmail", relatie.NieuwEmail);
        //        cmd.Parameters.AddWithValue("@Straat", relatie.Straat);
        //        cmd.Parameters.AddWithValue("@Plaats", relatie.Plaats);
        //        cmd.Parameters.AddWithValue("@Huisnummer", relatie.Huisnummer);
        //        cmd.Parameters.AddWithValue("@Toevoeging", relatie.Toevoeging);
        //        SqlDataReader srdr = cmd.ExecuteReader();
        //        if (srdr.HasRows)
        //        {
        //            while (srdr.Read())
        //            {
        //                Relatie_ID = srdr.GetInt32(0);
        //            }
        //        }
        //        srdr.Close();

        //        return Relatie_ID;


        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog("Fout bij aanmaken relatie : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
        //        return -1;
        //    }
        //}

        private Boolean AanmakenMasterData(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                //int Relatie_ID = -1;
                String strSql = "INSERT INTO EnergieDB.masterdata.Masterdata \n";
                strSql += "(EAN18Code \n";
                strSql += ",StartDatum \n";
                strSql += ",EindDatum \n";
                strSql += ",AansluitingId \n";
                //strSql += ",AdresId \n";
                strSql += ",PVActief \n";
                strSql += ",LVActief \n";
                strSql += ",RelatieId \n";
                strSql += ",PortfolioId \n";
                strSql += ",HoofdKenmerkId \n";
                strSql += ",MeterID \n";
                strSql += ",FysiekeKenmerkID \n";
                strSql += ",VerwerktDT) \n";
                strSql += "VALUES \n";
                strSql += "(@EAN18Code \n";
                strSql += ",@StartDatum \n";
                strSql += ",@EindDatum \n";
                strSql += ",@AansluitingId \n";
                //strSql += ",@AdresId \n";
                strSql += ",@PVActief \n";
                strSql += ",@LVActief \n";
                strSql += ",@RelatieId \n";
                strSql += ",@PortfolioId \n";
                strSql += ",@HoofdKenmerkId \n";
                strSql += ",@MeterID \n";
                strSql += ",@FysiekeKenmerkID \n";
                strSql += ",@VerwerktDT)";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@EAN18Code", masterdata.EAN18Code);
                cmd.Parameters.AddWithValue("@StartDatum", masterdata.StartDatum);
                cmd.Parameters.AddWithValue("@EindDatum", masterdata.EindDatum);
                cmd.Parameters.AddWithValue("@AansluitingId", masterdata.AansluitingId);
                //cmd.Parameters.AddWithValue("@AdresId", Masterdata.AdresId);
                cmd.Parameters.AddWithValue("@PVActief", masterdata.PVActief);
                cmd.Parameters.AddWithValue("@LVActief", masterdata.LVActief);
                cmd.Parameters.AddWithValue("@RelatieId", masterdata.RelatieId == -1 ? 66159 : masterdata.RelatieId);//TODO Als er geen switch gevonden wordt kan de masterdata niet weggeschreven worden. Nu update met 66159
                cmd.Parameters.AddWithValue("@PortfolioId", masterdata.PortfolioId);
                cmd.Parameters.AddWithValue("@HoofdKenmerkId", masterdata.HoofdKenmerkId);
                cmd.Parameters.AddWithValue("@MeterID", masterdata.MeterID);
                cmd.Parameters.AddWithValue("@FysiekeKenmerkID", masterdata.FysiekeKenmerkID);
                cmd.Parameters.AddWithValue("@VerwerktDT", masterdata.VerwerktDT);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken Masterdata : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return false;
            }
        }

        private Boolean UpdateMasterData(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                //int Relatie_ID = -1;
                String strSql = "UPDATE EnergieDB.masterdata.Masterdata \n";
                strSql += "SET EindDatum = @EindDatum \n";
                strSql += ",AansluitingId = @AansluitingId \n";
                //strSql += ",AdresId = @AdresId \n";
                strSql += ",PVActief = @PVActief \n";
                strSql += ",LVActief = @LVActief \n";
                strSql += ",RelatieId = @RelatieId \n";
                strSql += ",PortfolioId = @PortfolioId \n";
                strSql += ",HoofdKenmerkId = @HoofdKenmerkId \n";
                strSql += ",MeterID = @MeterID \n";
                strSql += ",FysiekeKenmerkID = @FysiekeKenmerkID \n";
                strSql += ",VerwerktDT = @VerwerktDT \n";
                strSql += "WHERE EAN18Code = @EAN18Code and StartDatum = @StartDatum";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@EAN18Code", masterdata.EAN18Code);
                cmd.Parameters.AddWithValue("@StartDatum", masterdata.StartDatum);
                cmd.Parameters.AddWithValue("@EindDatum", masterdata.EindDatum);
                cmd.Parameters.AddWithValue("@AansluitingId", masterdata.AansluitingId);
                //cmd.Parameters.AddWithValue("@AdresId", masterdata.AdresId);
                cmd.Parameters.AddWithValue("@PVActief", masterdata.PVActief);
                cmd.Parameters.AddWithValue("@LVActief", masterdata.LVActief);
                cmd.Parameters.AddWithValue("@RelatieId", masterdata.RelatieId);
                cmd.Parameters.AddWithValue("@PortfolioId", masterdata.PortfolioId);
                cmd.Parameters.AddWithValue("@HoofdKenmerkId", masterdata.HoofdKenmerkId);
                cmd.Parameters.AddWithValue("@MeterID", masterdata.MeterID);
                cmd.Parameters.AddWithValue("@FysiekeKenmerkID", masterdata.FysiekeKenmerkID);
                cmd.Parameters.AddWithValue("@VerwerktDT", masterdata.VerwerktDT);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij update Masterdata : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return false;
            }
        }

        private Boolean AanmakenDossier(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                //int Relatie_ID = -1;
                String strSql = "INSERT INTO EnergieDB.masterdata.Dossier \n";
                strSql += "(EAN18Code \n";
                strSql += ",StartDatum \n";
                strSql += ",Dossier \n";
                strSql += ",Referentie \n";
                strSql += ",Reden) \n";
                strSql += "VALUES \n";
                strSql += "(@EAN18Code \n";
                strSql += ",@StartDatum \n";
                strSql += ",@Dossier \n";
                strSql += ",@Referentie \n";
                strSql += ",@Reden) \n";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@EAN18Code", masterdataDossier.EAN18Code);
                cmd.Parameters.AddWithValue("@StartDatum", masterdataDossier.StartDatum);
                cmd.Parameters.AddWithValue("@Dossier", masterdataDossier.Dossier);
                cmd.Parameters.AddWithValue("@Referentie", masterdataDossier.Referentie);
                cmd.Parameters.AddWithValue("@Reden", masterdataDossier.Reden);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken Dossier : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return false;
            }
        }

        private Boolean UpdateDossier(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                //int Relatie_ID = -1;
                String strSql = "UPDATE EnergieDB.masterdata.Dossier \n";
                strSql += "SET Referentie = @Referentie, Reden=@Reden \n";
                strSql += "WHERE EAN18Code = @EAN18Code and StartDatum = @StartDatum and Dossier = @Dossier";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@EAN18Code", masterdataDossier.EAN18Code);
                cmd.Parameters.AddWithValue("@StartDatum", masterdataDossier.StartDatum);
                cmd.Parameters.AddWithValue("@Dossier", masterdataDossier.Dossier);
                cmd.Parameters.AddWithValue("@Referentie", masterdataDossier.Referentie);
                cmd.Parameters.AddWithValue("@Reden", masterdataDossier.Reden);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij update Dossier : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return false;
            }
        }

        private int AanmakenFysiekeKenmerk(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                int FysiekeKenmerkID = -1;
                String strSql = "INSERT INTO EnergieDB.masterdata.FysiekeKenmerk \n";
                strSql += "(BemeteringsType \n";
                strSql += ",Profiel \n";
                if (fysiekeKenmerk.CapTarCode > 0) { strSql += ",CapTarCode \n"; }
                strSql += ",FysiekeStatusId \n";
                strSql += ",AdminStatusSmartMeter \n";
                strSql += ",LeveringsStatus \n";
                strSql += ",LeveringsRichtingId \n";
                strSql += ",SJVPeak \n";
                strSql += ",SJVOffPeak \n";
                strSql += ",SJIPeak \n";
                strSql += ",SJIOffPeak \n";
                strSql += ",AllocatieMethode \n";
                strSql += ",BusinessType \n";
                strSql += ",PowerSystemType)  \n";
                strSql += "VALUES \n";
                strSql += "(@BemeteringsType \n";
                strSql += ",@Profiel \n";
                if (fysiekeKenmerk.CapTarCode > 0) { strSql += ",@CapTarCode \n"; }
                strSql += ",@FysiekeStatusId \n";
                strSql += ",@AdminStatusSmartMeter \n";
                strSql += ",@LeveringsStatus \n";
                strSql += ",@LeveringsRichtingId \n";
                strSql += ",@SJVPeak \n";
                strSql += ",@SJVOffPeak \n";
                strSql += ",@SJIPeak \n";
                strSql += ",@SJIOffPeak \n";
                strSql += ",@AllocatieMethode \n";
                strSql += ",@BusinessType \n";
                strSql += ",@PowerSystemType); SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@BemeteringsType", fysiekeKenmerk.BemeteringsType);
                cmd.Parameters.AddWithValue("@Profiel", fysiekeKenmerk.Profiel);
                if (fysiekeKenmerk.CapTarCode > 0) { cmd.Parameters.AddWithValue("@CapTarCode", fysiekeKenmerk.CapTarCode); }
                cmd.Parameters.AddWithValue("@FysiekeStatusId", fysiekeKenmerk.FysiekeStatusId);
                cmd.Parameters.AddWithValue("@AdminStatusSmartMeter", fysiekeKenmerk.AdminStatusSmartMeter);
                cmd.Parameters.AddWithValue("@LeveringsStatus", fysiekeKenmerk.LeveringsStatus);
                cmd.Parameters.AddWithValue("@LeveringsRichtingId", fysiekeKenmerk.LeveringsRichtingId);
                cmd.Parameters.AddWithValue("@SJVPeak", fysiekeKenmerk.SJVPeak);
                cmd.Parameters.AddWithValue("@SJVOffPeak", fysiekeKenmerk.SJVOffPeak);
                cmd.Parameters.AddWithValue("@SJIPeak", fysiekeKenmerk.SJIPeak);
                cmd.Parameters.AddWithValue("@SJIOffPeak", fysiekeKenmerk.SJIOffPeak);
                cmd.Parameters.AddWithValue("@AllocatieMethode", fysiekeKenmerk.AllocatieMethode);
                cmd.Parameters.AddWithValue("@BusinessType", fysiekeKenmerk.BusinessType);
                cmd.Parameters.AddWithValue("@PowerSystemType", fysiekeKenmerk.PowerSystemType);
                Object objResult = cmd.ExecuteScalar();
                if (objResult != null)
                {

                    FysiekeKenmerkID = int.Parse(objResult.ToString());
                    fysiekeKenmerk.FysiekeKenmerkID = FysiekeKenmerkID;

                }

                return FysiekeKenmerkID;

            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken FysiekeKenmerk : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }
        }

        private int UpdateFysiekeKenmerk(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                int FysiekeKenmerkID = -1;
                String strSql = "UPDATE EnergieDB.masterdata.FysiekeKenmerk \n";
                strSql += "SET BemeteringsType = @BemeteringsType \n";
                strSql += ",Profiel = @Profiel \n";
                strSql += ",CapTarCode = @CapTarCode \n";
                strSql += ",FysiekeStatusId = @FysiekeStatusId \n";
                strSql += ",AdminStatusSmartMeter = @AdminStatusSmartMeter \n";
                strSql += ",LeveringsStatus = @LeveringsStatus \n";
                strSql += ",LeveringsRichtingId = @LeveringsRichtingId \n";
                strSql += ",SJVPeak = @SJVPeak \n";
                strSql += ",SJVOffPeak = @SJVOffPeak \n";
                strSql += ",SJIPeak = @SJIPeak \n";
                strSql += ",SJIOffPeak = @SJIOffPeak \n";
                strSql += ",AllocatieMethode = @AllocatieMethode \n";
                strSql += ",BusinessType=@BusinessType  \n";
                strSql += ",PowerSystemType=@PowerSystemType \n";
                strSql += "WHERE FysiekeKenmerkID = @FysiekeKenmerkID";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@BemeteringsType", fysiekeKenmerk.BemeteringsType);
                cmd.Parameters.AddWithValue("@Profiel", fysiekeKenmerk.Profiel);
                cmd.Parameters.AddWithValue("@CapTarCode", fysiekeKenmerk.CapTarCode);
                cmd.Parameters.AddWithValue("@FysiekeStatusId", fysiekeKenmerk.FysiekeStatusId);
                cmd.Parameters.AddWithValue("@AdminStatusSmartMeter", fysiekeKenmerk.AdminStatusSmartMeter);
                cmd.Parameters.AddWithValue("@LeveringsStatus", fysiekeKenmerk.LeveringsStatus);
                cmd.Parameters.AddWithValue("@LeveringsRichtingId", fysiekeKenmerk.LeveringsRichtingId);
                cmd.Parameters.AddWithValue("@SJVPeak", fysiekeKenmerk.SJVPeak);
                cmd.Parameters.AddWithValue("@SJVOffPeak", fysiekeKenmerk.SJVOffPeak);
                cmd.Parameters.AddWithValue("@SJIPeak", fysiekeKenmerk.SJIPeak);
                cmd.Parameters.AddWithValue("@SJIOffPeak", fysiekeKenmerk.SJIOffPeak);
                cmd.Parameters.AddWithValue("@AllocatieMethode", fysiekeKenmerk.AllocatieMethode);
                cmd.Parameters.AddWithValue("@FysiekeKenmerkID", fysiekeKenmerk.FysiekeKenmerkID);
                cmd.Parameters.AddWithValue("@BusinessType", fysiekeKenmerk.BusinessType);
                cmd.Parameters.AddWithValue("@PowerSystemType", fysiekeKenmerk.PowerSystemType);
                cmd.ExecuteNonQuery();
                FysiekeKenmerkID = fysiekeKenmerk.FysiekeKenmerkID;

                return FysiekeKenmerkID;

            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken FysiekeKenmerk : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }
        }

        private int AanmakenHoofdKenmerk(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                int HoofdKenmerkId = -1;
                String strSql = "INSERT INTO EnergieDB.masterdata.HoofdKenmerk \n";
                strSql += "(NetbeheerderId \n";
                strSql += ",NetgebiedId \n";
                strSql += ",PV \n";
                strSql += ",LV \n";
                if (hoofdKenmerk.MV > -1)
                {
                    strSql += ",MV \n";
                }
                strSql += ",Product \n";
                strSql += ",FactuurMaand \n";
                strSql += ",ContractedCapacity \n";
                strSql += ",MaxConsumption \n";
                strSql += ",Residential \n";
                strSql += ",Complex \n";
                strSql += ",MarketSegment \n";
                strSql += ",Straat \n";
                strSql += ",Huisnummer \n";
                strSql += ",Toevoeging \n";
                strSql += ",Postcode \n";
                strSql += ",Plaats) \n";
                strSql += "VALUES \n";
                strSql += "(@NetbeheerderId \n";
                strSql += ",@NetgebiedId \n";
                strSql += ",@PV \n";
                strSql += ",@LV \n";
                if (hoofdKenmerk.MV > -1)
                {
                    strSql += ",@MV \n";
                }
                strSql += ",@Product \n";
                strSql += ",@FactuurMaand \n";
                strSql += ",@ContractedCapacity \n";
                strSql += ",@MaxConsumption \n";
                strSql += ",@Residential \n";
                strSql += ",@Complex \n";
                strSql += ",@MarketSegment  \n";
                strSql += ",@Straat \n";
                strSql += ",@Huisnummer \n";
                strSql += ",@Toevoeging \n";
                strSql += ",@Postcode \n";
                strSql += ",@Plaats \n";
                strSql += "); SELECT SCOPE_IDENTITY()";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@NetbeheerderId", hoofdKenmerk.NetbeheerderId);
                cmd.Parameters.AddWithValue("@NetgebiedId", hoofdKenmerk.NetgebiedId);
                cmd.Parameters.AddWithValue("@PV", hoofdKenmerk.PV);
                cmd.Parameters.AddWithValue("@LV", hoofdKenmerk.LV);
                if (hoofdKenmerk.MV > -1)
                {
                    cmd.Parameters.AddWithValue("@MV", hoofdKenmerk.MV);
                }
                cmd.Parameters.AddWithValue("@Product", hoofdKenmerk.Product);
                cmd.Parameters.AddWithValue("@FactuurMaand", hoofdKenmerk.FactuurMaand);
                cmd.Parameters.AddWithValue("@ContractedCapacity", hoofdKenmerk.ContractedCapacity);
                cmd.Parameters.AddWithValue("@MaxConsumption", hoofdKenmerk.MaxConsumption);
                cmd.Parameters.AddWithValue("@Residential", hoofdKenmerk.Residential);
                cmd.Parameters.AddWithValue("@Complex", hoofdKenmerk.Complex);
                cmd.Parameters.AddWithValue("@MarketSegment", hoofdKenmerk.MarketSegment);
                cmd.Parameters.AddWithValue("@Straat", hoofdKenmerk.Straat);
                cmd.Parameters.AddWithValue("@Huisnummer", hoofdKenmerk.Huisnummer);
                cmd.Parameters.AddWithValue("@Toevoeging", hoofdKenmerk.Toevoeging);
                cmd.Parameters.AddWithValue("@Postcode", hoofdKenmerk.Postcode);
                cmd.Parameters.AddWithValue("@Plaats", hoofdKenmerk.Plaats);
                Object objResult = cmd.ExecuteScalar();
                if (objResult != null)
                {
                    HoofdKenmerkId = int.Parse(objResult.ToString());
                    hoofdKenmerk.HoofdKenmerkId = HoofdKenmerkId;
                }
                else
                {
                    WriteLog("Fout bij aanmaken Hoofdkenmerk geen Id bepaald", LEVEL_Critical, ref conn, BerichtId);
                    return -1;
                }
                //SqlDataReader srdr = cmd.ExecuteReader();
                //if (srdr.HasRows)
                //{
                //    while (srdr.Read())
                //    {
                //        HoofdKenmerkId = srdr.GetInt32(0);
                //        HoofdKenmerk.HoofdKenmerkId = HoofdKenmerkId;
                //    }
                //}
                //srdr.Close();

                return HoofdKenmerkId;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken HoofdKenmerk : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }
        }



        private int UpdateHoofdKenmerk(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                int HoofdKenmerkId = -1;
                String strSql = "UPDATE EnergieDB.masterdata.HoofdKenmerk \n";
                strSql += "SET NetbeheerderId = @NetbeheerderId \n";
                strSql += ",NetgebiedId = @NetgebiedId \n";
                strSql += ",PV = @PV \n";
                strSql += ",LV = @LV \n";
                strSql += ",MV = @MV \n";
                strSql += ",Product = @Product \n";
                strSql += ",FactuurMaand = @FactuurMaand \n";
                strSql += ",ContractedCapacity = @ContractedCapacity \n";
                strSql += ",MaxConsumption = @MaxConsumption \n";
                strSql += ",Residential = @Residential \n";
                strSql += ",Complex = @Complex \n";
                strSql += ",MarketSegment = @MarketSegment \n";
                strSql += ",Straat = @Straat \n";
                strSql += ",Huisnummer = @Huisnummer \n";
                strSql += ",Toevoeging = @Toevoeging \n";
                strSql += ",Postcode = @Postcode \n";
                strSql += ",Plaats = @Plaats \n";
                strSql += "WHERE HoofdKenmerkId = @HoofdKenmerkId";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@NetbeheerderId", hoofdKenmerk.NetbeheerderId);
                cmd.Parameters.AddWithValue("@NetgebiedId", hoofdKenmerk.NetgebiedId);
                cmd.Parameters.AddWithValue("@PV", hoofdKenmerk.PV);
                cmd.Parameters.AddWithValue("@LV", hoofdKenmerk.LV);
                cmd.Parameters.AddWithValue("@MV", hoofdKenmerk.MV);
                cmd.Parameters.AddWithValue("@Product", hoofdKenmerk.Product);
                cmd.Parameters.AddWithValue("@FactuurMaand", hoofdKenmerk.FactuurMaand);
                cmd.Parameters.AddWithValue("@MaxConsumption", hoofdKenmerk.MaxConsumption);
                cmd.Parameters.AddWithValue("@Residential", hoofdKenmerk.Residential);
                cmd.Parameters.AddWithValue("@Complex", hoofdKenmerk.Complex);
                cmd.Parameters.AddWithValue("@MarketSegment", hoofdKenmerk.MarketSegment);
                cmd.Parameters.AddWithValue("@Straat", hoofdKenmerk.Straat);
                cmd.Parameters.AddWithValue("@Huisnummer", hoofdKenmerk.Huisnummer);
                cmd.Parameters.AddWithValue("@Toevoeging", hoofdKenmerk.Toevoeging);
                cmd.Parameters.AddWithValue("@Postcode", hoofdKenmerk.Postcode);
                cmd.Parameters.AddWithValue("@Plaats", hoofdKenmerk.Plaats);
                cmd.Parameters.AddWithValue("@HoofdKenmerkId", hoofdKenmerk.HoofdKenmerkId);
                cmd.ExecuteNonQuery();
                HoofdKenmerkId = hoofdKenmerk.HoofdKenmerkId;

                return HoofdKenmerkId;

            }

            catch (Exception ex)
            {
                WriteLog("Fout bij update HoofdKenmerk : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }
        }

        private int AanmakenMeter(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                int MeterId = -1;
                String strSql = "INSERT INTO EnergieDB.masterdata.Meter \n";
                strSql += "(MeterNummer \n";
                strSql += ",SlimmeMeter \n";
                strSql += ",Uitleesbaar \n";
                strSql += ",TempCorrectie \n";
                strSql += ",AantalRegisters) \n";
                strSql += "VALUES \n";
                strSql += "(@MeterNummer \n";
                strSql += ",@SlimmeMeter \n";
                strSql += ",@Uitleesbaar \n";
                strSql += ",@TempCorrectie \n";
                strSql += ",@AantalRegisters); SELECT SCOPE_IDENTITY()";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@MeterNummer", meter.MeterNummer);
                cmd.Parameters.AddWithValue("@SlimmeMeter", meter.SlimmeMeter);
                cmd.Parameters.AddWithValue("@Uitleesbaar", meter.Uitleesbaar);
                cmd.Parameters.AddWithValue("@TempCorrectie", meter.TempCorrectie);
                cmd.Parameters.AddWithValue("@AantalRegisters", meter.AantalRegisters);

                Object objResult = cmd.ExecuteScalar();
                if (objResult != null)
                {
                    MeterId = int.Parse(objResult.ToString());
                    meter.MeterId = MeterId;
                }
                else
                {
                    WriteLog("Fout bij aanmaken Meter geen MeterId bepaald", LEVEL_Critical, ref conn, BerichtId);
                    return -1;
                }

                return MeterId;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken Meter : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }
        }

        private int UpdateMeter(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                int MeterId = -1;
                String strSql = "UPDATE EnergieDB.masterdata.Meter \n";
                strSql += "SET MeterNummer = @MeterNummer \n";
                strSql += ",SlimmeMeter = @SlimmeMeter \n";
                strSql += ",Uitleesbaar = @Uitleesbaar \n";
                strSql += ",TempCorrectie = @TempCorrectie \n";
                strSql += ",AantalRegisters = @AantalRegisters \n";
                strSql += "WHERE MeterId = @MeterId";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@MeterNummer", meter.MeterNummer);
                cmd.Parameters.AddWithValue("@SlimmeMeter", meter.SlimmeMeter);
                cmd.Parameters.AddWithValue("@Uitleesbaar", meter.Uitleesbaar);
                cmd.Parameters.AddWithValue("@TempCorrectie", meter.TempCorrectie);
                cmd.Parameters.AddWithValue("@AantalRegisters", meter.AantalRegisters);
                cmd.ExecuteNonQuery();


                return MeterId;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij update Meter : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }
        }

        private Boolean AanmakenRegister(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                String strSql = "INSERT INTO EnergieDB.masterdata.Register \n";
                strSql += "(MeterId \n";
                strSql += ",RegisterId \n";
                strSql += ",TariefType \n";
                strSql += ",LeveringsRichting \n";
                strSql += ",AantalTelwielen \n";
                strSql += ",Factor) \n";
                strSql += "VALUES \n";
                strSql += "(@MeterId \n";
                strSql += ",@RegisterId \n";
                strSql += ",@TariefType \n";
                strSql += ",@LeveringsRichting \n";
                strSql += ",@AantalTelwielen \n";
                strSql += ",@Factor);";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@MeterId", register.MeterId);
                cmd.Parameters.AddWithValue("@RegisterId", register.RegisterId);
                cmd.Parameters.AddWithValue("@TariefType", register.TariefType);
                cmd.Parameters.AddWithValue("@LeveringsRichting", register.LeveringsRichting);
                cmd.Parameters.AddWithValue("@AantalTelwielen", register.AantalTelwielen);
                cmd.Parameters.AddWithValue("@Factor", register.Factor);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij aanmaken Register : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return false;
            }
        }

        private Boolean UpdateRegister(ref SqlConnection conn, int BerichtId)
        {
            try
            {
                String strSql = "UPDATE EnergieDB.masterdata.Register \n";
                strSql += "SET TariefType = @TariefType \n";
                strSql += ", LeveringsRichting = @LeveringsRichting \n";
                strSql += ", AantalTelwielen = @AantalTelwielen \n";
                strSql += ", Factor = @Factor \n";
                strSql += "WHERE MeterId = @MeterId and RegisterId = @RegisterId";
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = strSql;
                cmd.Parameters.AddWithValue("@MeterId", register.MeterId);
                cmd.Parameters.AddWithValue("@RegisterId", register.RegisterId);
                cmd.Parameters.AddWithValue("@TariefType", register.TariefType);
                cmd.Parameters.AddWithValue("@LeveringsRichting", register.LeveringsRichting);
                cmd.Parameters.AddWithValue("@AantalTelwielen", register.AantalTelwielen);
                cmd.Parameters.AddWithValue("@Factor", register.Factor);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Fout bij update Register : " + ex.Message, LEVEL_Critical, ref conn, BerichtId);
                return false;
            }
        }

        private Int16 AanmakenCapacieitsCode(ref SqlConnection conn, int BerichtId, Int64 Ean13)
        {
            Int16 CapaciteitsId = -1;
            String strSql = "INSERT INTO dbo.Capaciteitscodes (EAN13_Code) VALUES (@EAN13_Code); SELECT SCOPE_IDENTITY()";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@EAN13_Code", Ean13);
            Object objResult = cmd.ExecuteScalar();
            if (objResult != null)
            {
                CapaciteitsId = Int16.Parse(objResult.ToString());
            }
            else
            {
                WriteLog("Fout bij aanmaken Meter geen MeterId bepaald", LEVEL_Critical, ref conn, BerichtId);
                return -1;
            }

            return CapaciteitsId;
        }
        private class Bericht
        {
            public int Bericht_ID;
            public Boolean Inkomend;
            public DateTime Datum;
            public String Onderwerp;
            public String strBericht;
            public int Type;
            public string BerichtID_Sender;
            public Boolean Verwerkt;
            public Boolean Fout;
        }
        private class Masterdata
        {
            public Int64 EAN18Code;
            public DateTime StartDatum;
            public DateTime EindDatum;
            public Int32 AansluitingId;
            //public Int32 AdresId;
            public Boolean PVActief;
            public Boolean LVActief;
            public Int32 RelatieId;
            public Int32 PortfolioId;
            public Int32 HoofdKenmerkId;
            public Int32 MeterID;
            public Int32 FysiekeKenmerkID;
            public DateTime VerwerktDT;
            //public Boolean Update;
        }
        private class MasterdataDossier
        {
            public Int64 EAN18Code;
            public DateTime StartDatum;
            public String Dossier;
            public String Referentie;
            public String Reden;
            public Boolean Insert;
            public Boolean Update;
        }
        private class FysiekeKenmerk
        {
            public int FysiekeKenmerkID;
            public Byte BemeteringsType;
            public String Profiel;
            public Int16 CapTarCode;
            public int FysiekeStatusId;
            public Boolean AdminStatusSmartMeter;
            public Boolean LeveringsStatus;
            public Int32 LeveringsRichtingId;
            public Int64 SJVPeak;
            public Int64 SJVOffPeak;
            public int AllocatieMethode;
            public Int64 SJIPeak;
            public Int64 SJIOffPeak;
            public int BusinessType;
            public int PowerSystemType;
            public Boolean Update;
        }
        private class HoofdKenmerk
        {
            public int HoofdKenmerkId;
            public int NetbeheerderId;
            public int NetgebiedId;
            public int PV;
            public int LV;
            public int MV;
            public int Product;
            public Int16 FactuurMaand;
            public int ContractedCapacity;
            public String MaxConsumption;
            public Boolean Residential;
            public Boolean Complex;
            public String MarketSegment;
            public String Straat;
            public String Huisnummer;
            public String Toevoeging;
            public String Postcode;
            public String Plaats;
            public Boolean Update;
        }
        private class Meter
        {
            public int MeterId;
            public string MeterNummer;
            public Boolean SlimmeMeter;
            public Boolean Uitleesbaar;
            public Boolean TempCorrectie;
            public int AantalRegisters;
            public Boolean Insert;
            public Boolean Update;
        }
        private class Register
        {
            public int MeterId;
            public string RegisterId;
            public string TariefType;
            public int LeveringsRichting;
            public int AantalTelwielen;
            public decimal Factor;
            public Boolean Insert;
            //public Boolean Update;
        }
        //private class Relatie
        //{
        //    public int Id;
        //    public string Naam;
        //    public string Korte_Naam;
        //    public string Naamregel1;
        //    public string Naamregel2;
        //    public string Naamregel3;
        //    public DateTime GeboorteDatum;
        //    public string IBAN;
        //    public string Voornaam;
        //    public string Achternaam;
        //    public string Email;
        //    public string Tussenvoegsel;
        //    public string NieuwEmail;
        //    public string Straat;
        //    public string Plaats;
        //    public string Huisnummer;
        //    public string Toevoeging;
        //    public string Postcode;
        //}

    }
}