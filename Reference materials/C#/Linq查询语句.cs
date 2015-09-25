select 
    ��������ѯ�˿͵Ĺ�˾������ַ��Ϣ 
    ��ѯ�䷨�� 
    var  ������������ 1 = from c in ctx.Customers 
						select new   
						{   
							��˾�� = c.CompanyName,   
							��ַ = c.Address   
						};
						
	��������ѯְԱ�������͹������ 
    ��ѯ�䷨��       
	var ������������ 2 = from emp in ctx.Employees   
						select new   
						{   
							���� = emp.LastName + emp.FirstName,   
							������ = emp.HireDate.Value.Year   
						}; 
						
	��������ѯ�˿͵� ID �Լ���ϵ��Ϣ(ְλ����ϵ��) 
    ��ѯ�䷨�� 
    var  ������������ 3 = from c in ctx.Customers 
						select new   
						{   
							ID = c.CustomerID,   
							��ϵ��Ϣ = new       
							{   
								ְλ = c.ContactTitle,   
								��ϵ�� = c.ContactName   
							}   
						}; 					
						
	��������ѯ�����źͶ����Ƿ��ص���Ϣ 
    ��ѯ�䷨�� 
	var select ������ = from o in ctx.Orders   
						select new   
						{   
							������ = o.OrderID,   
							�Ƿ��� = o.Freight > 100 ? "��" : "��"   
						}; 

						
where 
    ��������ѯ�˿͵Ĺ��ҡ����кͶ�������Ϣ��Ҫ������Ƿ������Ҷ��������� 5 
    ��ѯ�䷨�� 
    var ������ = from c in ctx.Customers   
				where c.Country == "France" && c.Orders.Count > 5   
				select new   
				{   
					���� = c.Country,   
					���� = c.City,   
					������ = c.Orders.Count   
				}; 
				
orderby 
    ��������ѯ����û��������Ա�Ĺ�������������չ����굹�򣬰��������� 
    ��ѯ�䷨��  
    var ���� = from emp in ctx.Employees   
			where emp.Employees.Count == 0   
			orderby emp.HireDate.Value.Year descending, emp.FirstName ascending   
			select new   
			{   
				������ = emp.HireDate.Value.Year,   
				�� = emp.FirstName   
			}; 			
				
��ҳ 
    ����������ÿҳ 10 ����¼����ѯ�ڶ�ҳ�Ĺ˿� 
    ��ѯ�䷨�� 
     var ��ҳ = (from c in ctx.Customers select c).Skip(10).Take(10); 			
				
GROUP 
    ���������ݹ˿͵Ĺ��ҷ��飬��ѯ�˿������� 5 �Ĺ������͹˿��� 
    ��ѯ�䷨�� 
    var һ����� = from c in ctx.Customers   
				group c by c.Country into g   
				where g.Count() > 5   
				orderby g.Count() descending   
				select new   
				{   
					���� = g.Key,   
					�˿��� = g.Count()   
				}; 				
				
	���������ݹ��Һͳ��з��飬��ѯ�˿͸��ǵĹ��Һͳ��� 
    ��ѯ�䷨�� 
	var �������ͷ��� = from c in ctx.Customers   
					group c by new { c.City, c.Country } into g   
					orderby g.Key.Country, g.Key.City   
					select new   
					{   
						���� = g.Key.Country,   
						���� = g.Key.City   
					}; 			
	
    �����������Ƿ����������飬�ֱ��ѯ�������� 
    ��ѯ�䷨�� 
    var ������������ = from o in ctx.Orders   
					group o by new { ���� = o.Freight > 100 } into g   
					select new   
					{   
						���� = g.Count(),   
						�Ƿ��� = g.Key.���� ? "��" : "��"   
					}; 	
				
distinct 
    ��������ѯ�˿͸��ǵĹ��� 
    ��ѯ�䷨�� 
     var  ������ͬ�� = (from c in ctx.Customers orderby c.Country select c.Country).Distinct(); 		
	 
union 
    ��������ѯ������ A ��ͷ�ͳ��а��� A �Ĺ˿Ͳ����չ˿��������� 
    ��ѯ�䷨�� 
    var ���Ӳ��ҹ�����ͬ�� = (from c in ctx.Customers where c.City.Contains("A") select c).Union   
							(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
							.OrderBy(c => c.ContactName);			
				
concat 		
	��������ѯ������ A ��ͷ�ͳ��а��� A �Ĺ˿Ͳ����չ˿�����������ͬ�Ĺ˿���Ϣ������� 
    ��ѯ�䷨�� 
	var ���Ӳ��Ҳ�������ͬ�� = (from c in ctx.Customers where c.City.Contains("A") select c).Concat 
								(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
								.OrderBy(c => c.ContactName);		
				
ȡ�ཻ�� 
    ��������ѯ������ A ��ͷ�Ĺ˿ͺͳ��а��� A �Ĺ˿͵Ľ����������չ˿��������� 
    ��ѯ�䷨��     
	var ȡ�ཻ�� = (from c in ctx.Customers where c.City.Contains("A") select c).Intersect   
					(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
					.OrderBy(c => c.ContactName);				
				
�ų��ཻ�� 
    ��������ѯ���а��� A �Ĺ˿Ͳ�����ɾ�������� A ��ͷ�Ĺ˿ͣ������չ˿���������     
	��ѯ�䷨�� 
    var �ų��ཻ�� = (from c in ctx.Customers where c.City.Contains("A") select c).Except     
					(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
					.OrderBy(c => c.ContactName);				
				
�Ӳ�ѯ     
	��������ѯ���������� 5 �Ĺ˿���Ϣ 
    ��ѯ�䷨�� 
	var �Ӳ�ѯ = from c in ctx.Customers   
    where   
		(from o in ctx.Orders group o by o.CustomerID into o where o.Count() > 5 select o.Key)
		.Contains(c.CustomerID) 
    select c; 				
				
IN ���� 
    ��������ѯָ�������еĿͻ� 
    ��ѯ�䷨�� 
     var in ���� = from c in ctx.Customers   
					where new string[] { "Brandenburg", "Cowes", "Stavern" }.Contains(c.City) 
					select c; 				
 
JOIN 
    �����������ӣ�û�з���Ĳ�Ʒ��ѯ���� 
    ��ѯ�䷨��  
	var innerjoin = from p in ctx.Products join c in ctx.Categories on p.CategoryID equals c.CategoryID   
					select p.ProductName; 				
	
	�����������ӣ�û�з���Ĳ�ƷҲ�ܲ�ѯ�� 
    ��ѯ�䷨�� 
    var leftjoin = from p in ctx.Products join c in ctx.Categories on p.CategoryID equals c.CategoryID into pro   
					from x in pro.DefaultIfEmpty()   
					select p.ProductName; 	
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				