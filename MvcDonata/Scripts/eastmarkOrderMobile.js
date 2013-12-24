$(document).ready(
	function()
	{
		function onChange()
		{
			var $li=$(this).parents("li");
			var price=$(".Price",$li).val();
			var amount=$(".Amount",$li).val();
			$(".Total",$li).html("￥"+price*amount);
		}
		
		function createNewRow($template,rowData)
		{
			$new=$template.clone().removeClass("template");
			$new.data("data",rowData);
						
			$(".Name",$new).html(rowData.Name+"["+rowData.Unit+"]");
			
			$(".Price",$new)
			.val(rowData.Price)
			.change(onChange)
			.focus(function()
				{
					var $target=$(this);
					setTimeout(function()
						{
							$target.select();
						},
						300
					);
				}
			);
			
			$(".Amount",$new)
			.val(rowData.Amount)
			.change(onChange)
			.focus(function()
				{
					var $target=$(this);
					setTimeout(function()
						{
							$target.select();
						},
						300
					);
					
				}
			);
			
			$(".Total",$new).html("￥"+rowData.Price*rowData.Amount);
			
			
			$(".mark",$new).click(
				function()
				{
					var $li=$(this).parents("li");
					var rowData=$li.data("data");
					rowData.Price=$(".Price",$li).val();
					rowData.Amount=$(".Amount",$li).val();
					
					if($li.parent().is("#notdoneOrderList"))
					{
						$newdone=createNewRow($("#done li.template"),rowData);
						$newdone.show().appendTo("#doneOrderList");
					}
					else
					{
						$newdone=createNewRow($("#notdone li.template"),rowData);
						$newdone.show().appendTo("#notdoneOrderList");
					}
					
					
					$li.remove();
					
					$li.parent().listview('refresh');
				}
			);
			
			return $new;
		}
		
		$.getJSON("/store/getEastMarkOrderDetails",
			{datas:$("input#ids").val()},
			function(rows)
			{
				
				$template=$("#notdone li.template");
				$.each(rows,function()
					{
						delete this.LastAccess;
						var $new=createNewRow($template,this);
						$new.show().appendTo("#notdoneOrderList");
										
					}
				);
				
				$("#notdoneOrderList").listview('refresh');
			}
		);
		
		
		$("a.changePage").click(function()
		{
			$("ol").listview('refresh');
		}
		);
		
		$("#saveOrder").click(function()
			{
				var datas=[];
				$("#doneOrderList li").each(
					function()
					{
						var $row=$(this);
						var row=$row.data("data");
						row.Price=$(".Price",$row).val();
						row.Amount=$(".Amount",$row).val();
						datas.push(row);
					}
				);
				
				if(datas.length===0)
				{
					alert("请不要提交空订单!");
					return;
				}
				
				$.post("/store/SaveStockIn",
					{
						datas:JSON.stringify(datas),
						provider:"东郊市场/西南郊市场",
						time:new Date().format("yyyy-mm-dd")
					},
					function(result)
					{
						if(result.result==="success")
						{
							alert("保存成功！");
						}
						else
						{
							alert("保存失败！");
						}
					}
				)
				.error(function()
				{
					funcFail("发生错误,请重试!")		
				}
				);
			}
		);
	}
);
