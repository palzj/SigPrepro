?/*
 * ��SharpDevelop������
 * �û��� Administrator
 * ����: 2011-12-29
 * ʱ��: 14:50
 * 
 * Ҫ�ı�����ģ������ ����|ѡ��|�����д|�༭��׼ͷ�ļ�
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
            //====���ĵ�ǰCADͼ����������,�Ա��ڽ��б༭
            // DocumentLock doclock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument();
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {

                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                // ��� PickFirst ѡ��    Get the PickFirst selection set              
                PromptSelectionResult acSSPrompt;
                acSSPrompt = ed.SelectImplied();
                SelectionSet acSSet;
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    acSSet = acSSPrompt.Value;
                }
                else
                {
                    // ��� PickFirst ѡ��    Clear the PickFirst selection set
                    ObjectId[] idarrayEmpty = new ObjectId[0];
                    ed.SetImpliedSelection(idarrayEmpty);

                    // Ҫ����ͼ��������ѡ�����    Request for objects to be selected in the drawing area
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
        //***********************************˵��
        //��POLYLINE���в���,�ҳ��Ϸ����߼��·�����
        public void PLINCLOR(Polyline PL)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                BlockTable acBlkTbl;
                acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // ��д��ʽ��ģ�Ϳռ����¼   Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                if (Clockwise(PL) == 1)//��ת����Ϊ˳ʱ�뷽������
                {
                    PL.ReverseCurve();
                }
                Point3d pone = ed.GetPoint("�����Ϻ����һ��׼���ϵĵ�").Value;
                double ds = PL.GetParameterAtPoint(pone);
                int iiy = Convert.ToInt32(Math.Floor(ds));//�������һ���ϵĵ� 
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

        //************************˳ʱ�뷵��-1,��ʱ�뷵��1************************
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