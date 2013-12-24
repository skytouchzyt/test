$(document).ready(function()
{

	
	//改变主框架的颜色
	$("#main,footer").css({'background-color': '#5C87B2'});
	
	var $header=$("#ProductList tr.header");
	
	$("td:contains('单价')",$header).attr("datafield","Price");
	
	$("#printOrderTemplate").button()
	.click(
		function()
		{
			var $printButton=$(this);
			$printButton.enable(false);
			if(!confirm("确定要打印订货单模板吗?"))
				return;
			var command="print ordertemplate";
			$.post("/store/CreateCommand",
				{
					node:"",
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
	
	
	
	var tempField=$("#node").val()+"_newOrderDetails";
	createOrder(
		$("#ProductList"),
		$("span.search"),
		tempField,
		function(funcSuccess,funcFail)
		{
			$.post("/store/saveorder",
				{jsonData:localStorage[tempField]},
				function(result)
				{
					if(result.result==="success")
					{
						funcSuccess();						
						window.location="/store/order";
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
		}
	).loadData("/store/GetRecentProducts",null,
		function($row)
		{
			$("input.price",$row).attr("readonly",true);
		}
		);
	
	
   
}
);