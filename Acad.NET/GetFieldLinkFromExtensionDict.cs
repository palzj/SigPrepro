
        [CommandMethod("GFL")]
        static public void GetFieldLink()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Ask the user to select an attribute or an mtext
            PromptEntityOptions opt = new PromptEntityOptions(
                "\nSelect an MText object containing field(s): ");
            opt.SetRejectMessage("\nObject must be MText.");
            opt.AddAllowedClass(typeof(MText), false);

            PromptEntityResult res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Check the entity is an MText object
                    DBObject obj = tr.GetObject(res.ObjectId, OpenMode.ForRead);
                    MText mt = obj as MText;
                    if (mt != null)
                    {
                        if (!mt.HasFields)
                        {
                            ed.WriteMessage("\nMText object does not contain fields.");
                        }
                        else
                        {
                            // Open the extension dictionary
                            DBDictionary extDict = (DBDictionary)tr.GetObject(
                                mt.ExtensionDictionary, OpenMode.ForRead);
                            const string fldDictName = "ACAD_FIELD";
                            const string fldEntryName = "TEXT";
                            // Get the field dictionary
                            if (extDict.Contains(fldDictName))
                            {
                                ObjectId fldDictId = extDict.GetAt(fldDictName);
                                if (fldDictId != ObjectId.Null)
                                {
                                    DBDictionary fldDict = (DBDictionary)tr.GetObject(
                                        fldDictId, OpenMode.ForRead);
                                    // Get the field itself
                                    if (fldDict.Contains(fldEntryName))
                                    {
                                        ObjectId fldId = fldDict.GetAt(fldEntryName);
                                        if (fldId != ObjectId.Null)
                                        {
                                            obj = tr.GetObject(fldId, OpenMode.ForRead);
                                            Field fld = obj as Field;
                                            if (fld != null)
                                            {
                                                // And finally get the string
                                                // including the field codes
                                                string fldCode = fld.GetFieldCode();
                                                ed.WriteMessage("\nField code: " + fldCode);
                                                // Loop, using our helper function
                                                // to find the object references
                                                do
                                                {
                                                    ObjectId objId;
                                                    fldCode = FindObjectId(fldCode, out objId);
                                                    if (fldCode != "")
                                                    {
                                                        // Print the ObjectId
                                                        ed.WriteMessage("\nFound Object ID: "
                                                          + objId.ToString());
                                                        obj = tr.GetObject(objId, OpenMode.ForRead);
                                                        // ... and the type of the object
                                                        ed.WriteMessage(", which is an object of type "
                                                          + obj.GetType().ToString());
                                                    }
                                                } while (fldCode != "");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Extract an ObjectId from a field string
        // and return the remainder of the string
        static public string FindObjectId(string text, out ObjectId objId)
        {
            const string prefix = "%<\\_ObjId ";
            const string suffix = ">%";
            // Find the location of the prefix string
            int preLoc = text.IndexOf(prefix);
            if (preLoc > 0)
            {
                // Find the location of the ID itself
                int idLoc = preLoc + prefix.Length;
                // Get the remaining string
                string remains = text.Substring(idLoc);
                // Find the location of the suffix
                int sufLoc = remains.IndexOf(suffix);
                // Extract the ID string and get the ObjectId
                string id = remains.Remove(sufLoc);
                objId = new ObjectId(Convert.ToInt32(id));
                // Return the remainder, to allow extraction
                // of any remaining IDs
                return remains.Substring(sufLoc + suffix.Length);
            }
            else
            {
                objId = ObjectId.Null;
                return "";
            }
        }
