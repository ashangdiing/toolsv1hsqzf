using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace net.callOut
{
    class callOutThread
    {
        SqlDataReader EvaluationSDR; 
        public static Thread callOutTask;public static string IVRUrl;
        creatVxmlFile cvf;
        public static string getIVRUrl()
        {
            if (IVRUrl == null)
                 IVRUrl = ConfigurationManager.AppSettings["IVRUrl"];
            return IVRUrl;
        }
        public callOutThread() {
            getIVRUrl();
            if (callOutTask == null) {
               
            callOutTask = new Thread(startTask);
            callOutTask.IsBackground = true;
            callOutTask.Start();
        }
            
        }
        public void startTask(){
            while (true)
            {

                client2webService();

                //不获取其他接口直接使用
                creatEvaluationFile();
                removeEvaluationFile();
                updateTimeoutEvaluation();
                System.Threading.Thread.Sleep(PingTools.PingTools.sleepTime);
                if (PingTools.PingTools.stopApp)
                {
                    callOutTask.Abort();
                    callOutTask = null;
                    break;
                }
            }
        }

        public void creatEvaluationFile(){
            List<Evaluation> le;
            le=readEvaluation();
            if (le == null || le.Count == 0)
                return;
            foreach(Evaluation e1 in le){
                
                cvf = new creatVxmlFile();
                cvf.e = e1;
                if ("success" == cvf.creatFile())
                {
                    updateEvaluation(e1.taskID, "1");
                   // write2CallOut(e1);
                }
            }
        }
        public void removeEvaluationFile()
        {
            List<Evaluation> le;
           le = readEvaluation("removeFile");
           if (le == null || le.Count == 0)
               return;
           foreach (Evaluation e1 in le)
           {

               cvf = new creatVxmlFile();
               cvf.e = e1;
               cvf.removeFile();
               updateEvaluation(e1.taskID,"-1");
           }
        }

        public List<Evaluation> readEvaluation() {
            return readEvaluation("creatFile");
        }
        public List<Evaluation> readEvaluationResult()
        {
            return readEvaluation("getEvaluationResult");
        }
        public List<Evaluation> readEvaluation(string Types)
        {
            List<Evaluation> le;
            //   test();
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
                //  command.Parameters.Add(new SqlParameter("end", end));
                // command.CommandText = "SELECT TOP 100 ID,FILENAME FROM [Esunnet].[dbo].[Record]  where RECDATE>DATEADD(ss,-1200,GETDATE()) and STATUS=0 order by RECDATE  desc";
                if (Types == "creatFile")
                    command.CommandText = "SELECT TOP 100 * FROM  [Evaluation]  where  state=0  order by dateTime ";
                else if (Types == "removeFile")
                    command.CommandText = "SELECT TOP 100 * FROM  [Evaluation]  where  state=-2  order by dateTime";
                else if (Types == "getEvaluationResult")
                    command.CommandText = "SELECT TOP 100 * FROM  [Evaluation]  where  state=1   and evaluationResult is not null order by dateTime";
                else return null;
                EvaluationSDR = command.ExecuteReader();
                if (!EvaluationSDR.HasRows) { return null; }
                     le=new List<Evaluation>();
                     Evaluation e;
                while (EvaluationSDR.Read())
                {
                    if (!EvaluationSDR.HasRows) { return null; }
                    e = new Evaluation();
                    e.name = EvaluationSDR["name"].ToString();
                    e.taskID = EvaluationSDR["TaskID"].ToString();
                    e.department = EvaluationSDR["TaskID"].ToString();
                    e.dateTime = EvaluationSDR["dateTime"].ToString();
                    e.lastUpdateTime = EvaluationSDR["lastUpdateTime"].ToString();
                    e.state = EvaluationSDR["state"].ToString();
                    e.phoneNumber = EvaluationSDR["phoneNumber"].ToString();
                    e.accidentAddress = EvaluationSDR["accidentAddress"].ToString();
                    e.evaluationResult = EvaluationSDR["evaluationResult"].ToString();
                    e.context = EvaluationSDR["context"].ToString();
                    
                    le.Add(e);
                }

            }
            catch (Exception ex)
            {
                tools.log.Debug("读取结果异常："+ex.ToString());
                return null;
            }
            finally
            {
                if (command != null)
                    command.Dispose();
                if (connection != null) connection.Dispose();
            } return le;
        }



        public void updateEvaluation(string taskID, string state)
        {
            SqlConnection connection = null;
            SqlCommand command = null;
            try
            {
                //   SqlConnection connection = new SqlConnection("server= 127.0.0.1;uid=sa;pwd=esun5005;database=crmrun");
                connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("taskID", taskID));
                command.Parameters.Add(new SqlParameter("state", state));
                command.CommandText = " update  Evaluation  set  state=@state,lastUpdateTime=GETDATE() where taskID=@taskID";
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                tools.log.Debug("更新TASKID：" + taskID + "状态" + state + "不成功");
                tools.log.Debug(ex.ToString());
                
            }
            finally
            {
                if (command != null)
                    command.Dispose();
                if (connection != null) connection.Dispose();
                
            }
        }



        public void updateTimeoutEvaluation()
        {
            SqlConnection connection = null;
            SqlCommand command = null;
            try
            {
                //   SqlConnection connection = new SqlConnection("server= 127.0.0.1;uid=sa;pwd=esun5005;database=crmrun");
                connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;

                command.CommandText = " update  [dbo].[Evaluation]  set evaluationResult=76,lastUpdateTime=getdate() where DATEADD(mi,-60*12,getdate())>lastUpdateTime and evaluationResult is null ";
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                tools.log.Debug("超时更新出错");
                tools.log.Debug(ex.ToString());

            }
            finally
            {
                if (command != null)
                    command.Dispose();
                if (connection != null) connection.Dispose();

            }
        }

        public void client2webService(){
            List<Evaluation> le=webServicApi.getXml2Evaluation();
            if (le != null)
            {
                foreach (Evaluation e1 in le)
                {
                    write2Evaluation(e1);
                }
            }


            le=readEvaluationResult();
            if (le != null) {
                foreach (Evaluation e1 in le)
                {
                    if (webServicApi.returnResult(e1.taskID, e1.evaluationResult) > 0)
                        updateEvaluation(e1.taskID,"-2");
                }
            }

        }

        public int write2Evaluation(Evaluation e)
        {

            int x = 0;
            SqlConnection connection = null;
            SqlCommand command = null;
            try
            {
                //   SqlConnection connection = new SqlConnection("server= 127.0.0.1;uid=sa;pwd=esun5005;database=crmrun");
                connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("taskID", e.taskID));
                command.Parameters.Add(new SqlParameter("name", e.name));
                command.Parameters.Add(new SqlParameter("dateTime", e.dateTime));
                command.Parameters.Add(new SqlParameter("department",e.department));
                command.Parameters.Add(new SqlParameter("accidentAddress", e.accidentAddress));
                command.Parameters.Add(new SqlParameter("context", e.context));
                command.Parameters.Add(new SqlParameter("phoneNumber", e.phoneNumber));
                command.CommandText = "INSERT INTO [dbo].[Evaluation]([taskID],[name],[dateTime],[department],[accidentAddress],[state] ,[lastUpdateTime],[context],[phoneNumber])VALUES"+
                    "  (@taskID,@name,@dateTime,@department,@accidentAddress,0 ,GETDATE(),@context,@phoneNumber)";


                x = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                tools.log.Debug("TASKID：" + e.taskID + "write2Evaluation插入不成功" + IVRUrl +"sql:"+command.CommandText+"参数："+command.Parameters);
                tools.log.Debug(ex.ToString());


            }
            finally
            {
                if (command != null)
                    command.Dispose();
                if (connection != null) connection.Dispose();

            }
            return x;
        }

        public int write2CallOut(Evaluation e) {
            
            int x=0;
            SqlConnection connection = null;
            SqlCommand command = null;
            try
            {
                //   SqlConnection connection = new SqlConnection("server= 127.0.0.1;uid=sa;pwd=esun5005;database=crmrun");
                connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("taskID", e.taskID));
                command.Parameters.Add(new SqlParameter("phone", e.phoneNumber));
                command.Parameters.Add(new SqlParameter("ivr", e.phoneNumber));
                command.Parameters.Add(new SqlParameter("url", IVRUrl.Trim() + "/callout"+ e.taskID.Trim() + ".html"));

                command.CommandText = " INSERT INTO  [Esunnet].[dbo].[TS_CALLOUT](TS_TASK_ID ,TS_TYPE,TS_CALLED ,TS_URL,TS_START_TIME ,TS_SCHEDULE_TIME" +
          ",TS_EXPIRED_TIME ,TS_TIMES  ,TS_BUSY_INTERVAL ,TS_NOANSWER_INTERVAL ,TS_OTHER_INTERVAL  ,TS_ALREADY_TIMES  ,TS_STATUS  ,TS_HOST)"
          + "VALUES(@taskID ,'test' ,@phone ,@url ,getdate() ,getdate(),'2020-01-01'" +
          " ,1 ,1 ,1 ,1 ,0 ,0,'')";
              x= command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                tools.log.Debug("TASKID：" + e.taskID + "插入ivr不成功" + IVRUrl);
                tools.log.Debug(ex.ToString());

            }
            finally
            {
                if (command != null)
                    command.Dispose();
                if (connection != null) connection.Dispose();
                 
            }
            return x;
        }

    }
}
