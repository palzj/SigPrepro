//  C#ȡ�ö������Բ���е�����
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
[assembly: ExtensionApplication(typeof(ClassLibrary.Lab8Class))]
[assembly: CommandClass(typeof(ClassLibrary.Lab8Class))]

namespace ClassLibrary
{
    public class Lab8Class : IExtensionApplication
    {
        public void Initialize()
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n���Գ�������LV");//��ʼ������

        }
        public void Terminate()
        {
            //�������
        }

        public Lab8Class()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        Database db = Autodesk.AutoCAD.DatabaseServices.HostApplicationServices.WorkingDatabase;
        Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = Autodesk.AutoCAD.DatabaseServices.HostApplicationServices.WorkingDatabase.TransactionManager;
        // Define Command "AsdkCmd1"
        [CommandMethod("LV")]
        public void getPlPoint()
        {
            try
            {
                Transaction trans = tm.StartTransaction();
                //    BlockTableRecord btr;
                //    BlockTable bt;
                using (trans)
                {
                    PromptEntityResult per = ed.GetEntity("��ѡ������");
                    if (per.Status == PromptStatus.OK)
                    {
                        DBObject obj = trans.GetObject(per.ObjectId, OpenMode.ForRead);
                        {
                            Polyline PL = obj as Polyline;
                            int vn = PL.NumberOfVertices;
                            for (int i = 0; i <= vn; i++)
                            {
                                Point3d pt3d = PL.GetPoint3dAt(i - 1);
                                double vBulge = PL.GetBulgeAt(i);
                                if (vBulge != 0)
                                {
                                    //����һ���Ƚϱ��ķ�����
                                    double len0 = PL.GetDistAtPoint(PL.GetPoint3dAt(i));
                                    double len1 = PL.GetDistAtPoint(PL.GetPoint3dAt(i + 1));
                                    double midlen = (len0 + len1) / 2;

                                    ed.WriteMessage("\n�ڶ��ַ��������Բ���е��ǣ�" + midP3d.ToString());

                                    //�������������ٶȱȽ���������Ϊ�����������Ĵ��룬�����ٶ���������ǰ���Ǹо��������ģ������Ҫͣ��һ�£��������쳣
                                    Point3d midL = PL.GetPointAtParameter(i + 0.5);
                                    ed.WriteMessage("\n�ڶ��ַ��������Բ���е��ǣ�" + midL.ToString());
                                }
                            }
                        }
                    }
                    trans.Commit();
                }
            }
            catch
            {
                ;
            }
            finally
            {
                ;
            }
        }

    }
}