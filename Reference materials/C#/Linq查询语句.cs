select 
    描述：查询顾客的公司名、地址信息 
    查询句法： 
    var  构建匿名类型 1 = from c in ctx.Customers 
						select new   
						{   
							公司名 = c.CompanyName,   
							地址 = c.Address   
						};
						
	描述：查询职员的姓名和雇用年份 
    查询句法：       
	var 构建匿名类型 2 = from emp in ctx.Employees   
						select new   
						{   
							姓名 = emp.LastName + emp.FirstName,   
							雇用年 = emp.HireDate.Value.Year   
						}; 
						
	描述：查询顾客的 ID 以及联系信息(职位和联系人) 
    查询句法： 
    var  构建匿名类型 3 = from c in ctx.Customers 
						select new   
						{   
							ID = c.CustomerID,   
							联系信息 = new       
							{   
								职位 = c.ContactTitle,   
								联系人 = c.ContactName   
							}   
						}; 					
						
	描述：查询订单号和订单是否超重的信息 
    查询句法： 
	var select 带条件 = from o in ctx.Orders   
						select new   
						{   
							订单号 = o.OrderID,   
							是否超重 = o.Freight > 100 ? "是" : "否"   
						}; 

						
where 
    描述：查询顾客的国家、城市和订单数信息，要求国家是法国并且订单数大于 5 
    查询句法： 
    var 多条件 = from c in ctx.Customers   
				where c.Country == "France" && c.Orders.Count > 5   
				select new   
				{   
					国家 = c.Country,   
					城市 = c.City,   
					订单数 = c.Orders.Count   
				}; 
				
orderby 
    描述：查询所有没有下属雇员的雇用年和名，按照雇用年倒序，按照名正序 
    查询句法：  
    var 排序 = from emp in ctx.Employees   
			where emp.Employees.Count == 0   
			orderby emp.HireDate.Value.Year descending, emp.FirstName ascending   
			select new   
			{   
				雇用年 = emp.HireDate.Value.Year,   
				名 = emp.FirstName   
			}; 			
				
分页 
    描述：按照每页 10 条记录，查询第二页的顾客 
    查询句法： 
     var 分页 = (from c in ctx.Customers select c).Skip(10).Take(10); 			
				
GROUP 
    描述：根据顾客的国家分组，查询顾客数大于 5 的国家名和顾客数 
    查询句法： 
    var 一般分组 = from c in ctx.Customers   
				group c by c.Country into g   
				where g.Count() > 5   
				orderby g.Count() descending   
				select new   
				{   
					国家 = g.Key,   
					顾客数 = g.Count()   
				}; 				
				
	描述：根据国家和城市分组，查询顾客覆盖的国家和城市 
    查询句法： 
	var 匿名类型分组 = from c in ctx.Customers   
					group c by new { c.City, c.Country } into g   
					orderby g.Key.Country, g.Key.City   
					select new   
					{   
						国家 = g.Key.Country,   
						城市 = g.Key.City   
					}; 			
	
    描述：按照是否超重条件分组，分别查询订单数量 
    查询句法： 
    var 按照条件分组 = from o in ctx.Orders   
					group o by new { 条件 = o.Freight > 100 } into g   
					select new   
					{   
						数量 = g.Count(),   
						是否超重 = g.Key.条件 ? "是" : "否"   
					}; 	
				
distinct 
    描述：查询顾客覆盖的国家 
    查询句法： 
     var  过滤相同项 = (from c in ctx.Customers orderby c.Country select c.Country).Distinct(); 		
	 
union 
    描述：查询城市是 A 打头和城市包含 A 的顾客并按照顾客名字排序 
    查询句法： 
    var 连接并且过滤相同项 = (from c in ctx.Customers where c.City.Contains("A") select c).Union   
							(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
							.OrderBy(c => c.ContactName);			
				
concat 		
	描述：查询城市是 A 打头和城市包含 A 的顾客并按照顾客名字排序，相同的顾客信息不会过滤 
    查询句法： 
	var 连接并且不过滤相同项 = (from c in ctx.Customers where c.City.Contains("A") select c).Concat 
								(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
								.OrderBy(c => c.ContactName);		
				
取相交项 
    描述：查询城市是 A 打头的顾客和城市包含 A 的顾客的交集，并按照顾客名字排序 
    查询句法：     
	var 取相交项 = (from c in ctx.Customers where c.City.Contains("A") select c).Intersect   
					(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
					.OrderBy(c => c.ContactName);				
				
排除相交项 
    描述：查询城市包含 A 的顾客并从中删除城市以 A 开头的顾客，并按照顾客名字排序     
	查询句法： 
    var 排除相交项 = (from c in ctx.Customers where c.City.Contains("A") select c).Except     
					(from c in ctx.Customers where c.ContactName.StartsWith("A") select c)
					.OrderBy(c => c.ContactName);				
				
子查询     
	描述：查询订单数超过 5 的顾客信息 
    查询句法： 
	var 子查询 = from c in ctx.Customers   
    where   
		(from o in ctx.Orders group o by o.CustomerID into o where o.Count() > 5 select o.Key)
		.Contains(c.CustomerID) 
    select c; 				
				
IN 操作 
    描述：查询指定城市中的客户 
    查询句法： 
     var in 操作 = from c in ctx.Customers   
					where new string[] { "Brandenburg", "Cowes", "Stavern" }.Contains(c.City) 
					select c; 				
 
JOIN 
    描述：内连接，没有分类的产品查询不到 
    查询句法：  
	var innerjoin = from p in ctx.Products join c in ctx.Categories on p.CategoryID equals c.CategoryID   
					select p.ProductName; 				
	
	描述：外连接，没有分类的产品也能查询到 
    查询句法： 
    var leftjoin = from p in ctx.Products join c in ctx.Categories on p.CategoryID equals c.CategoryID into pro   
					from x in pro.DefaultIfEmpty()   
					select p.ProductName; 	
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				
				