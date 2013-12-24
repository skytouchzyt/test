$(document).ready(
	function()
	{
		var dishes=[];
		var classes=[];
		var currentClassName="";
		
		$("#refresh").click(loadDishes);
		
		$("#add").click(function()
		{
			insert_edit(
				{
					ID:0,
					Name:"",
					ClassName:currentClassName,
					CanDiscounted:true
				}
			);	
		}
		);
		
		function loadDishes()
		{
			$.getJSON("/dishes/getdishesforedit",
				function(result)
				{
					if(isArray(result))
					{
						dishes=result;
						
						for(var i=0;i<dishes.length;i++)
						{
							if(classes.indexOf(dishes[i].ClassName)>=0)
								continue;
							classes.push(dishes[i].ClassName);
						}
						
						$("select.className")
						.empty()
						.change(displayDishes)
						
						for(var i=0;i<classes.length;i++)
						{
							$("<option />")
							.attr("value",classes[i])
							.html(classes[i])
							.appendTo("select.className");
						}
						
						if(currentClassName)
						{
							$("select.className").val(currentClassName);
						}
						
						displayDishes();
						
						
						
					}
				}
			);
		}
	
		loadDishes();
		
		function displayDishes()
		{
			$("#dishes").dataGrid(
				{
					getDataAddress:getDishesByClassName,
					onExecuteCommand:function(command,dish)
					{
						switch(command)
						{
							case "删除":
								if(confirm("确定要删除"+dish.Name+"吗?"))
								{
									$.post("/dishes/delete",
										{
											id:dish.ID
										},
										function(result)
										{
											loadDishes();
										}
									);
								}
								break;
							case "编辑":
								insert_edit(dish);
								break;
						}
					},
					onFormatData:function(row,field)
					{
						if(field==="CanDiscounted")
						{
							return row[field]?"可":"否";
						}
						else 
							return row[field];
					}
				}
			);
		}
		
		
		function getDishesByClassName()
		{
			currentClassName=$("select.className").val();
			var list=[];
			for(var i=0;i<dishes.length;i++)
			{
				if(dishes[i].ClassName===currentClassName)
				{
					list.push(dishes[i]);
				}
			}
			return list;
		}
		
		function insert_edit(dish)
		{
			var $dialog=$("#dialog");
			var title="";
			if(dish.ID===0)
			{
				title="添加新的餐品"
			}
			else
			{
				title="修改餐品:"+dish.Name;
			}
			
			for(var field in dish)
			{
				$("[datafield='"+field+"']",$dialog).val(dish[field]);
			}
			
			$("[datafield='CanDiscounted']",$dialog).attr("checked",dish.CanDiscounted);
			
			$("input.cancel",$dialog)
			.unbind()
			.click(function(){$dialog.dialog('close');});
			
			$("input.save",$dialog)
			.unbind()
			.click(function()
			{
				var temp={};
				var completed=true;
				$("[datafield]",$dialog).each(
					function()
					{
						var val=$(this).val().trim();
						if(!val)
						{
							alert("请填写完整!")
							completed=false;
							return false;
						}
						temp[$(this).attr("datafield")]=$(this).val();	
					}
				);
				
				if(!completed)
				{
					return;
				}
				
				temp.CanDiscounted=$("[datafield='CanDiscounted']",$dialog).check();
				
				
				var finds=$.grep(dishes,function(d,index)
				{
					return d.ID!=temp.ID
						   &&d.Name===temp.Name;					
				}
				);
				
				if(finds.length>0)
				{
					alert("餐品名称重复,请重新输入名称!");
					return;
				}
				
				var datas=JSON.stringify(temp);
				if(temp.ID==0) //新建餐品
				{
					saveDish("/dishes/create",temp,$dialog);
				}
				else //更新餐品
				{
					saveDish("/dishes/edit",temp,$dialog);
				}
				
				
			}
			);
			
			function saveDish(address,dish,$dialog)
			{
				$.post(address,{datas:JSON.stringify(dish)},
						function(result)
								{
									if(result.result==="success")
									{
										alert("保存成功.");
										$dialog.dialog('close');
										loadDishes();
									}
									else
									{
										alert("保存错误,请重试."+result.errorMessage);
									}
								},
								'json'
						)
						.error(
								function()
								{
									alert("保存错误,请重试.");
								}
						);
			}
			
			$dialog.dialog({modal:true,title:title});
		}
	}
);
