$(document).ready(function()
{
	
	$("div.page").css("width","90%");
	
	$("#tabs").tabs()
    .bind("tabsselect",function(e,ui)
    {
    	if(ui.index>0)
    	{
    		$("div.move").appendTo(ui.panel);

    	}
    	if(ui.index===1)
    	{
    		loadOrdersList("订货单");
    	}
    	else if(ui.index===2)
    	{
    		loadOrdersList("出库单");
    	}
    });
	
	$("#txtSelectedDate").datepicker({
                currentText: '今天',
                dateFormat: 'yy-mm-dd',
                dayNamesMin: ['周日', '周一', '周二', '周三', '周四', '周五', '周六'],
                defaultDate: "",
                monthNames: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'],
                nextText: '下个月',
				prevText:'上个月',
                showMonthAfterYear: true
            }
		)
		.val(new Date().format("yyyy-mm-dd"))
		.change(
			function()
			{
				
				if($("#tabs").tabs("option","selected")==2)
					loadOrdersList("出库单");
				else
					loadOrdersList("订货单");
			}
		).button();
	
	
	function getSelectedOrderIDs()
	{
		var selecteds=[];
		$("#orders tr.selectedRow")
		.each(
			function()
			{
				var id=$(this).data("data").ID;
				if(isArray(id)) //如果是数组
				{
					$.each(id,function(){selecteds.push(this);});
				}
				else
				{
					selecteds.push($(this).data("data").ID);
				}
				
			}
		);
		return selecteds;
	}
	
	function loadOrdersList(orderType)
	{
		if(orderType==="出库单")
			address="/store/GetMonthNodeOrders";
		else
			address="/store/GetWeeklyNodeOrders";
		$("#orders").dataGrid(
			{
				getDataAddress:address,
				getDataParams:function()
				{
					
					if(orderType==="订货单")
					{
						return {
							time:$("#txtSelectedDate").val(),
							orderType:orderType
						};
					}
					else if(orderType==="出库单")
					{
						return {
							time:$("#txtSelectedDate").val(),
							orderType:orderType,
							combined:true
						};
					}	
					
				},
				onRowClick:function($row,e)
				{
					if(!e.ctrlKey) //用ctrl实现多选
					{
						$row
						.parent()
						.children("tr:not(.template)")
						.removeClass("selectedRow");						
					}
					
					$row.addClass("selectedRow");
					
					var ids=getSelectedOrderIDs();
					$("#datas").val(JSON.stringify(ids));
					
					loadOrderDetails(ids);
							
				},
				onRenderRow:function($row)
				{
					$("input.del",$row).click(
						function()
						{
							var order=$row.data("data");
							if(confirm("确定要删除此订货单吗?"))
							{
								$.post(
									"/store/DeleteNodeOrder",
									{
										orderID:order.ID
									},
									function(result)
									{
										if(result.result==="success")
										{
											alert("删除成功.");
										}
										else
										{
											alert("删除失败,请重试."+result.errorMessage);
										}
										loadOrdersList("订货单");
										loadOrderDetails(0);
									},
									'json'
								)
								.error(
									function()
									{
										alert("删除失败,请重试.");
										loadOrdersList("订货单");
										loadOrderDetails(0);
									}
								);
							}
						}
					);
					
					$("input.print",$row).click(
						function()
						{
							var $printButton=$(this);
							$printButton.enable(false);
							var order=$row.data("data");
							if(!confirm("确定要打印此发货单吗?"))
								return;
							var ids=order.ID;
							if(!isArray(ids))
							{
								ids=[ids];
							}
							ids=JSON.stringify(ids);
							var command="print store "+ids;
							$.post("/store/CreateCommand",
								{
									node:order.Node,
									command:command
								},
								function(result)
								{
									if(result.result==="success")
									{
										alert("打印指令提交成功.");
									}
									else
									{
										alert("提交打印指令错误,请重试."+result.errorMessage);
									}
									$printButton.enable(true);
								},
								'json'
							)
							.error(
									function()
									{
										alert("提交打印指令错误,请重试.");
										$printButton.enable(true);
									}
							);
						}
					);
					
					$row.css({cursor:'pointer'});
					
					if(orderType==="订货单")
					{
						$("input.del",$row).show();
						$("input.print",$row).hide();
					}
					else
					{
						$("input.del",$row).hide();
						$("input.print",$row).show();
					}
					
					return $row;
				}
			}
		);
	}
	
	loadOrdersList("订货单");
	
	
	
	function loadOrderDetails(orderIDs)
	{
		if(orderIDs===0)
		{
			$("#details tr.datarow").remove();
			return;
		}
		$("#details").dataGrid(
			{
				getDataAddress:"/store/GetNodeOrderDetails",
				getDataParams:function()
				{
					var p={datas:JSON.stringify(orderIDs)};
					return p;
				},
				onRenderRow:function($row)
				{
					$row.attr("title",$row.data("data").Provider);
					return $row;
				},
				onReceivedData:function(datas)
				{
					return datas.Details;
				}
		
			}
		)
	}
	
	//处理订单
	$("#dealOrder")
	.click(
		function()
		{
			var ids=getSelectedOrderIDs();
			if(ids.length==0)
				return false;
			$("#datas").val(JSON.stringify(ids));
			$("#postData")
			.attr("action","/store/stockout")
			.submit();
		}
	)
	.button();
	
	$("#eastMarkOrder")
	.button()
	.click(function()
		{
			var orders=[];
			$("#orders tr.datarow")
			.each(
				function()
				{
					orders.push($(this).data("data").ID);
				}
			);
			if(orders.length===0)
			{
				return;
			}
			$("#datas").val(JSON.stringify(orders));
			$("#postData")
			.attr("action","/store/EastMarkOrder")
			.submit();
		}
	);
		
}
);