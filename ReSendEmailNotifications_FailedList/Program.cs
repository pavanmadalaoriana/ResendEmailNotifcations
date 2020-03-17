using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace ReSendEmailNotifications_FailedList
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        static string sqlConnectionString;
        static Database objDB;

        static void Main(string[] args)
        {
            bool sentYN = false;
            var result = string.Empty;
            int emailID = 0;
            try
            {
                DataSet dsFailedList = ReSendEmailNotificationsFailedList();

                if (dsFailedList != null && dsFailedList.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsFailedList.Tables[0].Rows.Count; i++)
                    {
                        emailID = Convert.ToInt32(dsFailedList.Tables[0].Rows[i]["email_id"].ToString());
                        string email_notifcation_type = dsFailedList.Tables[0].Rows[i]["email_notifcation_type"].ToString();
                        string email_from = dsFailedList.Tables[0].Rows[i]["email_from"].ToString();
                        string email_to = dsFailedList.Tables[0].Rows[i]["email_to"].ToString();
                        string email_cc = dsFailedList.Tables[0].Rows[i]["email_cc"].ToString();
                        string email_bcc = dsFailedList.Tables[0].Rows[i]["email_bcc"].ToString();
                        string email_subject = dsFailedList.Tables[0].Rows[i]["email_subject"].ToString();
                        string email_body = dsFailedList.Tables[0].Rows[i]["email_body"].ToString();

                        var client = new SmtpClient();
                        client.Send(email_from, email_to, email_subject, email_body);
                        result = "success";

                        if (result == "success")
                        {
                            sentYN = true;
                            EmailNotificationStatusUpdate(emailID, sentYN);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                if (emailID > 0)
                    EmailNotificationStatusUpdate(emailID, sentYN);

                //throw;
            }
        }

        static DataSet ReSendEmailNotificationsFailedList()
        {
            sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            DataSet dsEmailFailedList = new DataSet();

            objDB = new SqlDatabase(sqlConnectionString);
            using (DbCommand objCMD = objDB.GetStoredProcCommand("usp_EMR5_EmailNotification_Failed_list"))
            {
                try
                {
                    dsEmailFailedList = objDB.ExecuteDataSet(objCMD);
                    if (dsEmailFailedList != null && dsEmailFailedList.Tables[0].Rows.Count > 0)
                    {
                        
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    throw;
                }
            }
            return dsEmailFailedList;
        }

        static void EmailNotificationStatusUpdate(int emailID, bool sentYN)
        {
            objDB = new SqlDatabase(sqlConnectionString);
            try
            {
                using (DbCommand objCMD1 = objDB.GetStoredProcCommand("usp_EMR5_EmailNotification_email_status_update"))
                {
                    objDB.AddInParameter(objCMD1, "@email_id", DbType.Int32, emailID > 0 ? emailID : (object)DBNull.Value);
                    objDB.AddInParameter(objCMD1, "@email_sentyn", DbType.Boolean, sentYN);
                    objDB.ExecuteNonQuery(objCMD1);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }
    }
}
