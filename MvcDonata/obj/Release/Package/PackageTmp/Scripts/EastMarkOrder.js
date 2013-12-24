$(document).ready(
	function()
	{

		$("input.search").css({width:'250px'});
		
		$("#printOrderTemplate").hide();
		
		var $header=$("#ProductList tr.header");
		
		var hiddenColumns=
		[
			$("td:contains('编号')",$header).index(),
			$("td:contains('规格')",$header).index(),
			$("td:contains('单位')",$header).index(),
			$("td:contains('类别')",$header).index(),
			$("td:contains('小计')",$header).index()			
		];
		
		function hideColumns($row)
		{
			$.each(hiddenColumns,
					function()
					{
						$("td:eq("+this+")",$row).hide();
					}						
				);
		}
		
		hideColumns($header);
		
		
		var tempField=$("#node").val()+"_newEastMarketOrderDetails";
		createOrder(
			$("#ProductList"),
			$("span.search"),
			tempField,
			function(funcSuccess,funcFail)
			{
				$.post("/store/SaveStockIn",
					{
						datas:localStorage[tempField],
						provider:"东郊市场/西南郊市场",
						time:new Date().format("yyyy-mm-dd")
					},
					function(result)
					{
						if(result.result==="success")
						{
							funcSuccess();						
							//window.location="/store/order";
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
				detail.Price=0;
				return detail;
			},
			true			
			)
			.loadData("/store/getEastMarkOrderDetails",{datas:$("input#ids").val()},
				function($row)
				{
					hideColumns($row);
					
				},
				function($table)
				{
					$("tr.datarow",$table).each(
						function()
						{
							$("td:eq(1)",this).click(function()
								{
									$(this)
									.parent()
									.addClass("done")
									.appendTo($table);								
								}
							);
							//$(this).removeAttr("style");
						}						
					);
				}
			);	
	}
);
