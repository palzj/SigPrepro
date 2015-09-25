Public Sub FillArray()
Dim ptArray(,) As String = Nothing'<-- declare empty array
Dim doc As Document = Application.DocumentManager.MdiActiveDocument
Dim db As Database= doc.Database
Dim ed As Editor= doc.Editor
Dim tr As Transaction= doc.TransactionManager.StartTransaction
Dim pr As PromptEntityResult = ed.GetEntity("Select table : ")
If pr.Status <> PromptStatus.OK ThenReturnUsingtr
Dim ent As Entity = DirectCast(tr.GetObject(pr.ObjectId, OpenMode.ForRead), Entity)
Dim table As Table = TryCast(ent, Table)
If table Is Nothing Then Return'create array of string instance with two dimension lengs
ptArray = _Array.CreateInstance(GetType(String), table.NumRows, table.NumColumns)
For row AsInteger = 0 To table.NumRows - 1
For col AsInteger = 0 To table.NumColumns - 1
ptArray(row, col) = table.TextString(row, col)
Next
Next
End Using
End Sub
But better yet to use
Dim ptArray as List(of List(of String))=new List(of string)(new String(){})