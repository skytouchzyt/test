$(document).ready(
function()
{
	var AllDatas={};
	var lastTime=null;
	
	 function showChart(title,description,data)
	 {
		
		$("#reports").empty();
			
		// prepare jqxChart settings
		var settings = {
			title: title,
			description: description,
			padding: { left: 5, top: 5, right: 5, bottom: 5 },
			titlePadding: { left: 90, top: 0, right: 0, bottom: 10 },
			source: data,
			categoryAxis:
				{
					dataField: 'Day',
					showGridLines: false
				},
			colorScheme: 'scheme01',
			seriesGroups:
				[
					{
						type: 'column',
						columnsGapPercent: 30,
						seriesGapPercent: 0,
						valueAxis:
						{
							minValue: 0,
							//maxValue: 100,
							unitInterval: 1000,
							description: '金额'
						},
						series: [
								{ dataField: '堂食', displayText: '堂食'},
								{ dataField: '外送', displayText: '外送'},
								{ dataField: '总计', displayText: '总计'}
							]
					}
				]
		};
		
		// select the chartContainer DIV element and render the chart.
		$('#reports').jqxChart(settings);
	}
			
			$("#selectedDate").datepicker({
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
		.change(GetAllDatas).button();
		
		function GetAllDatas()
		{
			$.getJSON("/reports/getweeklydata",
			{date:$("#selectedDate").val()},
			function(datas)
			{
				AllDatas=datas;
				
				var $selected=$("#selectedNode");
				$("option",$selected).remove();
				
				$.each(AllDatas.NodesList,
				function(i,n)
				{
					$("<option />")
					.val(n)
					.html(n)
					.appendTo($selected);
				}
				);
				
				lastTime=new Date();
				
				$("#lastTime").html("统计时间为:"+lastTime.format("HH:MM:ss"));
				
				refreshData();
			}
			);
		}

		
		$("#selectedNode").change(refreshData);
		
		function getTotal(datas,node,indexOfWeek)
		{
			for(var i=0;i<datas.length;i++)
			{
				if(datas[i].IndexOfWeek==indexOfWeek&&datas[i].Node==node)
				{
					return datas[i].Total;
				}
			}
			return 0;
		}
		
		
		// prepare chart data
		// var  sampleData = [
				// { Day:'Monday', Keith:30, Erica:15, George: 25},
				// { Day:'Tuesday', Keith:25, Erica:25, George: 30},
				// { Day:'Wednesday', Keith:30, Erica:20, George: 25},
				// { Day:'Thursday', Keith:35, Erica:25, George: 45},
				// { Day:'Friday', Keith:20, Erica:20, George: 25},
				// { Day:'Saturday', Keith:30, Erica:20, George: 30},
				// { Day:'Sunday', Keith:60, Erica:45, George: 90}
			// ];
			
		var weekDays=["周日","周一","周二","周三","周四","周五","周六"];
		function refreshData()
		{
			var node=$("#selectedNode").val();
			
			var displayDatas=[];
			var totaldine=0;
			var totalphone=0;
			for(var i=0;i<weekDays.length;i++)
			{
				var dine=getTotal(AllDatas.DineOrders,node,i);
				var phone=getTotal(AllDatas.PhoneOrders,node,i);
				var day=
				{
				   Day:weekDays[i],
				   '堂食':dine,
				   '外送':phone,
				   '总计':(dine-0)+(phone-0)
				};
				displayDatas.push(day);
				
				totaldine+=dine;
				totalphone+=phone;
			}
			
			var title=node+"本周营业总计:￥"+(totaldine+totalphone)+"元.";
			var des="其中堂食:￥"+totaldine+"元,外送:￥"+totalphone+"元.";
			
			showChart(title,des,displayDatas);
			
			
			
			
		}
		
		$("#refresh")
		.button()
		.click(
		function()
		{
			GetAllDatas();
		}
		);
		
		GetAllDatas();
		
		
		//刷新订单
		setInterval(
		function()
		{
			var diff=new Date()-lastTime;
			if(Math.abs(diff/60/1000)>10) //和目前的时间相隔大于10分钟,才刷新数据
			{								 
				GetAllDatas();
			}
		}
		,60000);
}
);