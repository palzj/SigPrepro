�򵥽����£�Ŀǰ������ע������3��
    1.����������KHDM���������������е�ı�׼������Ҫ�����ñ�עĳ���㣬����Ҫ�ܶ��䣬Ҳ����Ҫ�ܶ�����ʽ��
    2.������������KED��������ǵ߸��˴�ͳ�Ļ�ͼϰ�ߣ�ѡ������һ����ͷ���Ϳ��������������������ע��λ�ã������׳Ƶ���ק���ܣ����������㣬
	  �ǳ��ķ���ֱ�ۣ����Ҳ�����ص㣬�ص�����ֻ��Ҫ��ק�޸�����һ����������Ǵ���·��������������ͬ��λ�ĵط��ı�߶��޸��ˡ�
    3.������������KE�����ͬ�ϣ�ɾ������������һ����ߣ�����·�����ö�����������ͬ��λ�����ݾ�ɾ���ˡ�

    ���ȵ�Ȼ��ѡ����ص����ݡ���ק���ܵ�ʵ�֡����Ӧ�á���չ������صȵȡ�
	����������CAD���ο����Ļ������ݡ�д�������֮ǰ������һ��ͼԪ�����ᣬ����д�����������������˸��������˽⡣
	
-----------------�����вο��ˡ�AutoCAD VBA & VB.NET����������ʵ���̡̳��д��롣using System;
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
        string GroupName;//����
        string GroupQ;//ǰ�������
        int GroupNum;//��ĸ�������0��ʼ
        int PLCONum;//��ǰ����ߵĶ������
        ObjectId layerNow;
        public void GetandSet(Database db, Editor ed, ObjectId[] TextId)
        {
            //�ȵõ������εĿ�
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectId[] plinesjxId = GetSsetId(ed, "LWPOLYLINE", "shuju1", "0");//�õ���ͷѡ��ID
                Autodesk.AutoCAD.DatabaseServices.Polyline entsj = (Autodesk.AutoCAD.DatabaseServices.Polyline)trans.GetObject(plinesjxId[0], OpenMode.ForRead);//�õ��˶����ͼԪ
                sjxK = entsj.StartPoint.Y - entsj.EndPoint.Y;//�õ����ߵĸ߶�
                //�õ����ֵ�������ʽ���߶�
                DBText entText = (DBText)trans.GetObject(TextId[0], OpenMode.ForRead);//�õ�������ͼԪ
                textH = entText.Height;//���ָ߶�
                textsTyle = entText.TextStyleName;//�õ�������ʽ
                textW = entText.WidthFactor;
            }
            CreateLayer(db);//������ͼ��
        }
        public void CreateLayer(Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                ObjectId layerId;
                layerNow=db.Clayer;
                if (lt.Has("������ע") == false)
                {
                    LayerTableRecord ltr = new LayerTableRecord();


                    ltr.Name = "������ע";
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


            ObjectId[] plineId= GetSsetId(ed,"POLYLINE","sjx");//�õ������ѡ��ID
            if (plineId[0] == ObjectId.Null)
            {
                Application.ShowAlertDialog("��ǰ�ĵ���û�к������Ϣ�����ȴ򿪺����ͼ");
                return;
            }
            else
            {
                //ed.WriteMessage("��ʼ��ע�����̣߳����Ժ�");
                CrossEdit(db,ed,plineId);
            }


        }
        public void CrossEdit(Database db,Editor ed,ObjectId[] plineId)
        {
            //�õ�����׮�ţ���չ���ݣ�
            ArrayList sjxAL = ObjectIdToXdata(db, plineId);


            ObjectId[] lineId = GetSsetId(ed, "LINE", "zhix");//�õ�������ѡ��ID����
            //�õ�����׮�ţ���չ���ݣ�
            ArrayList zhixAL = ObjectIdToXdata(db, lineId);


            ObjectId[] TextId = GetSsetId(ed, "TEXT", "shuju1");//�õ����ĸ߳�ѡ��ID����
            //�õ�����׮�ţ���չ���ݣ�
            ArrayList shuju1AL = ObjectIdToXdata(db, TextId);


            GetandSet(db, ed, TextId);//��ȡ����һЩ��Ϣ


            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < plineId.Length; i++)
                {
                    Point3dCollection oneCollection = new Point3dCollection();
                    oneCollection = GetPOLYLINEPoint2dCollection(trans, plineId[i]);//�õ�ȥ���ظ����Ķ���߶��㼯��
                    //ȥ����������е���λ��ĩλ���ǲ���Ҫ��ע��
                    //int kaka=oneCollection.Count;
                    oneCollection.RemoveAt(oneCollection.Count);
                    oneCollection.RemoveAt(0);


                    //���õ�ǰ�飨������ǰ�������������������ɡ�1.�������2.�����飬���ﶨ������飩����ǰ����߶������
                    //�������ǰ��
                    if (i == 0)//���Ϊ��1��
                    {
                        GroupNum = 0;
                        PLCONum = oneCollection.Count;//��һ��Ķ���߶����������
                    }


                    else
                    {
                        if (PLCONum != oneCollection.Count)//���PLCONum�����ڵ�ǰ�Ķ���������Ǳ�������һ�ֶ��濪ʼ�ˣ���Ҫ����PLCONum���½�һ����
                        {
                            PLCONum = oneCollection.Count;
                            GroupNum = GroupNum + 1;
                        }
                    }
                    GroupQ = "Group" + GroupNum;


                    //����Ӧ����չ����׮��Ϊ��StrZhuang
                    var StrZhuang = sjxAL[i];
                    //���Ҵ�׮����zhixAL�е�λ��
                    int IndexZ = zhixAL.IndexOf(StrZhuang);
                    //�͵õ���ӦͼԪ��ID,��lineId��
                    //�õ����ǵĽ���
                    Entity ent = (Entity)trans.GetObject(plineId[i], OpenMode.ForRead);//�õ��˶����ͼԪ
                    Entity ent1 = (Entity)trans.GetObject(lineId[i], OpenMode.ForRead);//�õ���ֱ��ͼԪ


                    Point3dCollection ints = new Point3dCollection();
                    ent1.IntersectWith(ent, Intersect.OnBothOperands, new Plane(), ints, 0, 0);
                    //double xx = ints[0].X; double yy = ints[0].Y; double zz = ints[0].Z;//�õ��Ľ���
                    //���Ҵ˽����ڶ�����ϵ�λ��
                    int zhongxin = oneCollection.IndexOf(ints[0]);
                    //�õ���׼���Yֵ
                    double BaseG = oneCollection[zhongxin].Y;
                    //���Ҵ�׮�Ž��㴦�ĸ߳�
                    DBText entText = (DBText)trans.GetObject(TextId[i], OpenMode.ForRead);//�õ�������ͼԪ
                    double gaochen = double.Parse(entText.TextString);//�ַ���ת��Ϊdouble����




                    //�õ��˶�����ϸ����ʵ�ʸ̣߳�����Ϊ���ĵ�ĸ̣߳��ڶ�����ϸ����е�λ����zhongxin��
                    //����ͼ


                    for (int j = 0; j < oneCollection.Count; j++)
                    {
                        GroupName = GroupQ + j;
                        //�õ���һ����ĸ߳�
                        //double tempG = System.Math.Round(oneCollection[j].Y - BaseG  +gaochen, 3);//����3λС����������Ϊ0ʱ,��ȥ�����0
                        string tempG = string.Format("{0:0.000}", oneCollection[j].Y - BaseG + gaochen);//����3λС����������3λ
                        //string tempStr = tempG.ToString();


                        //����ͷ������д�ֻ���
                        Point3d temppt = new Point3d();
                        ObjectId Plineid = GetSj(sjxK, oneCollection[j], out temppt);
                        AddXData(Plineid, StrZhuang);
                        //Point3d temppt = GetSj(sjxK, oneCollection[j]);
                        //���������д��
                        ObjectId textid = ModelSpace.AddText(temppt, tempG.ToString(), textH, 0, 0, textW);
                        AddXData(textid, StrZhuang);
                        //�������
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
            Application.ShowAlertDialog("��ע��ɣ�");


        }
        public void AddXData(ObjectId entid,Object Xdata)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //��ȡ��ǰ���ݿ��ע��Ӧ�ó����
                RegAppTable reg = (RegAppTable)trans.GetObject(db.RegAppTableId, OpenMode.ForWrite);
                //���û����Ϊ"ʵ����չ����"��ע��Ӧ�ó�����¼����
                if (!reg.Has("KAKANIMO��չ����"))
                {
                    //����һ��ע��Ӧ�ó�����¼������ʾ��չ����
                    RegAppTableRecord app = new RegAppTableRecord();
                    //������չ���ݵ�����
                    app.Name = "KAKANIMO��չ����";
                    //��ע��Ӧ�ó���������չ����
                    reg.Add(app);
                    trans.AddNewlyCreatedDBObject(app, true);
                }
                 
                //������չ���ݵ�����
                Entity ent = (Entity)trans.GetObject(entid, OpenMode.ForRead);//�õ�ͼԪ
                ResultBuffer rb = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, "KAKANIMO��չ����"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, Xdata));
                //���½�����չ���ݸ��ӵ���ѡ���ʵ����
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
                //�����ֵ��������ؼ���ΪgroupName�����������ҵ��򷵻�����ObjectId
                ObjectId gpid=new ObjectId();
                try
                {
                    gpid = dict.GetAt(groupName);
                }
                catch
                {
                    if (gpid == ObjectId.Null)//��������ھ��½�
                    {
                        //�½�һ�������
                        Group gp = new Group(groupName,false);
                        //�򿪵�ǰ���ݿ�����ֵ�����Լ����½��������
                        DBDictionary dictG = (DBDictionary)trans.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);
                        //�����ֵ��н��������Ϊһ������Ŀ���룬��ָ�����������ؼ���ΪgroupName
                        dictG.SetAt(groupName, gp);
                    }
                }
                
                trans.Commit();
            }
        }