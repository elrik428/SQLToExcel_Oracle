using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
//using System.Linq;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;// ODP.NET Oracle managed provider
using Oracle.ManagedDataAccess.Types;
using SMTPClass;



namespace WeeklyTids_CSV
{
    public partial class Program
    {

        //public static string OutputDirectory = @"\\GRAT1-FPS-SV2O\Common\Users\PDS_TMS_Reports\";
        public static string OutputDirectory = @"C:\Users\lnestoras\Desktop\";
        //public static string timestamp = DateTime.Now.ToShortDateString();
        public static string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        public static string ErrorFile = "";
        public static int ErrorCount = 0;
        public static OracleConnection SHARPconn = null;
        public static string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + OutputDirectory + "SHARP.xlsx;Extended Properties='Excel 12.0 Xml;HDR=YES;';";

        public static string mailBody_1, mailBody, subjectMail;
        public static string emailTo_1, emailTo, emailCC, emailBCC;
        public static string Path;


        static void Main(string[] args)
        {

            Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Path = Path.Substring(0, Path.LastIndexOf('\\') + 1);
            //emailTo = "GR-POS-Developers-Dl@euronetworldwide.com";
            emailTo_1 = "lnestoras@euronetworldwide.com";
            emailTo = "lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com";
            emailCC = "lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com;lnestoras@euronetworldwide.com";
            subjectMail = "ΠΡΟΣΦΟΡΑ: Urgent αρχείο για τα TID από τα TMS Πειραιώς και Εuronet (epos) ";

            ConnectSHARPdb();
            if (SHARPconn.State != ConnectionState.Open)
            {
                Console.WriteLine("ERROR: Unable to connect to SHARP.");
                return;
            }

            // Delete xls file from loaction
            System.IO.File.Delete(@"C:\Users\lnestoras\Desktop\SHARP.xlsx");

            string sqlSelQry =
                "select  distinct TERMINAL.TER_TID as TID, " +
"TERMINAL.TER_MID as MID, " +
"TERMINAL.TER_MER_NAME as DIAKRITIKOS_TITLOS, " +
"TERMINAL.TER_MER_NAME2  as DIEFTHINSI, " +
"TERMINAL.TER_MER_ADDRESS as POLI, " +
"TERMINAL.TER_MER_CITY as TILEFONO, " +
"case substr(TERMINAL.ter_funcs,6,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as DOSEIS, " +
"'N/A' AS PLITHOS_DOSEON, "+
"case TERMINAL.TER_MESSGL when 'GREN' then 'Greek' else 'English' end as GLOSSA,                                                                        " +
"case substr(TERM_CARD_TYPE_RANGE.TCR_OPTIONS,8,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as PLIKTROLOGISI, " +
"case substr(TERMINAL.ter_flags,7,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as CVV, " +
"case substr(TERMINAL.ter_funcs,7,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as PROEGKRISI, " +
"case substr(TERMINAL.ter_funcs,8,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as COMPLETION, " +
"case substr(TERMINAL.ter_flags,9,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as DCC, " +
"case t1.TCT_CARD_TYPE when 29 then 'YES' else 'NO' end as CUP, " +
"case t2.TCT_CARD_TYPE when 30 then 'YES' else 'NO' end as KARTA_SYMVOLAIAKIS, " +
"case substr(TERMINAL.TER_DL_CHANNEL,1,2) when 'X0' then 'Ethernet' when 'I0' then 'Dialup' when 'G0' then 'GPRS' else 'N/A' end as SINDESI,  " +
"'N/A' as TYPOS_KARTAS_SIM,                                                                                                                             " +
"'N/A' as AYTOMATI_APOSTOLI_PAKETOY,                                                                                                                    " +
"'YES' as CONTACTLESS,                                                                                                                                  " +
"'N/A' as FOROKARTA,                                                                                                                                   " +
"TERMINAL.TER_START_PARAM_DL as LAST_PARAMETER_CALL, " +
"case TERMINAL.TER_ECR when 1 then 'YES' when 0 then 'NO' else 'N/A' end as SYNDESI_TAMEIAKIS, " +
"case substr(TERMINAL.ter_funcs,3,1) when '1' then 'YES' when '0' then 'NO' else 'N/A' end as TIP, " +
"'NO' as SYNDESI_PINPAD, " +
"TERM_STAT_ACT.TSA_VERSION as EKDOSI_EFARMOGIS, " +
"'IWL_220' as MONTELO_TERMATIKOU, " +
"'Ingenico' as KATASKEVASTIS, " +
"case (TERM_LOYALTY_PBG_PAR.PBG_LOYALTY_PBG) when 'TRUE' then 'YES' when 'FALSE' then 'NO' else 'N/A' end as LOYALTY, " +
"TERMINAL.TER_CRE_AT as BIRTH_DATE " +
"from (((((TERMINAL left join TERM_CARD_TYPE_RANGE ON TERMINAL.TER_TID=TERM_CARD_TYPE_RANGE.TCR_TERMINAL AND TERM_CARD_TYPE_RANGE.TCR_CARD_TYPE=1) " +
"left join TERM_LOYALTY_PBG_PAR ON TERM_LOYALTY_PBG_PAR.PBG_TERMINAL=TERMINAL.TER_TID) " +
"left join TERM_CARD_TYPE t1 ON t1.TCT_TERMINAL=TERMINAL.TER_TID AND t1.TCT_CARD_TYPE=29) " +
"left join TERM_CARD_TYPE t2 ON t2.TCT_TERMINAL=TERMINAL.TER_TID AND t2.TCT_CARD_TYPE=30) " +
"left join TERM_STAT_ACT ON TERM_STAT_ACT.TSA_TERMINAL=TERMINAL.TER_TID) " +
"where TERMINAL.TER_CLUSTER IN (" +
"303, " +
"304," +
"305," +
"306," +
"307," +
"308," +
"309)" +
" and TERMINAL.TER_STATUS=1 " +
"order by TERMINAL.TER_TID ";         

            OracleCommand sqlcmdSel_Excel = new OracleCommand();
            sqlcmdSel_Excel.Connection = SHARPconn;
            //sqlcmdSel_Excel.CommandTimeout = 0;
            sqlcmdSel_Excel.CommandType = CommandType.Text;
            sqlcmdSel_Excel.CommandText = sqlSelQry;
            OracleDataReader rdrSelExcel = null;
            //OracleDataReader rdrSelExcel = sqlcmdSel_Excel.ExecuteReader();
            rdrSelExcel = sqlcmdSel_Excel.ExecuteReader();

            OleDbCommand dbCmd = new OleDbCommand("CREATE TABLE [SHARPreport] ([TID] char(255),[MID] char(255),[ΔΙΑΚΡΙΤΙΚΟΣ ΤΙΤΛΟΣ] char(255),[ΔΙΕΥΘΥΝΣΗ] char(255),[ΠΟΛΗ] char(255),[ΤΗΛΕΦΩΝΟ] char(255), " +
 " [ΔΟΣΕΙΣ] char(255),[ΠΛΗΘΟΣ ΔΟΣΕΩΝ] char(255),[ΓΛΩΣΣΑ] char(255),[ΠΛΗΚΤΡΟΛΟΓΗΣΗ] char(255),[CVV] char(255),[ΠΡΟΕΓΚΡΙΣΗ] char(255),[COMPLETION] char(255), " +
 " [DCC] char(255),[CUP] char(255),[ΚΑΡΤΑ ΣΥΜΒΟΛΑΙΑΚΗΣ] char(255),[ΣΥΝΔΕΣΗ]	char(255),[ΤΥΠΟΣ ΚΑΡΤΑΣ SIM] char(255),[ΑΥΤΟΜΑΤΗ ΑΠΟΣΤΟΛΗ ΠΑΚΕΤΟΥ] char(255),[CONTACTLESS] char(255), " +
 " [ΦΟΡΟΚΑΡΤΑ] char(255),[LAST PARAMETER CALL] char(255),[ΣΥΝΔΕΣΗ ΤΑΜΕΙΑΚΗΣ] char(255),[TIP] char(255),[ΣΥΝΔΕΣΗ PINPAD] char(255),[ΕΚΔΟΣΗ ΕΦΑΡΜΟΓΗΣ] char(255),[ΜΟΝΤΕΛΟ ΤΕΡΜΑΤΙΚΟΥ] char(255), " +
 " [ΚΑΤΑΣΚΕΥΑΣΤΗΣ] char(255),[LOYALTY] char(255),[BIRTH DATE] char(255) )");

            //OracleConnection myConnection = new OracleConnection(connectionString);
            OleDbConnection myConnection = new OleDbConnection(connectionString);
            myConnection.Open();
            dbCmd.Connection = myConnection;
            dbCmd.ExecuteNonQuery();

            while (rdrSelExcel.Read())
            {
                OleDbCommand myCommand = new OleDbCommand("Insert into [SHARPreport] ([TID],[MID],[ΔΙΑΚΡΙΤΙΚΟΣ ΤΙΤΛΟΣ],[ΔΙΕΥΘΥΝΣΗ],[ΠΟΛΗ],[ΤΗΛΕΦΩΝΟ], " +
               " [ΔΟΣΕΙΣ],[ΠΛΗΘΟΣ ΔΟΣΕΩΝ],[ΓΛΩΣΣΑ],[ΠΛΗΚΤΡΟΛΟΓΗΣΗ],[CVV],[ΠΡΟΕΓΚΡΙΣΗ],[COMPLETION],[DCC],[CUP], " +
               " [ΚΑΡΤΑ ΣΥΜΒΟΛΑΙΑΚΗΣ],[ΣΥΝΔΕΣΗ],[ΤΥΠΟΣ ΚΑΡΤΑΣ SIM],[ΑΥΤΟΜΑΤΗ ΑΠΟΣΤΟΛΗ ΠΑΚΕΤΟΥ],[CONTACTLESS],[ΦΟΡΟΚΑΡΤΑ], " +
               " [LAST PARAMETER CALL],[ΣΥΝΔΕΣΗ ΤΑΜΕΙΑΚΗΣ],[TIP],[ΣΥΝΔΕΣΗ PINPAD], " +
               " [ΕΚΔΟΣΗ ΕΦΑΡΜΟΓΗΣ],[ΜΟΝΤΕΛΟ ΤΕΡΜΑΤΙΚΟΥ],[ΚΑΤΑΣΚΕΥΑΣΤΗΣ],[LOYALTY],[BIRTH DATE] ) " +
               " values ('" + rdrSelExcel.GetValue(0).ToString() + "','" + rdrSelExcel.GetValue(1).ToString() + "','" + rdrSelExcel.GetValue(2).ToString().Replace('\'', ' ') + "','" + rdrSelExcel.GetValue(3).ToString().Replace('\'', ' ') + "','"
                + rdrSelExcel.GetValue(4).ToString().Replace('\'', ' ') + "','" + rdrSelExcel.GetValue(5).ToString().Replace('\'', ' ') + "','" + rdrSelExcel.GetValue(6).ToString() + "','" + rdrSelExcel.GetValue(7).ToString() + "','"
                + rdrSelExcel.GetValue(8).ToString() + "','" + rdrSelExcel.GetValue(9).ToString() + "','" + rdrSelExcel.GetValue(10).ToString() + "','" + rdrSelExcel.GetValue(11).ToString() + "','"
                + rdrSelExcel.GetValue(12).ToString() + "','" + rdrSelExcel.GetValue(13).ToString() + "','" + rdrSelExcel.GetValue(14).ToString() + "','" + rdrSelExcel.GetValue(15).ToString() + "','"
                + rdrSelExcel.GetValue(16).ToString() + "','" + rdrSelExcel.GetValue(17).ToString() + "','" + rdrSelExcel.GetValue(18).ToString() + "','" + rdrSelExcel.GetValue(19).ToString() + "','"
                + rdrSelExcel.GetValue(20).ToString() + "','" + rdrSelExcel.GetValue(21).ToString() + "','" + rdrSelExcel.GetValue(22).ToString() + "','" + rdrSelExcel.GetValue(23).ToString() + "','"
                + rdrSelExcel.GetValue(24).ToString() + "','" + rdrSelExcel.GetValue(25).ToString() + "','" + rdrSelExcel.GetValue(26).ToString() + "','" + rdrSelExcel.GetValue(27).ToString() + "','"
                + rdrSelExcel.GetValue(28).ToString() + "','" + rdrSelExcel.GetValue(29).ToString() + "')");

                myCommand.Connection = myConnection;
                myCommand.ExecuteNonQuery();

            }
                       
            Console.WriteLine("Export finished");
            Console.Write('\r');
            //Console.ReadLine();
            myConnection.Close();
            dbCmd.Dispose();
            rdrSelExcel.Close();
            SHARPconn.Close();

            mailBody_1 += "Weekly SHARP TIDs successfully exported";
            mailBody += "Dear all, "+ "\n" +
                        "Please resend files from same location, " + @"\\GRAT1-FkPS-SV2O\Common\Users\PDS_TMS_Reports. " + "\n" +
                        "Thank you. ";

            MailClass.SendMail(Path, "SHARP Weekly TIDs", mailBody_1, emailTo_1, "", "");
            MailClass.SendMail(Path, subjectMail, mailBody, emailTo, emailCC, "");

        }

        static void ConnectSHARPdb()
        {

            /************CONNECT**************/
            string oradb = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = hubp1-stm-db1p.commonpr.eeft.com)(PORT = 1521))" +
        "(CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = tmsdb.commonpr.eeft.com)));User Id=eutms2;Password=EUTMS;";//pira prod

            /************CONNECT**************/
            for (int i = 0; i < 3; i++)
            {
                try
                {

                    SHARPconn = new OracleConnection(oradb);  // C#
                    SHARPconn.Open();
                    if (SHARPconn.State == ConnectionState.Open)
                        i = 3;
                }
                catch (Exception exc)
                {



                }

            }
        }

    }
}

