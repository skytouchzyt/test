$(document).ready(function()
{
	//改变主框架的颜色
	$("#main,footer").css({'background-color': '#5C87B2'});
	
	var $header=$("#ProductList tr.header");
	
	$("td:contains('单价')",$header).attr("datafield","Price");
	
	$("#printOrderTemplate").hide();
	
	$("#saveOrder").val("盘点");
	
	
	$("td.need").show();
	
	$("td.operation").show();

	createOrder(
		$("#ProductList"),
		$("span.search"),
		"", /*不需要保存数据*/
		function(funcSuccess,funcFail)
		{
			var details=[];
			$("#ProductList tr.datarow").each(
				function()
				{
					var p=$(this).data("data");
					if(p.Amount>0) 
					{
						details.push(p);
					}
							
				}
			);
			
			$.post("/store/CheckStock",
				{datas:JSON.stringify(details)},
				function(result)
				{
					if(result.result==="success")
					{
						funcSuccess();						
						window.location="/store";
					}
					else
					{
						funcFail(result.errorMessage);
					}
				}
			)
			.error(function()
			{
				funcFail("发生错误,请重试!")		
			}
			);
		},
		function(detail)
		{
			detail.PY="";
			return detail;
		}
	).loadData("/store/GetStock",{datas:$("#orders").val()},
		function($row)
		{
			$row.attr("title",$row.data("data").Provider);
		}
	);
	
	$("div.provider").show();
	
	$("#Provider")
	.attr("placeholder","必须填写")
	.attr("required",true);
	
	//autocomplete
	$.get("/store/GetProviders",
		function(r)
		{
			if(isArray(r))
			{
				
				
				$("<option />")
				.val("全部")
				.html("全部")
				.appendTo("select.Provider");
						
				$.each(r,function()
					{
						$("<option />")
						.val(this.Provider)
						.html(this.Provider)
						.appendTo("select.Provider");
					}
				);
				
				$("select.Provider").change(
					function()
					{
						var selected=$(this).val();
						if(selected==="全部")
						{
							$("#ProductList tr.datarow").show();
						}
						else
						{
							$("#ProductList tr.datarow").each(
								function()
								{
									if($(this).data("data").Provider===selected)
									{
										$(this).show();
									}
									else
									{
										$(this).hide();
									}
								}
							);
						}
					}
				);
				
				$("#Provider").autocomplete(
				{
					minLength:0,
					source:function(req,res)
					{
						
						res(r);
					},
					focus:function(event,ui)
					{
						$("#Provider").val(ui.item.Provider);
						return false;
					},
					select:function(event,ui)
					{
						$("#Provider").val(ui.item.Provider);
						return false;
					}	
				}				
				)
				.data("autocomplete")._renderItem=
				function(ul,item)
				{
					return $("<li style='text-align:left;'>")
					.data("item.autocomplete",item)
					.append("<a>"+item.Provider+"</a>")
					.appendTo(ul);
				};
				
			}
		}
	)
	
	
}
);
