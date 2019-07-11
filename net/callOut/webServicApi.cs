
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace net.callOut
{
      class webServicApi
    {
        
         //是否为static
       public  static string xx = "";
        static hsqzfWebService.GetCaseList caselist = new hsqzfWebService.GetCaseList();
        public static List<Evaluation>  getXml2Evaluation() {
            List<Evaluation> le = new List<Evaluation>();
            string GetCListString =xx;
          
            try { 
         GetCListString= caselist.GetCList();
         
           if (string.IsNullOrEmpty(GetCListString.Trim()))
               return le;
           tools.log.Debug("GetCListString：读取了一次" + GetCListString);
           le=analyzeCallOutJson(GetCListString);



          
        }catch(Exception e){
            tools.log.Debug("getXml2Evaluation异常："+e.ToString());
        }
            
            return le;
        }


          public static string  morenumber2Onenumber(string phone){
              string temp=phone;

              if (!temp.Contains(","))
                  return temp;
              tools.log.Debug("包含有多个电话号码：" + temp);
              string[] sArray = temp.Split(',');
              foreach (string i in sArray) {
                  if (!string.IsNullOrEmpty(i)&&i.Length>0&&i.Length<15)
                  {

                      tools.log.Debug("包含有多个电话号码：" + phone+"使用号码为："+i);
                  return i;
                  }
              }
              return temp;
          }
        public static List<Evaluation> analyzeCallOutJson(string jsonString)
        {
            List<Evaluation>  analyzele = new List<Evaluation>();
            Evaluation e;
            try
            {
                List<jsonHsqzfEvaluation> les = JsonConvert.DeserializeObject<List<jsonHsqzfEvaluation>>(jsonString);
                
               
                

                 int st = 0;
                 foreach (jsonHsqzfEvaluation item in les)
                {

                   
                    e = new Evaluation();
                    //更新成功后才能写入数据库
                    st = 0;
                    st = caselist.UpState(item.案件记录号.Trim());
                  
                    if (st > 0)
                    {
                        e.name = item.市民姓名.Trim();
                        e.taskID = item.案件记录号.Trim();
                        e.department = item.主办部门.Trim();
                        e.accidentAddress = item.详细地址.Trim();
                        e.context = item.案件类型名称.Trim();
                        e.dateTime = item.处理时间.Trim();
                        e.recordTime = item.录入时间.Trim();
                        string temp = item.市民电话;
                        temp = morenumber2Onenumber(temp);
                        e.phoneNumber = "9"+temp;
                        if (!"027".Equals(callerLocal.getAreaCode(temp)))
                        {
                            e.phoneNumber = "90" + temp;
                            tools.log.Debug("长途号码：" + e.toString());
                        }
                        tools.log.Debug("获取到xml键值为：" + item.市民姓名 + "案件号：" + item.案件记录号 + "更新的状态为：" + st+"信息：" + e.toString());
                        analyzele.Add(e); 
                    }
                    
                }
            }
            catch (Exception e1) {
                tools.log.Debug("analyzeCallOutJson转换json异常：返回的状态" + e1.ToString());
            }
            
            return analyzele;
        }

      

        public static int returnResult(string taskId,string result) {
            int state=0;
            try
            {
                state=caselist.AddMydContent(taskId, result);
                tools.log.Debug("returnResultSuccess 任务id：" + taskId+"结果:"+result);
            }
            catch (Exception e) {
                tools.log.Debug("returnResult数据异常：" + e.ToString());
            }
            return state;
        }

    }
}
