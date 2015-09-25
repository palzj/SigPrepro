?/*
 * 由SharpDevelop创建。
 * 用户： Administrator
 * 日期: 2011-12-29
 * 时间: 14:50
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;
using DNA;
using Autodesk;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;

namespace CADdrawing2
{
    /// <summary>
    /// Description of PLINE.
    /// </summary>
    public class PLINE
    {

        [CommandMethod("SSY", CommandFlags.UsePickSet)]
        public void addlinescale()
        {
            //====对文当前CAD图档进行锁定,以便于进行编辑
            // DocumentLock doclock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {

                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                // 获得 PickFirst 选择集    Get the PickFirst selection set              
                PromptSelectionResult acSSPrompt;
                acSSPrompt = ed.SelectImplied();
                SelectionSet acSSet;
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    acSSet = acSSPrompt.Value;
                }
                else
                {
                    // 清除 PickFirst 选择集    Clear the PickFirst selection set
                    ObjectId[] idarrayEmpty = new ObjectId[0];
                    ed.SetImpliedSelection(idarrayEmpty);

                    // 要求在图形区域中选择对象    Request for objects to be selected in the drawing area
                    acSSPrompt = ed.GetSelection();
                    acSSet = acSSPrompt.Value;
                }
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    ObjectId[] ID = acSSet.GetObjectIds();
                    foreach (ObjectId id in ID)
                    {
                        Entity ent = (Entity)trans.GetObject(id, OpenMode.ForWrite);

                        if (ent.GetType().ToString() == "Autodesk.AutoCAD.DatabaseServices.Polyline")
                        {
                            ed.WriteMessage(ent.GetType().ToString());
                            Polyline PLONE = (Polyline)trans.GetObject(id, OpenMode.ForRead);
                            PLINCLOR(PLONE);
                        }
                    }
                }
                trans.Commit();

            }
        }
        //***********************************说明
        //对POLYLINE进行操作,找出上方的线及下方的线
        public void PLINCLOR(Polyline PL)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                BlockTable acBlkTbl;
                acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                if (Clockwise(PL) == 1)//反转曲线为顺时针方向走向
                {
                    PL.ReverseCurve();
                }
                Point3d pone = ed.GetPoint("给出料厚方向的一基准段上的点").Value;
                double ds = PL.GetParameterAtPoint(pone);
                int iiy = Convert.ToInt32(Math.Floor(ds));//点的是哪一段上的点 
                ed.WriteMessage(iiy.ToString());
                Polyline pll = new Polyline();
                int ii = 0;
                for (int i = iiy + 1; i != iiy; ++i)
                {
                    Point2d pt1 = PL.GetPoint2dAt(i);
                    double bu = PL.GetBulgeAt(i);
                    pll.AddVertexAt(ii, pt1, bu, 0, 0);
                    ii = ii + 1;
                    if (i == PL.NumberOfVertices - 1)
                    {
                        i = -1;
                    }
                }
                pll.AddVertexAt(PL.NumberOfVertices - 1, PL.GetPoint2dAt(iiy), PL.GetBulgeAt(iiy), 0, 0);
                pll.SetEndWidthAt(PL.NumberOfVertices - 1, 0.025);
                pll.SetStartWidthAt(PL.NumberOfVertices - 1, 0.025);
                pll.SetEndWidthAt((PL.NumberOfVertices - 1)/2, 0.025);
                pll.SetStartWidthAt((PL.NumberOfVertices - 1)/2, 0.025);
                acBlkTblRec.AppendEntity(pll);
                trans.AddNewlyCreatedDBObject(pll, true);
                pll.Closed = true;
                PL.Erase(true);
                trans.Commit();
            }
        }

        //************************顺时针返回-1,逆时针返回1************************
        public int Clockwise(Polyline pline)
        {
            Polyline pline1 = (Polyline)pline.Clone();
            double bulge0 = pline1.GetBulgeAt(0);
            double area0 = pline1.Area;
            if (bulge0 == 0.0)
            {
                pline1.SetBulgeAt(0, 0.5);
                double area1 = pline1.Area;
                if (area1 > area0)
                    return 1;
                else
                    return -1;
            }
            else
            {
                pline1.SetBulgeAt(0, 0);
                double area1 = pline1.Area;
                if (bulge0 > 0)
                {
                    if (area1 > area0)
                        return -1;
                    else
                        return 1;
                }
                else
                {
                    if (area1 > area0)
                        return 1;
                    else
                        return -1;
                }
            }
        }
    }
}