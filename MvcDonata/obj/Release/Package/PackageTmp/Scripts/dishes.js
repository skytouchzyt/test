
var scope;

function dishCtrl($scope) {
    scope = $scope;
    $scope.setData = function (dish) {
        $scope.dish = dish;
    };
}

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
				},
                false
			);	
		}
		);
		
		function loadDishes()
		{
		    $.getJSON("/dishes/getdishes",
                {all:true},
				function(result)
				{
				    result = JSON.parse(result.result);
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
							case "编辑":
								insert_edit(dish,true);
								break;
						}
					},
					onFormatData:function(row,field)
					{
						if(field==="CanDiscounted")
						{
							return row[field]?"可":"否";
						}
						else if (field == "Actived") {
						    return row[field] ? "激活" : "禁用";
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
				    if ($("#displayAll").check() == false && dishes[i].Actived == false) {
				        continue;
				    }
					list.push(dishes[i]);
				}
			}
			return list;
		}
		
		function insert_edit(dish,edited)
		{
			var $dialog=$("#dialog");
			var title="";
			if(edited==false)
			{
			    title = "添加新的餐品"
			    $("[datafield='Name']", $dialog).removeAttr("readonly");
			}
			else
			{
			    title = "修改餐品:" + dish.Name;
			    $("[datafield='Name']", $dialog).attr("readonly","");
			}


			
			for(var field in dish)
			{
				$("[datafield='"+field+"']",$dialog).val(dish[field]);
			}
			
			$("[datafield='CanDiscounted']",$dialog).attr("checked",dish.CanDiscounted);
			$("[datafield='Actived']", $dialog).attr("checked", dish.Actived);
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
						if(!val&&$(this).attr("required"))
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
				temp.Actived = $("[datafield='Actived']", $dialog).check();
				
				if (edited == false) { //如果是添加新的餐品
				    var finds = $.grep(dishes, function (d, index) {
				        return d.Name === temp.Name;
				    }
                    );

				    if (finds.length > 0) {
				        alert("餐品名称重复,请重新输入名称!");
				        return;
				    }
				}
				
				var datas=JSON.stringify(temp);
				if(edited==false) //新建餐品
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
