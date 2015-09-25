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
        private PaletteSet ps = new PaletteSet("��ѡ����");
        private Hashtable BlockName_Count = new Hashtable();
        private UserControl1 mycontrol = new UserControl1();
        public CAD() { }

        /// <summary>        
        /// ��ȡ��ǰ��ĵ������п�����������Hashtable��        
        /// </summary>        
        /// <returns>����Hashtable</returns>        
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
        /// ը�����п�       
        /// </summary>        
        /// <param name="b_name">������</param>        
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
            ed.WriteMessage("\nInitialize����������\n");
            ps.Style = PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowPropertiesMenu;
            //ps.Dock = DockSides.None;            
            ps.DockEnabled = DockSides.Left | DockSides.Right | DockSides.None;
            ps.MinimumSize = new System.Drawing.Size(150, 100);
            ps.Size = new System.Drawing.Size(150, 100);
            ps.Add("Palette1", mycontrol);
            ps.Visible = true;

            //ע���ĵ��л��¼�            
            cadapp.Application.DocumentManager.DocumentActivated += new cadapp.DocumentCollectionEventHandler(DocumentManager_DocumentActivated);
            //ע���ĵ��򿪡������¼�            
            cadapp.Application.DocumentManager.DocumentCreated += new cadapp.DocumentCollectionEventHandler(DocumentManager_DocumentCreated);
            //ע���ĵ�����ɾ���¼�           
            cadapp.Application.DocumentManager.MdiActiveDocument.Database.ObjectErased += new ObjectErasedEventHandler(Database_ObjectErased);
            //ע���ĵ����󴴽��¼�            
            //cadapp.Application.DocumentManager.MdiActiveDocument.Database.ObjectAppended += new ObjectEventHandler(Database_ObjectAppended);           
            UpdateGV();
        }

        void DocumentManager_DocumentCreated(object sender, cadapp.DocumentCollectionEventArgs e)
        {
            try
            {
                //MessageBox.Show("DocumentCreated Event:" + e.Document.Name);                
                //ע���ĵ������޸��¼�                
                //e.Document.Database.ObjectModified += new ObjectEventHandler(Database_ObjectModified);                
                //ע���ĵ�����ɾ���¼�                
                e.Document.Database.ObjectErased += new ObjectErasedEventHandler(Database_ObjectErased);
                //ע���ĵ����󴴽��¼�                
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
                    //����Ϊ���������������¼�����������ֶ�ȡ��ȫ��DataBase�����Ǹ���ôʵ����ɾ��/����/�ı��¼��ж�ȡȫ��DataBase�أ�                
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
        /// ����GridView������        
        /// </summary>        
        private void UpdateGV()
        {
            Hashtable bts = ReloadBlockNames();
            if (bts.Count == 0)
            {
                mycontrol.bt_explode.Text = "�޿�";
                mycontrol.bt_explode.Enabled = false;
            }
            else
            {
                mycontrol.bt_explode.Text = "ը��";
                mycontrol.bt_explode.Enabled = true;
                mycontrol.setdgvDataSource(bts);
            }
        }

        public void Terminate()
        {
        }
    }
}