$(document).ready(function()
{
	
	$("div.page").css("width","90%");
		
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
		.change(function(){loadOrdersList("进货单");}).button();
	
	
	function getSelectedOrderIDs()
	{
		var selecteds=[];
		$("#orders tr.selectedRow")
		.each(
			function()
			{
				selecteds.push($(this).data("data").ID);
			}
		);
		return selecteds;
	}
	
	$("select.stockinListProvider").change(
		function()
		{
			loadOrdersList("进货单",$(this).val());
		}
	);
	
	
	$("select.stockinListProvider").empty();
    
    $.get("/store/GetProviders",
    	function(providers)
    	{
    		providers.unshift({Provider:"全部"});
    		//写入供货商
			$.each(providers,
				function(i,m)
				{
					$("<option />")
					.val(m.Provider)
					.html(m.Provider)
					.appendTo("select.stockinListProvider");
				}
			);
    	}
    )			
	
	
	function loadOrdersList(orderType,provider)
	{
		if(!provider)
			provider="全部";
			
		$("#orders").dataGrid(
			{
				getDataAddress:"/store/GetMonthNodeOrders",
				onReceivedData:function(datas)
				{					
					var total=0;
					$.each(datas,
						function()
						{
							total+=this.Total-0;
							
							this.Remark=JSON.parse(this.Remark);
							this.Provider=this.Remark.Provider;
						}
					);
					datas.push({Node:"总计",Total:total});
					
					return datas;
				},
				onFormatData:function(rowData,dataField)
				{
					if(dataField==="Total")
					{
						return "￥"+rowData[dataField].round(2);
					}
					return rowData[dataField];
				},
				getDataParams:function()
				{
					var p=
						{
							time:$("#txtSelectedDate").val(),
							orderType:orderType,
							provider:provider
						};
					return p;
				},
				onRowClick:function($row,e)
				{
					if($row.data("data").Node==="总计")
						return;
					if(!e.ctrlKey) //用ctrl实现多选
					{
						$row
						.parent()
						.children("tr:not(.template)")
						.removeClass("selectedRow");						
					}
					
					$row.addClass("selectedRow");
					
					$("#datas").val(JSON.stringify(getSelectedOrderIDs()));
					
					loadOrderDetails(getSelectedOrderIDs());
							
				},
				onRenderRow:function($row)
				{
					var d=$row.data("data");
					if(d.Node==="总计")
					{
						$("input.del",$row).hide();
					}
					
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
										loadOrdersList("进货单");
										loadOrderDetails(0);
									},
									'json'
								)
								.error(
									function()
									{
										alert("删除失败,请重试.");
										loadOrdersList("进货单");
										loadOrderDetails(0);
									}
								);
							}
						}
					);
					
					
					
					$row.css({cursor:'pointer'});
					
					
					
					return $row;
				}
			}
		);
	}
	
	loadOrdersList("进货单");
	
	
	
	function loadOrderDetails(orderIDs)
	{
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
	.button();
	
	$("form")
	.submit(
		function()
		{			
			var ids=getSelectedOrderIDs();
			if(ids.length==0)
				return false;
		}
	);	
}
);