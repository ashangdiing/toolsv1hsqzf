using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace net.callOut
{
    class callerLocal
    {
        public static string getAreaCode(string mobileNumber){
            mobileNumber = mobileNumber.Substring(0,7);
           
            string areaCode = "027"; SqlDataReader EvaluationSDR;
          
            SqlConnection connection = null;
            SqlCommand command = null;
            //  connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);

            try
            {
                connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                //  command.Parameters.Add(new SqlParameter("begin", begin));
                command.Parameters.Add(new SqlParameter("@mobileNumber", mobileNumber+"%"));
                // command.CommandText = "SELECT TOP 100 ID,FILENAME FROM [Esunnet].[dbo].[Record]  where RECDATE>DATEADD(ss,-1200,GETDATE()) and STATUS=0 order by RECDATE  desc";

                command.CommandText = "SELECT * FROM [Esunnet].[dbo].[CallerLocal] where mobileAreanumber like @mobileNumber";
             //   tools.log.Debug("获取手机：_______" + command.CommandText);
                EvaluationSDR = command.ExecuteReader();
                if (!EvaluationSDR.HasRows) { return areaCode; }
                    
                while (EvaluationSDR.Read())
                {
                    
                    areaCode = EvaluationSDR["postCode"].ToString();
                    //tools.log.Debug("获取手机：" + mobileNumber);
                }
              //  tools.log.Debug("获取手机：" + command.CommandText);
            }
            catch (Exception ex)
            {
                tools.log.Debug("获取手机号失败："+ex.ToString());
                return null;
            }
            finally
            {
                if (command != null)
                    command.Dispose();
                if (connection != null) connection.Dispose();
            } 


        return areaCode;
        }



    }
}
