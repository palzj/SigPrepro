using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using AutoCAD;

namespace CADAutomation
{

    public static class PF
    {
        public static AcadApplication gbl_app;
        public static AcadDocument gbl_doc;
        public static AcadModelSpaceClass gbl_modSpace;
        public static AcadAcCmColor gbl_color;
        public static double gbl_pi = 3.14159;
        //Layer For Donuts
        public static AcadLayer TerminalsLayer;
        public static AcadLayer SwitchLayer;
        //Layer Termination Points
        public static AcadLayer TerminationPoints;

        #region PublicFunctions

        public static void CloseAllInstance()
        {
            Process[] aCAD =
               Process.GetProcessesByName("acad");

            foreach (Process aCADPro in aCAD)
            {
                aCADPro.CloseMainWindow();
            }
        }

 

        public static void CreateAutoCADObject()
        {
            try
            {
                CloseAllInstance();
                gbl_app = new AcadApplication();
                gbl_doc = gbl_app.ActiveDocument;
                gbl_app.Application.Visible = true;
                gbl_modSpace = (AcadModelSpaceClass)gbl_doc.ModelSpace;
                gbl_doc.Linetypes.Load("HIDDEN", "acad.lin");
                gbl_doc.Linetypes.Load("CENTER", "acad.lin");

                //Other Objects Layer

                SwitchLayer = PF.gbl_doc.Layers.Add("Switch110Layer");
                SwitchLayer.color = AutoCAD.AcColor.acGreen;
                PF.gbl_doc.ActiveLayer = SwitchLayer;

                //Layer For Donuts

                TerminalsLayer = PF.gbl_doc.Layers.Add("TerminalsLayer");
                TerminalsLayer.color = AutoCAD.AcColor.acRed;

                //Layer Termination Points

                TerminationPoints = 
                  PF.gbl_doc.Layers.Add("TerminationPoints");
                TerminationPoints.color = AutoCAD.AcColor.acWhite;

 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DrawLine(double StartX1,
                                    double StartY1,
                                    double EndX2,
                                    double EndY2,
                                    string LineType,
                                    bool DrawDonutsOnLineStart,
                                    bool DrawDonutsOnLineEnds)
        {

            AcadLine lineObj;
            double[] startPoint = new double[3];
            double[] endPoint = new double[3]; ;

            startPoint[0] = StartX1;
            startPoint[1] = StartY1;
            startPoint[2] = 0.0;
            endPoint[0] = EndX2;

            endPoint[1] = EndY2;
            endPoint[2] = 0.01;
            lineObj = gbl_doc.ModelSpace.AddLine(startPoint, endPoint);

            if (LineType.Length > 0)
            {
                lineObj.Linetype = LineType; //'"HIDDEN"
                lineObj.LinetypeScale = 10;
                lineObj.Update();
            }

 

            if (DrawDonutsOnLineStart == true)
            {
                DrawDonut((AcadBlock)gbl_doc.ModelSpace, 
                              0, 3.0, StartX1, StartY1);

            }

            if (DrawDonutsOnLineEnds == true)
            {
                DrawDonut((AcadBlock)gbl_doc.ModelSpace, 
                                  0, 3.0, EndX2, EndY2);

            }
            gbl_app.ZoomAll();
        }

        public static void DrawLine(double StartX1,
                                    double StartY1,
                                    double EndX2,
                                    double EndY2)
        {
            DrawLine(StartX1, StartY1, EndX2, EndY2, "", false, false);
        }

        public static void DrawLine(double StartX1,
                                    double StartY1,
                                    double EndX2,
                                    double EndY2,
                                    string LineType)
        {
            DrawLine(StartX1, StartY1, EndX2, EndY2, 
                              LineType, false, false);

        }

        public static void DrawLine(double StartX1,
                                    double StartY1,
                                    double EndX2,
                                    double EndY2,
                                    string LineType,
                                    bool DrawDonutsOnLineStart)

        {
            DrawLine(StartX1, StartY1, EndX2, 
              EndY2, LineType, DrawDonutsOnLineStart, false);
        }

        public static AcadLWPolyline DrawDonut(AcadBlock space,
                                                double inRad,
                                                double outRad,
                                                double cenPt1,
                                                double cenPt2)
        {

            double width, radius, PI;
            double[] tmp = new double[2];
            double[] v = new double[4];
            AcadLWPolyline pl;
            double[] basePnt = new double[3];

            try
            {
                //Switch to terminals layer
                gbl_doc.ActiveLayer = TerminalsLayer;

                basePnt[0] = cenPt1;
                basePnt[1] = cenPt2;
                basePnt[2] = 0.0;
                PI = Math.Atan(1) * 4;
                width = (outRad - inRad) / 2;
                radius = (inRad + width) / 2;
                tmp = (double[])gbl_doc.Utility.PolarPoint(basePnt, 
                                                       PI, radius);

                v[0] = tmp[0];
                v[1] = tmp[1];
                tmp = (double[])gbl_doc.Utility.PolarPoint(basePnt, 
                                                        0, radius);
                v[2] = tmp[0];
                v[3] = tmp[1];
                pl = space.AddLightWeightPolyline(v);

 

                pl.Closed = true;
                pl.SetWidth(0, width, width);
                pl.SetBulge(0, -1);
                pl.SetWidth(1, width, width);
                pl.SetBulge(1, -1);

                //Switch to other layer
                gbl_doc.ActiveLayer = SwitchLayer;               

                return pl;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;

            }
        }

 

        public static void DrawSolid(double StartingXPoint,
                                        double StartingYPoint,
                                        double Length,
                                        double Width)
        {

            AcadSolid solidObj;
            double[] point1 = new double[3];
            double[] point2 = new double[3];
            double[] point3 = new double[3];
            double[] point4 = new double[3];

            //Solid Starts

            point1[0] = StartingXPoint;
            point1[1] = StartingYPoint;
            point1[2] = 0.0;
            point2[0] = StartingXPoint;
            point2[1] = (StartingYPoint) - Width;
            point2[2] = 0.0;

            point3[0] = StartingXPoint + Length;
            point3[1] = StartingYPoint;
            point3[2] = 0.0;
            point4[0] = StartingXPoint + Length;
            point4[1] = (StartingYPoint) - Width;
            point4[2] = 0.0;
            solidObj  = gbl_doc.ModelSpace.AddSolid(point1, 
                                   point2, point3, point4);

            //Solid ENDS
        }

 

        public static void DrawText(double StartingXPoint,
                                    double StartingYPoint,
                                    string textString,
                                    double Height,
                                    double Rotation)
        {
            AcadText textObj;
            double[] insertionPoint = new double[3];

            insertionPoint[0] = StartingXPoint;
            insertionPoint[1] = StartingYPoint;
            insertionPoint[2] = 0.0;
            textObj = gbl_doc.ModelSpace.AddText(textString, 
                                    insertionPoint, Height);
            textObj.Alignment = AcAlignment.acAlignmentLeft;
            textObj.Backward = false;
            textObj.Rotation = Rotation;
        }

        public static void DrawText(double StartingXPoint,
                            double StartingYPoint,
                            string textString)
        {
            DrawText(StartingXPoint, StartingYPoint, textString, 3, 0);
        }

 

        public static void AddText(double StartingXPoint,
                                    double StartingYPoint,
                                    string textString,
                                    double Height)
        {
            DrawText(StartingXPoint, StartingYPoint, textString, Height, 0);
        }

 

 

        public static void DrawCircle(double StartingXPoint,
                                    double StartingYPoint,
                                    double Radius)

        {
            AcadCircle circleObj;
            double[] centerPoint = new double[3];

            centerPoint[0] = StartingXPoint;
            centerPoint[1] = StartingYPoint;
            centerPoint[2] = 0.0;
            circleObj = gbl_doc.ModelSpace.AddCircle(centerPoint, Radius);
        }

 

        public static void DrawArc(double StartingXPoint,
                                   double StartingYPoint,
                                   double Radius)

        {
            //For Drawing Arc
            AcadArc arcObj;
            AcadCircle circleObj;
            double[] centerPoint = new double[3];
            double startAngleInDegree;
            double endAngleInDegree;
            double startAngleInRadian;
            double endAngleInRadian;

 

            //Draw Arc
            centerPoint[0] = StartingXPoint;
            centerPoint[1] = StartingYPoint;
            startAngleInDegree = 175.0;
            endAngleInDegree = 5.0;
            startAngleInRadian = startAngleInDegree * 3.141592 / 180.0;
            endAngleInRadian = endAngleInDegree * 3.141592 / 180.0;
            arcObj = gbl_doc.ModelSpace.AddArc(centerPoint, Radius,
                startAngleInRadian, endAngleInRadian);

        }

 

        public static void DrawTerminationPoint(double StartingXPoint,
                                    double StartingYPoint)
        {
            gbl_doc.ActiveLayer = TerminationPoints;
            DrawCircle(StartingXPoint, StartingYPoint, 1.8);
            DrawLine(StartingXPoint - 2.5, StartingYPoint - 4.0,
               (StartingXPoint - 2.5) + 4.0, 
               (StartingYPoint - 4.0) + 7.0);

           gbl_doc.ActiveLayer = SwitchLayer;
        }
        #endregion
    }
}