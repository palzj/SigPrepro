[LispFunction("mGetMethods")]        
public void MyGetMethods(ResultBuffer rb)        
{            
	if (rb != null)            
	{                
		TypedValue[] values = rb.AsArray();                
		if (values.Length == 1 && values[0].TypeCode == (int)LispDataType.ObjectId)                
		{                    
			Document doc = Application.DocumentManager.MdiActiveDocument;                    
			Editor ed = doc.Editor;                    
			Database db = doc.Database;                    
			
			Transaction tr = db.TransactionManager.StartTransaction();                    
			var id = (ObjectId)values[0].Value;                    
			using (tr)                    
			{                        
				try                        
				{                            
					var obj = (DBObject)id.GetObject(OpenMode.ForRead);                            
					var methodInfos = obj.GetType().GetMethods();                            
					foreach (var tParam in methodInfos)                            
					{                                
						var pams = tParam.GetParameters();                                
						string sString = "";                                
						foreach (var type in pams)                                
						{                                    
							sString += (type.ParameterType.ToString()).Split(new Char[] { '.' }).Last() + ",";                                
						}                                
						
						var rString = tParam.ReturnParameter.ToString();                                
						if (rString .Contains( "."))                                
						{                                    
							rString = rString.Split(new[] {'.'}).Last();                                
						}                                
						ed.WriteMessage("\n{0}({1}), ReturnParameter is {2}",                                    
						tParam.Name,                                    
						sString.TrimEnd(new char[] { ',' }),rString );                            
					}                            
					tr.Commit();                        
				}                        
				
				catch (Autodesk.AutoCAD.Runtime.Exception)                        
				{                            
					return;                        
				}                    
			}                
		}            
	}        
}