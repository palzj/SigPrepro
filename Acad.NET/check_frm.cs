using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CDDCICore.threading;
using DCIAUTOCAD;
using Autodesk.AutoCAD.DatabaseServices;
using ElectronicsApprovalSystem.model;
using Autodesk.AutoCAD.ApplicationServices;
using CDDCICore.model;

namespace ElectronicsApprovalSystem.form
{
    public partial class Check_Frm : Form
    {
        List<checkData> errDatLst = new List<checkData>(); // 保存检测结果

        public ObjectIdCollection _pgreenLandIds;   // 公共绿地实体Id
        public ObjectIdCollection _ogreenLandIds;   // 其它绿地实体Id
        public ObjectIdCollection _buildEntIds;     // 建筑实体Id

        private static ThreadHandler th = null;

        //定义代理消息句柄
        public delegate void MessageHandler(MessageEventArgs e);

        //需更新的控件或者其他获取消息数据的方法
        public void Message(MessageEventArgs e)
        {
//             string str = e.Message.ObjId1.ToString() + e.Message.ObjId2.ToString() + e.Message.Distance.ToString() +
//                 e.Message.Type1 + e.Message.Type2 + e.Message.CheckTypeDescription;

            //errDatLst.Add(e.Message);
            string[] contens = {e.Message.ObjId1.ToString(), e.Message.ObjId2.ToString(),
                        e.Message.Type1, e.Message.Type2, e.Message.Distance.ToString(), e.Message.CheckTypeDescription};
            dataGridView1.Rows.Add(contens);

//            richTextBox1.AppendText(/*e.Message*/ str );
        }

        private void _subthread_MessageSend(object sender, MessageEventArgs e)
        {
            //实例化代理
            MessageHandler handler = new MessageHandler(Message);

            //调用Invoke
            this.Invoke(handler, new object[] { e });
        }


        public Check_Frm()
        {
            InitializeComponent();

            _pgreenLandIds = selectTools.getPublicGreenland();
            _ogreenLandIds = selectTools.getOtherGreenLand();
            _buildEntIds = selectTools.getBuildingEnt();

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
//             for (int i = 0; i < 20; i++)
//             {
//                 MyThread _thread = new MyThread(i + 1);
//                 _thread.MessageSend += new MyThread.MessageEventHandler(this._subthread_MessageSend);
//                 th = new ThreadHandler(_thread);
//                 th.Start();
//             }


//             // 公共绿地 <--> 建筑
//             List<Polyline> pgreenLands = selectTools.objIdstoPolyline(_pgreenLandIds);
//             List<Polyline> buildEnts = selectTools.objIdstoPolyline(_buildEntIds);
//             foreach(Polyline pgreenLand in pgreenLands)
//             {
//                 foreach(Polyline buildEnt in buildEnts)
//                 {
//                     MyThread _thread = new MyThread(pgreenLand, buildEnt);
//                     _thread.MessageSend += new MyThread.MessageEventHandler(this._subthread_MessageSend);
//                     th = new ThreadHandler(_thread);
//                     th.Start();
//                 }
//             }


//             foreach (ObjectId objId1 in _pgreenLandIds)
//             {
//                 foreach (ObjectId objId2 in _ogreenLandIds)
//                 {
//                     MyThread _thread = new MyThread(objId1/*.ToString()*/, objId2/*.ToString()*/);
//                     _thread.MessageSend += new MyThread.MessageEventHandler(this._subthread_MessageSend);
//                     th = new ThreadHandler(_thread);
//                     th.Start();
//                 }
//             }


//             // 公共绿地 <--> 建筑
//             foreach(ObjectId objId1 in _pgreenLandIds)
//             {
//                 foreach(ObjectId objId2 in _ogreenLandIds)
//                 {
//                     MyThread _thread = new MyThread(objId1/*.ToString()*/, objId2/*.ToString()*/);
//                     _thread.MessageSend += new MyThread.MessageEventHandler(this._subthread_MessageSend);
//                     th = new ThreadHandler(_thread);
//                     th.Start();
//                 }
//             }
//             



//             // 其它绿地 <--> 建筑
//             foreach (ObjectId objId1 in _ogreenLandIds)
//             {
//                 foreach (ObjectId objId2 in _ogreenLandIds)
//                 {
//                     MyThread _thread = new MyThread(objId1, objId2);
//                     _thread.MessageSend += new MyThread.MessageEventHandler(this._subthread_MessageSend);
//                     th = new ThreadHandler(_thread);
//                     th.Start();
//                 }
//             }
            //             

            #region test1
            // 先完成采样点然后再进行计算
            List<PointsEnty> pgreenLandPntLst = new List<PointsEnty>();   // 公花绿地点集
            List<PointsEnty> ogreenLandPntLst = new List<PointsEnty>();   // 其它绿地点集
            List<PointsEnty> buildPntLst = new List<PointsEnty>();        // 建筑点集

            foreach(ObjectId objId in _pgreenLandIds)
            {
                pgreenLandPntLst.Add(new PointsEnty(objId, 100, true));

            }

            foreach (ObjectId objId in _ogreenLandIds)
            {
                ogreenLandPntLst.Add(new PointsEnty(objId, 100, true));
            }

            foreach (ObjectId objId in _buildEntIds)
            {
                buildPntLst.Add(new PointsEnty(objId, 100, true));
            }




            indicators_info_List indic_info_Lst = new indicators_info_List();
            List<indicators_info> indic_infos = indic_info_Lst.GetIndicatorsInfosOnlyOneList("公共绿地", "检测");


            string sql1 = indic_infos[0].Sqls;
            List<PointsEnty> pntsLst1;
            ObjectIdCollection objIds1 = seleEnts(sql1);
            pntsLst1 = expodeSamplepoint(objIds1);

            string sql2 = indic_infos[1].Sqls;
            List<PointsEnty> pntsLst2;
            ObjectIdCollection objIds2 = seleEnts(sql2);
            pntsLst2 = expodeSamplepoint(objIds2);







            // 公共绿地 <--> 建筑
            foreach (PointsEnty pgreenLandPnt in /*pgreenLandPntLst*/ pntsLst1)
            {
                foreach (PointsEnty buildPnt in /*buildPntLst*/ pntsLst2)
                {
                    MyThread _thread = new MyThread(pgreenLandPnt, buildPnt);
                    _thread.MessageSend += new MyThread.MessageEventHandler(this._subthread_MessageSend);
                    th = new ThreadHandler(_thread);
                    th.Start();
                }
            }
            #endregion


        }


        private ObjectIdCollection seleEnts(string sql)
        {
            ObjectIdCollection objIds = new ObjectIdCollection();

            string fileName = Tools.curDwgNameWithoutExtension();
            EntityGetDataHelper entDatHelper = new EntityGetDataHelper(fileName);
            entDatHelper.Sqls = sql;
            entityTypeObjectList Dats = entDatHelper.getEntityOnlyOneList();

            foreach (EntityTypeObject dat in Dats)
            {
                ObjectId objId = Tools.HandleToObjectId(dat.Entityid.ToString());

                if (false == objIds.Contains(objId))
                {
                    objIds.Add(objId);
                }

            }

            return objIds;
        }

        private List<PointsEnty> expodeSamplepoint(ObjectIdCollection objIds)
        {
            List<PointsEnty> pgreenLandPntLst = new List<PointsEnty>();   // 绿地点集
            foreach (ObjectId objId in objIds)
            {
                pgreenLandPntLst.Add(new PointsEnty(objId, 100, true));
            }

            return pgreenLandPntLst;
        }




        private void toolStripButton2_Click(object sender, EventArgs e)
        {
//             string[] contens = {"a1","a1","a1", "a1", "a1"};
//             dataGridView1.Rows.Add(contens);

            // 从indicators_info中提取条目
            indicators_info_List indic_info_Lst = new indicators_info_List();
            List<indicators_info> indic_infos = indic_info_Lst.GetIndicatorsInfosOnlyOneList("公共绿地", "检测");


            string sql1 = indic_infos[0].Sqls ;
            List<PointsEnty> pntsLst1;
            ObjectIdCollection objIds1 = seleEnts(sql1);
            pntsLst1 = expodeSamplepoint(objIds1);

            string sql2 = indic_infos[1].Sqls;
            List<PointsEnty> pntsLst2;
            ObjectIdCollection objIds2 = seleEnts(sql1);
            pntsLst2 = expodeSamplepoint(objIds1);






//                 // 选择所有的实体Id(已经去掉重复的)
//                 object [] dats = (from dat in Dats
//                             where dat.Key == "id"
//                             select new {dat.Entityid} ).ToArray();
// 

                
        }



        private void Check_Frm_Load(object sender, EventArgs e)
        {
//            dataGridView1.DataSource = errDatLst;
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
//             string strId1 = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
//             string strId2 = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
// 
//             ObjectId objId1 = Tools.HandleToObjectId(strId1);
//             ObjectId objId2 = Tools.HandleToObjectId(strId2);
// 
//             ObjectIdCollection objIds = new ObjectIdCollection();
//             objIds.Add(objId1);
//             objIds.Add(objId2);
// 
// //            Tools.focusEntites(objIds);

          

        }

//         private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
//         {
// 
//         }
    }



    public class checkData
    {
        private ObjectId _objId1;     // 第一个实体的ID
        public Autodesk.AutoCAD.DatabaseServices.ObjectId ObjId1
        {
            get { return _objId1; }
            set { _objId1 = value; }
        }

        private ObjectId _objId2;     // 第二个实体的ID
        public Autodesk.AutoCAD.DatabaseServices.ObjectId ObjId2
        {
            get { return _objId2; }
            set { _objId2 = value; }
        }

        private string _type1;        // 第一个实体的类型
        public string Type1
        {
            get { return _type1; }
            set { _type1 = value; }
        }

        private string _type2;        // 第二个实体的类型
        public string Type2
        {
            get { return _type2; }
            set { _type2 = value; }
        }

        private double _distance;     // 两个实体的距离  
        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        private string _checkTypeDescription;  // 检测的类型描述
        public string CheckTypeDescription
        {
            get { return _checkTypeDescription; }
            set { _checkTypeDescription = value; }
        }

        public checkData()
        {
            _objId1 = ObjectId.Null;
            _objId2 = ObjectId.Null;
            _type1 = "Default";
            _type2 = "Default";
            _distance = -1.0;
            _checkTypeDescription = "Default"; 
        }

    }

    /// <summary>
    /// 消息传递事件类，定义需要传递的消息类型，
    /// 此处是字符，可以根据需求扩展
    /// </summary>
    public class MessageEventArgs : EventArgs
    {

        /*public String Message; //传递字符串信息*/

        public checkData Message; //传递字符串信息

        public MessageEventArgs(/*string*/checkData message)
        {
            this.Message = message;
        }

    }


    /// <summary>
    /// 线程样例
    /// </summary>
    public class MyThread : IThreadClass
    {
        /// <summary>
        /// Internal variable
        /// </summary>

//         private ObjectId /*string*/ _objId1;
//         private ObjectId /*string*/ _objId2;

//         private Polyline _pl1;
//         private Polyline _pl2;

//         private int _cnt;
//         private int _threadCount;

        private PointsEnty _pnte1;
        private PointsEnty _pnte2;

        //定义线程与form传递消息事件
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);

        public event MessageEventHandler MessageSend;

        /*

         * 说明:定义事件处理函数,当然这里也可以不用直接在引发事件时调用this.MessageSend(sender, e);

         * 这里的参数要和事件代理的参数一样

         * */

        public void OnMessageSend(object sender, MessageEventArgs e)
        {
            if (MessageSend != null)
                this.MessageSend(sender, e);

        }

        #region xxxxxx
        /// <summary>
        /// Constructor...
        /// </summary>
        /// <param name="counter"></param>
//         public MyThread(ObjectId objId1, ObjectId objId2)
//         {
//             _objId1 = objId1;
//             _objId2 = objId2;
// /*            _cnt = 5;*/
// /*            _threadCount = counter;*/
//         }

//         public MyThread(int counter)
//         {
//             _cnt = 5;
//             _threadCount = counter;
//         }
//         

//         public MyThread(/*string */ObjectId objId1, /*string*/ ObjectId objId2)
//         {
//             _objId1 = objId1;
//             _objId2 = objId2;
//             /*            _cnt = 5;*/
//             /*            _threadCount = counter;*/
//         }
//         

//         public MyThread(Polyline pl1, Polyline pl2)
//         {
// //             _objId1 = objId1;
// //             _objId2 = objId2;
// //             
//             /*            _cnt = 5;*/
//             /*            _threadCount = counter;*/
// 
//             _pl1 = pl1;
//             _pl2 = pl2;
//         }
//      
  
        #endregion



        #region test1
        public MyThread(PointsEnty p1, PointsEnty p2)
        {
            _pnte1 = p1;
            _pnte2 = p2;
        }

        #endregion








        /// <summary>
        /// This function MUST BE IMPLEMENTED...
        /// </summary>
        /// <param name="arg"></param>
        public override void OnJob(ThreadHandlerEventArgs arg)
        {
            // THE WORKING FUNCTION...
//             while (_cnt > 0)
//             {

//            double dis = GeTools.minDistance(_objId1, _objId2, 50);
//            double dis = 0.5;
//            
//            double dis = GeTools.minDistance(_pl1, _pl2, 50);

            double dis = GeTools.minDistance(_pnte1, _pnte2);

//            string str = _pnte1.ObjId.ToString() + "Distant To" +_pnte2.ObjId.ToString() + "is:" + dis.ToString() + "\n";
            checkData chkDat = new checkData();
            chkDat.ObjId1 = _pnte1.ObjId;
            chkDat.ObjId2 = _pnte2.ObjId;
            chkDat.Type1 = "OBJ1.TYPE";
            chkDat.Type2 = "OBJ2.TYPE";
            chkDat.Distance = dis;
            chkDat.CheckTypeDescription = "Default test";


            this.MessageSend(this, new MessageEventArgs(/*str*/ chkDat ));

//                 this.MessageSend(this, new MessageEventArgs("thread:" + _objId1.ToString() + _objId2.ToString() 
//                     /*+ _threadCount.ToString()*/ + "\n"));

//                this.MessageSend(this, new MessageEventArgs("thread:" + _threadCount.ToString() + "\n"));

//                 _cnt--;
//                 System.Threading.Thread.Sleep(500);
//             }
        }

        // The following functions are only support functions. They must not be overrided.
        public override void OnFinish(ThreadHandlerEventArgs arg)
        {
            base.OnFinish(arg);

//            this.MessageSend(this, new MessageEventArgs("Finish thread:" /*+ _threadCount.ToString()*/ + "\n"));
        }

        public override void OnTerminate(ThreadHandlerEventArgs arg)
        {
            base.OnTerminate(arg);
//            this.MessageSend(this, new MessageEventArgs("Terminate thread:" /*+ _threadCount.ToString()*/ + "\n"));
        }

        public override void OnAbort(ThreadHandlerEventArgs arg)
        {
            base.OnAbort(arg);
//            this.MessageSend(this, new MessageEventArgs("Abort thread:" /*+ _threadCount.ToString()*/ + "\n"));
        }

        public override void OnException(ThreadHandlerExceptionArgs arg)
        {
            // I've commented it out, to avoid re-throwing of the catched exception...
            base.OnException(arg);
//            this.MessageSend(this, new MessageEventArgs("Exception thread:" /*+ _threadCount.ToString()*/ + "\n"));
        }

    }






}
