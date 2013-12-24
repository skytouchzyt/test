createOrder=function($table,$search,tempField,funcSubmitData,funcAddNewProduct,abandonLeft)
{
	var canSearch=true;	
	var handle; //setTimeout

	
	//排序,或者重新排列
	function sort(term,top)
	{
		if(!term)
		{
			$table.dataGrid(
				"sort",
				{
					sort:function(a,b)
					{
						if(a.Index<b.Index) return -1;
						if(a.Index>b.Index) return 1;
						return 0;
					}
				}
			)
			return;
		}
		
		
		
		var found=[];
		var num=parseInt(term);
		$("tr.datarow",$table)
		.removeClass("found")
		.each(
			function()
			{
				var data=$(this).data("data");
				
				if(isNaN(num))
				{
					if((data.PY&&data.PY.indexOf(term)>=0)||data.Name.indexOf(term)>=0)
					{
						found.push($(this));
					}
				}
				else
				{
					if(num===data.ProductID-0)
					{
						found.push($(this));
					}
				}
			}
		);
		
		$.each(found,
			function()
			{
				if(top)
				{
					$(this)
					.addClass("found")
					.insertAfter($("tr.header",$table));
				}
				else
				{
					$(this)
					.addClass("found")
					.appendTo($table);
				}
				
			}
		);
	}
	
	//处理按键,用于搜索
	$("input.search").keypress(
		function(e)
		{
			if(e.keyCode===13)
			{
				sort($(this).val(),$(this).is(".top"));
				$(this).val("");
				return false;
			}			
		}
	);
	
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
			.appendTo("#MaterialType");
		}
	);
	
	
		//如果数量为0就显示红色,否则就是显示白色
	function setColorByAmount($row)
	{
		var detail=$row.data("data");	
		var $amountColumn=$("input.amount",$row);
		if(detail.Amount==0)
		{
			$amountColumn.css({color:'pink'});
			$row.css({background:''});
		}
		else if(detail.Amount>0)
		{
			$amountColumn.css({color:'white'});
			$row.css({background:'#336666'});
		}
		else 
		{
			$amountColumn.css({color:'red'});
			$row.css({background:'#663399'});
		}
	}
	
	//临时保存数据,或者读取
	function tempData(datas)
	{
		if(datas) //不为空就写入
		{
			localStorage[tempField]=JSON.stringify(datas);
		}
		else
		{
			if(localStorage[tempField])
				return JSON.parse(localStorage[tempField]);
			else
				return [];
		}
	}
	
	
	//更新订单
	function updateOrder(details,onRenderRow)
	{
		
		var temp=tempData();
		$.each(details,
			function(index,detail)
			{
				detail.LastAccess=new Date(parseInt(detail.LastAccess.replace(/\/Date\((-?\d+)\)\//, '$1')))
				.format("yyyy-mm-dd");
				$.each(temp,
					function(index,item)
					{
						if(item.Name===detail.Name)
						{
							detail.Amount=item.Amount;
							detail.Price=item.Price;
							temp.splice(index,1);
							return false;
						}
					}
				);
			}
		);
		
		//把数据库中没有的商品加入进来
		if(!abandonLeft)
		{
			$.each(temp,
				function(index,item)
				{
					details.push(item);
				}
			);
		}
		
		function calcTotal(list)
		{
			var total=0;
			$.each(list,
				function()
				{
					total+=this.Amount*this.Price;
				}
			);
			$("span.total").html("￥"+total.round(2));
		}
		
		calcTotal(details);
					
		$table.dataGrid(
			{
				getDataAddress:details,
				onFormatData:function(rowData,dataField)
				{
					if(dataField=="Total")
					{
						return "￥"+(rowData.Amount*rowData.Price).round(4);
					}
					else if(dataField=="Price")
					{
						return "￥"+rowData[dataField];
					}
					else if(dataField=="ProductID")
					{
						return "P"+(rowData[dataField]-0+10000)
						.toString().substr(1);
					}
					else
						return rowData[dataField];					
				},
				onRenderRow:function($row)
				{
					$("input.amount",$row)
					.unbind()
					.change(
						function()
						{
							$row.data("data").Amount=$(this).val();
							$table.dataGrid("updateRowUI",$row);			
							var datas=[];
							var total=0;
							$row
							.parent()
							.children("tr.datarow")
							.each(
								function()
								{
									var data=$(this).data("data");
									if((data.Amount-0)===0)
										return;
									datas.push(data);
									
								}
							);
							
							tempData(datas);
							
							calcTotal(datas);
						}
					)
					.blur(function(){canSearch=true;})
					.focus(
						function()
						{
							canSearch=false;
							var $number=$(this);
							setTimeout(
								function()
								{
									$number.select();	
								},
								100
							);
						}
					)
					.keypress(
						function(e)
						{
							if(e.keyCode===13) //回车就切换到下一行
							{
								$(this)
								.change()
								.parents("tr.datarow")
								.next("tr.datarow")
								.find("input.amount")
								.focus();
							}
							return true;
						}
					)
					.val($row.data("data").Amount);	
					
					$("input.price",$row)
					.unbind()
					.change(function()
						{
							$row.data("data").Price=$(this).val();
							$table.dataGrid("updateRowUI",$row);
						}
					)
					.val($row.data('data').Price);
					
					setColorByAmount($row);
					
					if(onRenderRow)
						return onRenderRow($row);
					return $row;
				}
			}
		);	
	}
		
	//添加新的商品
	$("form.formAddProduct").submit(function()
		{
			
			var detail={};
			
			$("input[type='number'],input[type='text'],select",this)
			.each(function()
			{
				var field=$(this).attr("id");
				detail[field]=$(this).val();	
			}
			);
			
			detail.ProductID=0;
						
			if(funcAddNewProduct)
			{
				detail=funcAddNewProduct(detail);
			}
			
			var found=false;
			$("tr.datarow:contains("+detail.Name+")",$table)
			.each(
				function()
				{
					if($(this).data("data").Name===detail.Name)
					{
						found=true;
						return false;
					}
				}
			);
			
			if(found)
			{
				alert("上面列表中已经有此类商品,请勿重复加入!");
				return false;
			}
			
			$table.dataGrid("appendRow",detail);
			
			$("tr.datarow input.amount:last",$table).change();
			
			return false;
		}
	);
	
	$("#addNewProduct").button();
	
	
	
	$("#saveOrder").click(
		function()
		{
			if(!localStorage[tempField]||localStorage[tempField]==="[]")
			{
				alert("请不要提交空订单.");
				return;
			}
			if(confirm("确定要提交订单吗?"))
			{
				$("#loadingDialog").dialog({modal:true});
				funcSubmitData(
					function() //success
					{
						delete localStorage[tempField];
						$("#loadingDialog").dialog('close');
						alert("提交订单成功.")
					},
					function(errorMessage) //fail
					{
						$("#loadingDialog").dialog('close');
						alert(errorMessage);
					}
				);
			}
		
	}
	).button();
	
	//autocomplete
	$("#Name").autocomplete(
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
					$("#"+p).val(ui.item[p]);
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
	
	
	return {
		loadData:function(address,params,onRenderRow)
		{
			if(params)
			{
				$.getJSON(address,
				params,
				function(datas)
				{
					if(isArray(datas))
					{
						updateOrder(datas,onRenderRow);
					}
				}
				);
			}
			else
			{
				$.getJSON(address,
				function(datas)
				{
					if(isArray(datas))
					{
						updateOrder(datas,onRenderRow);
					}
				}
				);
			}
			
		}
	};
}
