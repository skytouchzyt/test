$(document).ready(function()
{

	//改变主框架的颜色
	$("#main,footer").css({'background-color': '#5C87B2'});
	
	var materialTypes=
	[
		"食材",
		"消耗品",
		"设备",
		"其他"
	];
	//写入类别
	$.each(materialTypes,
		function(i,m)
		{
			$("<option />")
			.val(m)
			.html(m)
			.appendTo("#materialtype");
		}
	);
	
	
	var allProducts=[];
	
	function loadAllProducts()
	{
		$.getJSON("/store/GetAllProducts",
		function(datas)
		{
			if(isArray(datas))
			{
				allProducts=datas;
			}
			
		}
		);
	}
	
	loadAllProducts();
	
	var left=[];
	
	function loadProducts()
	{
		left=[];
		$.getJSON("/store/GetNodeOrderDetails",
		{datas:$("#orderIDs").val()},
		function(datas)
		{
			datas=datas.Details;
			if(datas.length)
			{
				$.each(datas,function(i,item)
				{
					//右边没有的才加入到左边
					if($.grep(right,function(j,index)
					{
						return j.Name===item.Name;
					}
					).length==0)
					{
						left.push(item);
					}
				}
				);
				updateOrder();
			}
		}
		);
	}
	
	loadProducts();
	
	//如果数量为0就显示红色,否则就是显示白色
	function setColorByAmount($row)
	{
		if($row.parents('table').is("#ProductList")) //如果是左边的
		{
			var amount=parseFloat($("input.amount",$row).val());
			var price=parseFloat($("input.price",$row).val())
			if(isNaN(amount)||amount<=0)
			{
				$("input.amount",$row).css({color:'red'});
			}
			else
			{
				$("input.amount",$row).css({color:'white'});
			}
			if(isNaN(price)||price<=0)
			{
				$("input.price",$row).css({color:'red'});
			}
			else
			{
				$("input.price",$row).css({color:'white'});
			}
		}
		else
		{
			var total=parseFloat($("td.total",$row).html());
			if(total<=0)
			{
				$("td.total",$row).css({color:'red'});
			}
			else
			{
				$("td.total",$row).css({color:'white'});
			}
		}
	}
	
	var right=readRight();
	function readRight()
	{
		if(localStorage.stockOut)
				right=JSON.parse(localStorage.stockOut);
			else
				right=[];
			return right;
	}
	function writeRight()
	{
		localStorage.stockOut=JSON.stringify(right);
	}
	
	//更新订单
	function updateOrder()
	{
		writeRight();
		//更新数量
		function updateAmount($row,inc,e,range)
		{
			if(e.ctrlKey)
				inc*=5;
			//获取当前的数量
			var amount=$("input.amount",$row).val();
			amount=amount-0+inc;
			if(range)
			{
				amount=Math.min(amount,range.maxValue);
				amount=Math.max(amount,range.minValue);
			}			
			$("input.amount",$row).val(amount);
				
			setColorByAmount($row);
		}
		

		
		$("#ProductList").dataGrid(
			{
				getDataAddress:left,
				onRenderRow:function($row)
				{
					$("input.add",$row).click(
					function(e)
					{
						updateAmount($row,1,e,{maxValue:100,minValue:-100});
					}
					);
					
					$("input.sub",$row).click(
						function(e)
						{
							updateAmount($row,-1,e,{maxValue:100,minValue:-100});
						}
					);
	
					$("input.del",$row).click(
						function(e)
						{
							var detail=$row.data("data");
							var price=parseFloat($("input.price",$row).val());
							if(isNaN(price))
							{
								alert("请在价格栏输入数值.");
								return;
							}
							var amount=parseFloat($("input.amount",$row).val());
							if(isNaN(amount))
							{
								alert("请在数量栏输入数值.");
								return;
							}
							detail.Price=price;
							detail.Amount=amount;
							if(detail.Price===0)
							{
								alert("请输入货物的单价.")
								return;
							}
							
							var index=$row.index();
							var del=left.splice(index-1,1);
							right.push(del[0]);
							updateOrder();	
						}
					);
					
					$("input.price,input.amount",$row).change(
						function()
						{
							setColorByAmount($row);
						}
					);
					
					var detail=$row.data("data");
					$("input.price",$row).val(detail.Price);
					$("input.amount",$row).val(detail.Amount);
					
					setColorByAmount($row);
					
					return $row;
				}
			}
		);
		
		$("#OrderList").dataGrid(
		{
			getDataAddress:readRight,
			onRenderRow:function($row)
			{

				$("input.del",$row).click(
					function(e)
					{
						var index=$row.index();
						var del=right.splice(index-1,1);
						left.push(del[0]);
						updateOrder();	
					}
				);
				
				var detail=$row.data("data");
				$("td.total",$row).html((detail.Price*detail.Amount).round(4));
				
				setColorByAmount($row);
				
				$row.attr({title:detail.Provider});
				
				return $row;
			}
		}
		);
		
		$("div.orderCount").html("总计共有货物:"+right.length+"种.");
	}
	
	function addOrderDetail(detail)
	{
		if(!detail.Name||!detail.Amount)
		{
			alert("请输入名称和数量!");
			return;
		}
		var findFunc=function(item,index)
		{
			return item.Name==detail.Name;
		};
		var find=$.grep(right,findFunc);
		
		
		
		if(find.length>0)
		{
			alert("已经添加了此商品!");
			return;
		}
		
		//如果左边有相同就删除掉
		$.each(left,
			function(index,item)
			{
				if(item.Name==detail.Name)
				{
					left.splice(index,1);
					return false;
				}
			}
		);
		
		right.push(detail);
		
		updateOrder();
	}
	
	//添加新的商品
	$("#addNewProduct").click(function()
		{
			var detail=
			{
				Name:$("#name").val(),
				Standards:$("#standards").val(),
				Unit:$("#unit").val(),
				MaterialType:$("#materialtype").val(),
				Price:$("#price").val(),
				Amount:$("#amount").val(),
				Provider:$("#provider").val(),
				ProductID:$("#productid").val()
			};
			
			addOrderDetail(detail);
			$("#name").val("").focus();
		}
	).button();
	
	
	
	$("#saveOrder").click(function()
	{
		if(right.length==0)
		{
			alert("请不要提交空订单.");
			return;
		}
		if(confirm("确定要提交订单吗?"))
		{
			$("#loadingDialog").dialog({modal:true});
			$.post("/store/SaveStockOut",
			{
				datas:JSON.stringify(readRight()),
				node:$("#node").val()
			},
			function(result)
			{
				if(result.result=="success")
				{
					$("#loadingDialog").dialog('close');
					loadProducts();
					right=[];
					writeRight();
					updateOrder();
					alert("订单提交完成.")
				}
				else
				{
					$("#loadingDialog").dialog('close');
					alert(result.errorMessage);
				}
			}
			)
			.error(function()
			{
				$("#loadingDialog").dialog('close');
				alert("发生错误,请重试!");		
			}
			);
		}
		
		
		
	}
	).button();
	
	
	//autocomplete
	
	$("#provider").autocomplete(
		{
			minLength:0,
			source:function(req,res)
			{
				var units=[];
				$.each(allProducts,
					function(index,item)
					{
						if(!item.Provider)
							return;
						if(units.indexOf(item.Provider)<0
							&&item.Provider.indexOf(req.term)>=0
						)
							units.push(item.Provider);
					}
				);
				res(units);
			},
			focus:function(event,ui)
			{
				$("#provider").val(ui.item.value);
				return false;
			},
			select:function(event,ui)
			{
				$("#provider").val(ui.item.value);
				return false;
			}	
		}				
		)
		.data("autocomplete")._renderItem=
		function(ul,item)
		{
			return $("<li style='text-align:left;'>")
			.data("item.autocomplete",item)
			.append("<a>"+item.value+"</a>")
			.appendTo(ul);
		};
	
	$("#unit").autocomplete(
		{
			minLength:0,
			source:function(req,res)
			{
				var units=[];
				$.each(allProducts,
					function(index,item)
					{
						if(!item.Unit)
							return;
						if(units.indexOf(item.Unit)<0)
							units.push(item.Unit);
					}
				);
				res(units);
			},
			focus:function(event,ui)
			{
				$("#unit").val(ui.item.value);
				return false;
			},
			select:function(event,ui)
			{
				$("#unit").val(ui.item.value);
				return false;
			}	
		}				
		)
		.data("autocomplete")._renderItem=
		function(ul,item)
		{
			return $("<li style='text-align:left;'>")
			.data("item.autocomplete",item)
			.append("<a>"+item.value+"</a>")
			.appendTo(ul);
		};
		
		$("#name").autocomplete(
		{
			minLength:0,
			source:function(req,res)
			{
				res($.grep(allProducts,
						function(item,index)
						{
							return item.Name.indexOf(req.term)>=0||
							   item.PY.indexOf(req.term)>=0;
						}
					));
			},
			focus:function(event,ui)
			{
				
				return false;
			},
			select:function(event,ui)
			{
				for(var p in ui.item)
				{
					$("#"+p.toLowerCase()).val(ui.item[p]);
				}
				return false;
			}	
		}				
		)
		.data("autocomplete")._renderItem=
		function(ul,item)
		{
			return $("<li style='text-align:left;'>")
			.data("item.autocomplete",item)
			.append("<a>"+item.Name+"</a>")
			.appendTo(ul);
		};
}
);