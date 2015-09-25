简单介绍下：目前横断面标注操作分3步
    1.命令行输入KHDM，完成设计线上所有点的标准（不需要再设置标注某个点，不需要管段落，也不需要管断面形式）
    2.在命令行输入KED，这里可是颠覆了传统的绘图习惯，选中任意一个箭头，就可以上下左右任意调整标注的位置，就是俗称的拖拽功能，所见即所点，
	  非常的方便直观，这个也不是重点，重点是你只需要拖拽修改任意一个，结果就是此种路幅段落中其他相同点位的地方的标高都修改了。
    3.在命令行输入KE，结果同上，删除段落内任意一个标高，此种路幅布置段落中其他相同点位的数据就删除了。

    首先当然是选择集相关的内容、拖拽功能的实现、组的应用、扩展数据相关等等。
	基本涵盖了CAD二次开发的基本内容。写这个程序之前连创建一个图元都不会，现在写完对这个方法流程有了个基本的了解。
	
-----------------代码中参考了《AutoCAD VBA & VB.NET开发基础与实例教程》中代码。using System;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DNA;
using System.Collections;
using myModelSpace;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Threading;


namespace CrossSectionalEdit
{
    public class Class1
    {
        double sjxK;
        double textH;
        double textW;
        string textsTyle;
        string GroupName;//组名
        string GroupQ;//前半个组名
        int GroupNum;//组的个数，从0开始
        int PLCONum;//当前多段线的顶点个数
        ObjectId layerNow;
        public void GetandSet(Database db, Editor ed, ObjectId[] TextId)
        {
            //先得到三角形的宽
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectId[] plinesjxId = GetSsetId(ed, "LWPOLYLINE", "shuju1", "0");//得到箭头选择集ID
                Autodesk.AutoCAD.DatabaseServices.Polyline entsj = (Autodesk.AutoCAD.DatabaseServices.Polyline)trans.GetObject(plinesjxId[0], OpenMode.ForRead);//得到此多段线图元
                sjxK = entsj.StartPoint.Y - entsj.EndPoint.Y;//得到引线的高度
                //得到文字的字体样式及高度
                DBText entText = (DBText)trans.GetObject(TextId[0], OpenMode.ForRead);//得到此文字图元
                textH = entText.Height;//文字高度
                textsTyle = entText.TextStyleName;//得到文字样式
                textW = entText.WidthFactor;
            }
            CreateLayer(db);//创建新图层
        }
        public void CreateLayer(Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                ObjectId layerId;
                layerNow=db.Clayer;
                if (lt.Has("横断面标注") == false)
                {
                    LayerTableRecord ltr = new LayerTableRecord();


                    ltr.Name = "横断面标注";
                    Color layerColor = Color.FromColorIndex(ColorMethod.ByColor, 120);
                    ltr.Color = layerColor;
                    layerId = lt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    db.Clayer = layerId;
                }
                trans.Commit();
            }
        }


        [CommandMethod("KHDM")]
        public void testSelection2()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            ObjectId[] plineId= GetSsetId(ed,"POLYLINE","sjx");//得到多段线选择集ID
            if (plineId[0] == ObjectId.Null)
            {
                Application.ShowAlertDialog("当前文档中没有横断面信息！请先打开横断面图");
                return;
            }
            else
            {
                //ed.WriteMessage("开始标注横断面高程，请稍候！");
                CrossEdit(db,ed,plineId);
            }


        }
        public void CrossEdit(Database db,Editor ed,ObjectId[] plineId)
        {
            //得到它的桩号（扩展数据）
            ArrayList sjxAL = ObjectIdToXdata(db, plineId);


            ObjectId[] lineId = GetSsetId(ed, "LINE", "zhix");//得到中心线选择集ID集合
            //得到它的桩号（扩展数据）
            ArrayList zhixAL = ObjectIdToXdata(db, lineId);


            ObjectId[] TextId = GetSsetId(ed, "TEXT", "shuju1");//得到中心高程选择集ID集合
            //得到它的桩号（扩展数据）
            ArrayList shuju1AL = ObjectIdToXdata(db, TextId);


            GetandSet(db, ed, TextId);//读取设置一些信息


            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < plineId.Length; i++)
                {
                    Point3dCollection oneCollection = new Point3dCollection();
                    oneCollection = GetPOLYLINEPoint2dCollection(trans, plineId[i]);//得到去掉重复点后的多段线顶点集合
                    //去掉这个集合中的首位和末位，是不需要标注的
                    //int kaka=oneCollection.Count;
                    oneCollection.RemoveAt(oneCollection.Count);
                    oneCollection.RemoveAt(0);


                    //设置当前组（祖名的前半个，组名有两部分组成。1.多段线组2.顶点组，这里定多段线组）及当前多段线顶点个数
                    //设置组的前半
                    if (i == 0)//如果为第1个
                    {
                        GroupNum = 0;
                        PLCONum = oneCollection.Count;//第一组的多段线顶点个数，当
                    }


                    else
                    {
                        if (PLCONum != oneCollection.Count)//如果PLCONum不等于当前的顶点个数，那表明另外一种断面开始了，需要跟新PLCONum及新建一个组
                        {
                            PLCONum = oneCollection.Count;
                            GroupNum = GroupNum + 1;
                        }
                    }
                    GroupQ = "Group" + GroupNum;


                    //它对应的扩展数据桩号为：StrZhuang
                    var StrZhuang = sjxAL[i];
                    //查找次桩号在zhixAL中的位置
                    int IndexZ = zhixAL.IndexOf(StrZhuang);
                    //就得到对应图元的ID,在lineId中
                    //得到他们的交点
                    Entity ent = (Entity)trans.GetObject(plineId[i], OpenMode.ForRead);//得到此多段线图元
                    Entity ent1 = (Entity)trans.GetObject(lineId[i], OpenMode.ForRead);//得到此直线图元


                    Point3dCollection ints = new Point3dCollection();
                    ent1.IntersectWith(ent, Intersect.OnBothOperands, new Plane(), ints, 0, 0);
                    //double xx = ints[0].X; double yy = ints[0].Y; double zz = ints[0].Z;//得到的交点
                    //查找此交点在多段线上的位置
                    int zhongxin = oneCollection.IndexOf(ints[0]);
                    //得到基准点的Y值
                    double BaseG = oneCollection[zhongxin].Y;
                    //查找此桩号交点处的高程
                    DBText entText = (DBText)trans.GetObject(TextId[i], OpenMode.ForRead);//得到此文字图元
                    double gaochen = double.Parse(entText.TextString);//字符串转换为double类型




                    //得到此多段线上各点的实际高程（上面为中心点的高程，在多段线上各点中的位置是zhongxin）
                    //并绘图


                    for (int j = 0; j < oneCollection.Count; j++)
                    {
                        GroupName = GroupQ + j;
                        //得到第一个点的高程
                        //double tempG = System.Math.Round(oneCollection[j].Y - BaseG  +gaochen, 3);//保留3位小数，当后面为0时,会去掉这个0
                        string tempG = string.Format("{0:0.000}", oneCollection[j].Y - BaseG + gaochen);//保留3位小数，必须有3位
                        //string tempStr = tempG.ToString();


                        //画箭头并返回写字基点
                        Point3d temppt = new Point3d();
                        ObjectId Plineid = GetSj(sjxK, oneCollection[j], out temppt);
                        AddXData(Plineid, StrZhuang);
                        //Point3d temppt = GetSj(sjxK, oneCollection[j]);
                        //以这个基点写字
                        ObjectId textid = ModelSpace.AddText(temppt, tempG.ToString(), textH, 0, 0, textW);
                        AddXData(textid, StrZhuang);
                        //分组加入
                        ObjectIdCollection ids = new ObjectIdCollection();
                        ids.Add(Plineid);
                        ids.Add(textid);
                        createGroup(GroupName);
                        ModelSpace.AppendEntityGroup(GroupName, ids);
                    }
                }
                db.Clayer = layerNow;
                trans.Commit();
            }
            Application.ShowAlertDialog("标注完成！");


        }
        public void AddXData(ObjectId entid,Object Xdata)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获取当前数据库的注册应用程序表
                RegAppTable reg = (RegAppTable)trans.GetObject(db.RegAppTableId, OpenMode.ForWrite);
                //如果没有名为"实体扩展数据"的注册应用程序表记录，则
                if (!reg.Has("KAKANIMO扩展数据"))
                {
                    //创建一个注册应用程序表记录用来表示扩展数据
                    RegAppTableRecord app = new RegAppTableRecord();
                    //设置扩展数据的名字
                    app.Name = "KAKANIMO扩展数据";
                    //在注册应用程序表加入扩展数据
                    reg.Add(app);
                    trans.AddNewlyCreatedDBObject(app, true);
                }
                 
                //设置扩展数据的内容
                Entity ent = (Entity)trans.GetObject(entid, OpenMode.ForRead);//得到图元
                ResultBuffer rb = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, "KAKANIMO扩展数据"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, Xdata));
                //将新建的扩展数据附加到所选择的实体中
                ent.XData = rb;
                trans.Commit();
            }
        }
        private void createGroup(string groupName)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary dict = (DBDictionary)trans.GetObject(db.GroupDictionaryId, OpenMode.ForRead);
                //在组字典中搜索关键字为groupName的组对象，如果找到则返回它的ObjectId
                ObjectId gpid=new ObjectId();
                try
                {
                    gpid = dict.GetAt(groupName);
                }
                catch
                {
                    if (gpid == ObjectId.Null)//如果不存在就新建
                    {
                        //新建一个组对象
                        Group gp = new Group(groupName,false);
                        //打开当前数据库的组字典对象以加入新建的组对象
                        DBDictionary dictG = (DBDictionary)trans.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);
                        //在组字典中将组对象作为一个新条目加入，并指定它的搜索关键字为groupName
                        dictG.SetAt(groupName, gp);
                    }
                }
                
                trans.Commit();
            }
        }