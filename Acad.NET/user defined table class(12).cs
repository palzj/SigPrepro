//  I am trying to get my C#.NET Plugin to draw a table in AutoCAD 
//  with information based on a .NET form the users fills out. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace WindowsDoors.NET
{
    class OpeningDataTable : Table
    {
        private int rowCount = 0;
        private static Document doc = Application.DocumentManager.MdiActiveDocument; //Current drawing
        private static Database db = doc.Database; //subclass of Document, 
        private static Editor ed = doc.Editor; //Editor object to ask user where table goes, subclass of Document

        public OpeningDataTable(bool isWindow)
        {
            PromptPointResult pr = ed.GetPoint("\nEnter table insertion point: ");
            if (pr.Status == PromptStatus.OK)
            {
                //Setting information about the table
                TableStyle = db.Tablestyle;
                SetSize(2, 5);
                SetRowHeight(3);
                SetColumnWidth(15);
                Position = pr.Value;

                //Creating titles to add
                String[] columnTitles = new String[5];
                columnTitles[0] = "Mark";
                columnTitles[1] = "Width";
                columnTitles[2] = "Height";
                columnTitles[3] = "Header\nMaterial";
                columnTitles[4] = "Packers\n(Each Side)";

                //Adding titles to table
                addRow(columnTitles);
            }
        }

        public void addRow(String[] data)
        {
            // Use a nested loop to format each cell
            for (int i = 0; i < data.Length; i++)
            {
                ParseOption s = new ParseOption();
                Cells[rowCount, i].TextHeight = 1;
                Cells[rowCount, i].SetValue(data[i], s);
            }
            GenerateLayout();


            Transaction tr = doc.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                btr.AppendEntity(this);
                tr.AddNewlyCreatedDBObject(this, true);
                tr.Commit();
            }
        }
    }
}