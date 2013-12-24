$(document).ready(function()
{
	
	//改变主框架的颜色
	$("#main,footer").css({'background-color': '#5C87B2'});
	
	$("input.Date").datepicker({
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
	.val(new Date().format("yyyy-mm-dd")).button();
		
	var providers=[];
	

	
	$.get("/store/GetProviders",
		function(datas)
		{
			providers=datas;
			
			//autocomplete
			$("input.Provider")
			.val(providers[0].Provider)
			.autocomplete(
			{
				minLength:0,
				source:function(req,res)
				{
					res(providers);
				},
				focus:function(event,ui)
				{
					$("input.Provider").val(ui.item.Provider);
					return false;
				},
				select:function(event,ui)
				{
					$("input.Provider").val(ui.item.Provider);
					dataGrid
					.loadData("/store/GetProductsByProvider",{provider:$("input.Provider").val()});
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
		
			dataGrid.loadData("/store/GetProductsByProvider",{provider:$("input.Provider").val()});
		}
	);
	
	
	
	var tempField=$("#node").val()+"_stockin";
	var dataGrid=createOrder(
				$("#ProductList"),
				$("span.search"),
				tempField,
				function(funcSuccess,funcFail)
				{
					var provider=$("input.Provider").val();
					if(!provider)
					{
						alert("请输入供货商!!!");
						return;
					}
					$.post("/store/SaveStockIn",
						{
							provider:provider,
							datas:localStorage[tempField],
							time:$("input.Date").val()
						},
						function(result)
						{
							if(result.result==="success")
							{
								funcSuccess();						
								dataGrid.loadData("/store/GetProductsByProvider",{provider:$("input.Provider").val()});
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
					detail.Provider=$("input.Provider").val();
					return detail;
				},
				false //放弃多余的商品
			);
	
	$("input.Provider").change(
		function()
		{
			dataGrid.loadData("/store/GetProductsByProvider",{provider:$(this).val()});
		}
	);

	$("#saveOrder").button().val("提交进货单");
	
	$("#printOrderTemplate").hide();

}
);