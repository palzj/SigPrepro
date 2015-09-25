using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
 
namespace WatchErasure
{
  public class Commands
  {
    // A list of erased entities, populated during OnErased()
 
    ObjectIdCollection _ids = null;
 
    // A list of blocks to look out for, popultade during AddWatch()
 
    SortedList<string, string> _blockNames = null;
 
    // A command to add a watch for a particular block
 
    [CommandMethod("AW")]
    public void AddWatch()
    {
      Document doc =
        Application.DocumentManager.MdiActiveDocument;
      Database db = doc.Database;
      Editor ed = doc.Editor;
 
      // Start by displaying the watches currently in place
 
      ListBlocksBeingWatched(ed);
 
      // Ask for the name of a block to watch for
 
      PromptStringOptions pso =
        new PromptStringOptions(
          "\nEnter block name to watch: "
        );
      pso.AllowSpaces = true;
 
      PromptResult pr = ed.GetString(pso);
 
      if (pr.Status != PromptStatus.OK)
        return;
 
      // Use all capitals for the block name
 
      string blockName = pr.StringResult.ToUpper();
 
      // If there currently isn't a list of block names,
      // create on, along with the erased entity list
      // Then attach our event handlers
 
      if (_blockNames == null)
      {
        _blockNames = new SortedList<string, string>();
        _ids = new ObjectIdCollection();
 
        db.ObjectErased +=
          new ObjectErasedEventHandler(OnObjectErased);
        doc.CommandEnded +=
          new CommandEventHandler(OnCommandEnded);
      }
 
      // If the list contains our block, no need to add it
 
      if (_blockNames.ContainsKey(blockName))
      {
        ed.WriteMessage(
          "\nAlready watching block \"{0}\".",
          blockName
        );
      }
      else
      {
        // Otherwise add the block name and display the list
 
        _blockNames.Add(blockName, blockName);
 
        ListBlocksBeingWatched(ed);
      }
    }
 
    // A command to stop watching for a particular block
 
    [CommandMethod("RW")]
    public void RemoveWatch()
    {
      Document doc =
        Application.DocumentManager.MdiActiveDocument;
      Database db = doc.Database;
      Editor ed = doc.Editor;
 
      // Start by displaying the watches currently in place
 
      ListBlocksBeingWatched(ed);
 
      // if there are no watches in place, nothing to do
 
      if (_blockNames == null || _blockNames.Count == 0)
        return;
 
      // Ask for the name of a block to stop watching for
 
      PromptStringOptions pso =
        new PromptStringOptions(
          "\nEnter block name to stop watching <All>: "
        );
      pso.AllowSpaces = true;
 
      PromptResult pr = ed.GetString(pso);
 
      if (pr.Status != PromptStatus.OK)
        return;
 
      // Use all capitals for the block name
 
      string blockName = pr.StringResult.ToUpper();
 
      // If a particular block was chosen...
 
      if (blockName != "")
      {
        // Remove it from our list, if it's on it
 
        if (_blockNames.ContainsKey(blockName))
        {
          _blockNames.Remove(blockName);
 
          ed.WriteMessage(
            "\nWatch removed for block \"{0}\".",
            blockName
          );
        }
        else
        {
          ed.WriteMessage(
            "\nNot currently watching a block named \"{0}\".",
            blockName
          );
        }
      }
 
      // If that was the last entry, or we're clearing the list...
 
      if (blockName == "" || _blockNames.Count == 0)
      {
        // Start by asking for confirmation, if we're clearing
 
        if (blockName == "")
        {
          PromptKeywordOptions pko =
            new PromptKeywordOptions(
              "Stop watching all blocks? [Yes/No]: ",
              "Yes No"
            );
 
          pko.Keywords.Default = "No";
 
          pr = ed.GetKeywords(pko);
          if (pr.Status != PromptStatus.OK ||
              pr.StringResult == "No")
          {
            return;
          }
        }
 
        // Now we remove the entity list and set it to null
 
        if (_ids != null)
        {
          _ids.Dispose();
          _ids = null;
        }
 
        // And the same for the list of block names
 
        if (_blockNames != null)
          _blockNames = null;
 
        // And we detach our event handlers
 
        db.ObjectErased -=
          new ObjectErasedEventHandler(OnObjectErased);
        doc.CommandEnded -=
          new CommandEventHandler(OnCommandEnded);
      }
 
      // Finally we report the current state of the watch list
 
      ListBlocksBeingWatched(ed);
    }
 
    // A helper function to list the block names in our list
 
    private void ListBlocksBeingWatched(Editor ed)
    {
      // Start by checking there's something on the list
 
      if (_blockNames == null)
      {
        ed.WriteMessage("\nNot watching any blocks.");
      }
      else
      {
        // If so, loop through and print the names, one by one
 
        ed.WriteMessage("\nWatching blocks: ");
        bool first = true;
        foreach(
          KeyValuePair<string, string> blockName in _blockNames
        )
        {
          ed.WriteMessage(
            "{0}{1}",
            (first ? "" : ", "),
            blockName.Key
          );
          first = false;
        }
        ed.WriteMessage(".");
      }
    }
 
    // A callback for the Database.ObjectErased event
 
    private void OnObjectErased(
      object sender, ObjectErasedEventArgs e
    )
    {
      // Very simple: we just add our ObjectId to the list
      // for later processing
 
      if (e.Erased)
      {
        if (!_ids.Contains(e.DBObject.ObjectId))
          _ids.Add(e.DBObject.ObjectId);
      }
    }
 
    // A callback for the Document.CommandEnded event
 
    private void OnCommandEnded(
      object sender, CommandEventArgs e
    )
    {
      // Start an outer transaction that we pass to our testing
      // function, avoiding the overhead of multiple transactions
 
      Document doc = sender as Document;
      if (_ids != null)
      {
        Transaction tr =
          doc.Database.TransactionManager.StartTransaction();
        using (tr)
        {
          // Test each object, in turn
 
          foreach (ObjectId id in _ids)
          {
            // The test function is responsible for presenting the
            // user with the information: this could be returned to
            // this function, if needed
 
            TestObjectAndShowMessage(doc, tr, id);
          }
 
          // Even though we're only reading, we commit the
          // transaction, as this is more efficient
 
          tr.Commit();
        }
 
        // Now we clear our list of entities
 
        _ids.Clear();
      }
    }
 
    // A function to test for the type of object we're interested in
 
    private void TestObjectAndShowMessage(
      Document doc, Transaction tr, ObjectId id
    )
    {
      // We are looking for blocks of a certain name,
      // although this function could be adapted to
      // watch for any kind of entity
 
      Editor ed = doc.Editor;
 
      // We must remember to pass true for "open erased?"
 
      DBObject obj = tr.GetObject(id, OpenMode.ForRead, true);
      BlockReference br = obj as BlockReference;
      if (br != null)
      {
        // If we have a block reference, get its associated
        // block definition
 
        BlockTableRecord btr =
          (BlockTableRecord)tr.GetObject(
            br.IsDynamicBlock ?
              br.DynamicBlockTableRecord :
              br.BlockTableRecord,
            OpenMode.ForRead
          );
 
        // Check its name against our list
 
        string blockName = btr.Name.ToUpper();
        if (_blockNames.ContainsKey(blockName))
        {
          // Display a message, if it's on it
 
          ed.WriteMessage(
            "\nBlock \"{0}\" erased.",
            blockName
          );
        }
      }
    }
  }
}