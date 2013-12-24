$(document).ready(
	function()
	{
		function refreshData()
		{
			$("#orderList").dataGrid();
		}
		
		
		setInterval(refreshData,60000);
		
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
		.change(refreshData).button();
		
		$("#selectedNode").change(refreshData);
		
		$("#btnRefresh").click(refreshData).button();

		
		$("#orderList").dataGrid(
			{
				showAnimation:{effect:{mode:'show',direction:'right'},duration:500},
				hideAnimation:{effect:{mode:'hide',direction:'left'},duration:500},
				paged:true,
				getDataAddress:"/dineorder/getdineorders",
				getDataParams:function(params)
				{
					params.node=$("#selectedNode").val();
					params.selectedDate=$("#txtSelectedDate").val();
					return params;
				},
				onReceivedData:function(result)
				{
					
					$("#orderList tr.details").remove();//清空明细行
					
					var $firstOption=$("#selectedNode option:eq(0)");
					
					
					
					//清空不需要显示的节点
					var already=[];
					$("#selectedNode option:gt(0)")
					.each(function(i,option)
					{
						if(result.nodes.indexOf($(option).val())>=0)
						{
							already.push($(option).val());
						}
						else
						{
							$(option).remove();
						}
					}
					); 
					
					//添加需要显示的节点
					$.each(result.nodes,function(i,node)
					{
						if((already.indexOf(node)>=0)==false)
						{
							$("<option />")
							.val(node)
							.html(node)
							.insertAfter($firstOption);							
						}
					}
					);
					return result;
					
				},
				onFormatData:function(row,dataField)
				{
					/*if(dataField=="Time")
					{
						var data=row[dataField];
						var temp=new Date(parseInt(data.replace(/\/Date\((-?\d+)\)\//, '$1')));
						return new Date(temp).format("HH:MM:ss");
					}*/
					if(dataField=="Total")
					{
						return Math.round(row["Total"]);
					}
					else
						return row[dataField];
				},
				onRowClick:function($currentRow)
				{
					//如果不是当前行
					if(!$currentRow.hasClass("currentRow"))
					{
						$currentRow.siblings().removeClass("currentRow");
						$currentRow.addClass("currentRow");
						//移除先前的明细
						var $prev=$("#orderList tr.details table");
						if($prev.length)
						{
							$prev.effect('blind',{},500,showDetails);
						}
						else
						{
							showDetails();
						}
						
						function showDetails()
						{
							$("#orderList tr.details").remove();
							//首先获取有多少列
							var columns=$currentRow.children("td").length;
							var $detailsRow=$("<tr class='details'><td>明细</td><td></td></tr>");
							$detailsRow
							.insertAfter($currentRow)
							.children("td:gt(0)").attr("colspan",columns-1);
							
							var $detailsTable=$("#orderDetails").clone();
							
							if($currentRow.data("data").Remark)
								$("td.remark",$detailsTable).html("备注:"+$currentRow.data("data").Remark||"").show();
							else
								$("tr.remark",$detailsTable).hide();
							
							$detailsTable.appendTo($detailsRow.children("td:gt(0)"))
                                .dataGrid(
							{
							    /*getDataAddress:"/dineorder/getdineorderdetails",
								getDataParams:function(params)
								{
									var temp={OrderID:$currentRow.data("data").ID};
									return temp;
								},*/
							    getDataAddress: $currentRow.data("data").Details,
							    showAnimation: { effect: { mode: 'show', direction: 'up' }, duration: 500 },
							    onFormatData: function (row, dataField) {
							        if (dataField == "Total") {
							            return Math.round(row["Price"] * row["Amount"]);
							        }
							        else
							            return row[dataField];
							    }
							}
							);
							
							
							
						}
					}
					else //如果是当前行
					{
						$currentRow.removeClass("currentRow");
						
						var $prev=$("#orderList tr.details table");
						if($prev.length)
						{
							$prev.effect('blind',{},500,
							function()
							{
								$("#orderList tr.details").remove();
								
							}
							);
						}
					}
					
					
				}
			}
		);
	}
);



