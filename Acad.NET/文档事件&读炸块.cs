using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;

using cadapp = Autodesk.AutoCAD.ApplicationServices;
[assembly: ExtensionApplication(typeof(CADBlockBrowse.CAD))]
[assembly: CommandClass(typeof(CADBlockBrowse.CAD))]

namespace CADBlockBrowse
{
    public class CAD : IExtensionApplication
    {
        private cadapp.Document CurDoc = cadapp.Application.DocumentManager.MdiActiveDocument;
        private Database CurDB = cadapp.Application.DocumentManager.MdiActiveDocument.Database;
        private Editor ed = cadapp.Application.DocumentManager.MdiActiveDocument.Editor;
        private PaletteSet ps = new PaletteSet("块选择器");
        private Hashtable BlockName_Count = new Hashtable();
        private UserControl1 mycontrol = new UserControl1();
        public CAD() { }

        /// <summary>        
        /// 读取当前活动文档的所有块名、数量到Hashtable中        
        /// </summary>        
        /// <returns>返回Hashtable</returns>        
        public static Hashtable ReloadBlockNames()
        {
            Hashtable hs = new Hashtable();
            Database db = cadapp.Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                foreach (ObjectId obj in btr)
                {
                    Entity ent = (Entity)trans.GetObject(obj, OpenMode.ForRead);
                    if (ent.GetType() == typeof(BlockReference))
                    {
                        BlockReference brf = ent as BlockReference;
                        if (hs.ContainsKey(brf.Name))
                        {
                            int oc = Convert.ToInt32(hs[brf.Name]);
                            hs[brf.Name] = oc + 1;
                        }
                        else
                        {
                            hs.Add(brf.Name, 1);
                        }
                    }
                }
            }
            return hs;
        }

        /// <summary>        
        /// 炸开所有块       
        /// </summary>        
        /// <param name="b_name">块名称</param>        
        public static void BlockSelectAndExplode(string b_name)
        {
            cadapp.Document doc = cadapp.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction transaction = db.TransactionManager.StartTransaction();
            using (transaction)
            {
                Entity entity = null;
                DBObjectCollection EntityCollection = new DBObjectCollection();
                BlockTable bt = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                cadapp.DocumentLock documentLock = doc.LockDocument();
                foreach (ObjectId id in btr)
                {
                    entity = (Entity)transaction.GetObject(id, OpenMode.ForWrite);
                    if (entity is BlockReference)
                    {
                        BlockReference br = (BlockReference)entity;
                        if (br.Name == b_name)
                        {
                            entity.Explode(EntityCollection);
                            entity.UpgradeOpen();
                            entity.Erase();
                        }
                    }
                }

                AddEntityCollection(EntityCollection, doc);
                transaction.Commit();
                documentLock.Dispose();
            }
        }

        private static void AddEntityCollection(DBObjectCollection dbos, cadapp.Document doc)
        {
            Database db = doc.Database;
            using (Transaction tran = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)tran.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                foreach (DBObject obj in dbos)
                {
                    Entity ent = (Entity)obj;
                    btr.AppendEntity(ent);
                    tran.AddNewlyCreatedDBObject(ent, true);
                }
                tran.Commit();
            }
        }

        public void Initialize()
        {
            ed.WriteMessage("\nInitialize函数中运行\n");
            ps.Style = PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowPropertiesMenu;
            //ps.Dock = DockSides.None;            
            ps.DockEnabled = DockSides.Left | DockSides.Right | DockSides.None;
            ps.MinimumSize = new System.Drawing.Size(150, 100);
            ps.Size = new System.Drawing.Size(150, 100);
            ps.Add("Palette1", mycontrol);
            ps.Visible = true;

            //注册文档切换事件            
            cadapp.Application.DocumentManager.DocumentActivated += new cadapp.DocumentCollectionEventHandler(DocumentManager_DocumentActivated);
            //注册文档打开、创建事件            
            cadapp.Application.DocumentManager.DocumentCreated += new cadapp.DocumentCollectionEventHandler(DocumentManager_DocumentCreated);
            //注册文档对象删除事件           
            cadapp.Application.DocumentManager.MdiActiveDocument.Database.ObjectErased += new ObjectErasedEventHandler(Database_ObjectErased);
            //注册文档对象创建事件            
            //cadapp.Application.DocumentManager.MdiActiveDocument.Database.ObjectAppended += new ObjectEventHandler(Database_ObjectAppended);           
            UpdateGV();
        }

        void DocumentManager_DocumentCreated(object sender, cadapp.DocumentCollectionEventArgs e)
        {
            try
            {
                //MessageBox.Show("DocumentCreated Event:" + e.Document.Name);                
                //注册文档对象修改事件                
                //e.Document.Database.ObjectModified += new ObjectEventHandler(Database_ObjectModified);                
                //注册文档对象删除事件                
                e.Document.Database.ObjectErased += new ObjectErasedEventHandler(Database_ObjectErased);
                //注册文档对象创建事件                
                //e.Document.Database.ObjectAppended += new ObjectEventHandler(Database_ObjectAppended);               
                UpdateGV();
            }
            catch (System.Exception ee)
            {
                cadapp.Application.ShowAlertDialog(ee.Message);
            }
        }

        //void Database_ObjectAppended(object sender, ObjectEventArgs e)        
        //{        
        //    //MessageBox.Show("ObjectAppended Event:" + e.DBObject.GetType().ToString());       
        //    try       
        //    {        
        //        UpdateGV();        
        //    }        
        //    catch (System.Exception ee)        
        //    {        
        //        MessageBox.Show("ObjectAppended Error:" + ee.Message);        
        //    }        
        //}        

        //private void Database_ObjectModified(object sender, ObjectEventArgs e)       
        //{        
        //    //MessageBox.Show("ObjectModified Event:" + e.DBObject.GetType().ToString());       
        //    UpdateGV();       
        //}        

        void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            try
            {
                if (e.Erased)
                {
                    UpdateGV();
                    //我认为问题出现在这里：在事件处理过程中又读取了全局DataBase。但是该怎么实现在删除/创建/改变事件中读取全局DataBase呢？                
                }
            }

            catch (System.Exception ee)
            {
                MessageBox.Show("ObjectErased Error:" + ee.Message);
            }
        }

        private void DocumentManager_DocumentActivated(object sender, cadapp.DocumentCollectionEventArgs e)
        {
            UpdateGV();
        }

        /// <summary>        
        /// 更新GridView的内容        
        /// </summary>        
        private void UpdateGV()
        {
            Hashtable bts = ReloadBlockNames();
            if (bts.Count == 0)
            {
                mycontrol.bt_explode.Text = "无块";
                mycontrol.bt_explode.Enabled = false;
            }
            else
            {
                mycontrol.bt_explode.Text = "炸开";
                mycontrol.bt_explode.Enabled = true;
                mycontrol.setdgvDataSource(bts);
            }
        }

        public void Terminate()
        {
        }
    }
}