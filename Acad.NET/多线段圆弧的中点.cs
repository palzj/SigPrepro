//  C#取得多段线中圆弧中点坐标
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
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n调试程序命令LV");//初始化操作

        }
        public void Terminate()
        {
            //清除操作
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
                    PromptEntityResult per = ed.GetEntity("请选择多段线");
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
                                    //方法一，比较笨的方法。
                                    double len0 = PL.GetDistAtPoint(PL.GetPoint3dAt(i));
                                    double len1 = PL.GetDistAtPoint(PL.GetPoint3dAt(i + 1));
                                    double midlen = (len0 + len1) / 2;

                                    ed.WriteMessage("\n第二种方法计算的圆弧中点是：" + midP3d.ToString());

                                    //方法二，但是速度比较慢好像，因为如果加上下面的代码，运行速度明显慢，前面是感觉不出来的，后面的要停顿一下，可能有异常
                                    Point3d midL = PL.GetPointAtParameter(i + 0.5);
                                    ed.WriteMessage("\n第二种方法计算的圆弧中点是：" + midL.ToString());
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