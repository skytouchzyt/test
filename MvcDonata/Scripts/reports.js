$(document).ready(
function()
{



   


	var AllDatas={};
	var lastTime=null;
	var weekDays=["周日","周一","周二","周三","周四","周五","周六"];
	var address="/reports/getweeklydata";
	var type='column';
	var days=weekDays;
	var refreshData=refreshDataForWeek;
	var refresh=true; //是否自动刷新数据
	
	$("ul#mode a").click(
		function()
		{
			var command=$(this).html();
			
			switch(command)
			{
				case '忙碌统计':
					type='spline';
					address="/reports/GetBusyStatus";
					refreshData=refreshDataForBusy;
					refresh=false;
					break;
				case '一周统计':
					type='column';
					address="/reports/getweeklydata";
					refreshData=refreshDataForWeek;
					refresh=true;
					break;
				case '一月统计':
					type='spline';
					address="/reports/getmonthlydata";
					refreshData=refreshDataForMonth;
					refresh=false;
					break;
				case '一年统计':
					alert('此功能还未完成.');
					return;
					type='spline';
					address="";
					refreshData=refreshDataForYear;
					refresh=false;
					break;
				case '运营统计':
					alert('此功能还未完成.');
					return;
					type='spline';
					address="";
					refresh=false;
					break;
			}
			$("ul#mode a").removeClass('selected');
			$(this).addClass("selected");
			$("h2").html("多纳达"+command);
			GetAllDatas();	
		}
	);
	
	 function showChart(title,subtitle,data,createxAxisTitle)
	 {	
	 	
	 	
	 			
		// prepare HightChart settings
		
		var dineTotal=JSLINQ(data).Sum(function(item){return item['堂食'];}).round(0);
		var phoneTotal=JSLINQ(data).Sum(function(item){return item['外送'];}).round(0);
		
		var dinePercent=(dineTotal/(dineTotal+phoneTotal)*100).round(2);
		
		var phonePercent=(phoneTotal/(dineTotal+phoneTotal)*100).round(2);
		
		
		var settings = {
			chart :
	        {
	            renderTo : "reports",
	            defaultSeriesType : type,
	            marginRight : 25,
	            marginBottom : 25
	        },
	        title :
	        {
	            text : title,
	            x : -20
	        },
	        subtitle :
	        {
	            text : subtitle,
	            x : -20
	        },
	        xAxis :
	        {
	            categories : createxAxisTitle()
	        },
	        yAxis:
	        {
	            allowDecimals:false,
	            min:0,
	            title : { text : "金额(元)" },
	            plotLines : 
	                  [ 
	                      { value : 0, width : 1, color : "#808080" } ,
	                      { value : 0, width : 1, color : "#336699" },
	                      { value : 0, width : 1, color : "#888088" }
	                  ]
	        },
	        plotOptions:
	        {
	        	pie:
	        	{
	        		allowPointSelect:true,
	        		cursor:'pointer'
	        	}
	        },
	        legend :
	        {
	            layout : "vertical",
	            align : "right",
	            verticalAlign : "top",
	            x : -50,
	            y : 0,
	            borderWidth : 1
	        },
	        series :
	        [
         		{
                    name:"总计",
                    data:JSLINQ(data).Select(function(item){return item['总计'];}).ToArray()
         		},
         		{
                    name:"堂食",
                    data:JSLINQ(data).Select(function(item){return item['堂食'];}).ToArray()
         		},
         		{
                    name:"外送",
                    data:JSLINQ(data).Select(function(item){return item['外送'];}).ToArray()
         		},
         		{
         			type:'pie',
         			name:'堂食外送比例',
         			data:[
         					{
	         					name:'堂食',
	         					y:dinePercent,
	         					color:'#AA4643'
         					},
         					{
         						name:'外送',
	         					y:phonePercent,
	         					color:'#89A54E'
         					}
         				 ],
	 				 center:[600,50],
	 				 size:50,
         		     showInLegend:true,
         		     dataLabels:{
         		     	enabled:false
         		     }		 
         		}
	        ],
	        tooltip:
	        {
	        	formatter: function() {
							var s;
							if (this.point.name) { // the pie chart
								s = ''+
									this.point.name +': '+ this.y +'%';
							} else {
								s = ''+
									this.x  +this.series.name+': ￥'+ this.y;
							}
							return s;
						}
	        }
		};
		
		var c=new Highcharts.Chart(settings);
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
		if(!address)
		{
			AllDatas=[];
			refreshData();
		}
		else
		{
			$.getJSON(address,
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
		
	}

	
	$("#selectedNode").change(
		function()
		{
			refreshData();
		}
	);
	
	function calcAverage(node)
	{
		var averageDine=JSLINQ(AllDatas.DineOrders)
					.Where(
						function(item)
						{
							return item.Node===node;
						}
					)
					.Average(function(item){return item.Total;});
					
		var averagePhone=JSLINQ(AllDatas.PhoneOrders)
					.Where(
						function(item)
						{
							return item.Node===node;
						}
					)
					.Average(function(item){return item.Total;});
					
		return (averageDine+averagePhone).round(0);
	}
	
	
	function refreshDataForBusy()
	{
		
		
		var node=$("#selectedNode").val();		
		
		
		function getTotal(list,hour)
    	{
    		return JSLINQ(list)
    			   .Where(
    			   		function(dine)
    			   		{
    			   			return dine.Hour==hour&&dine.Node===node;
    			   		}
    			   )
    			   .Select(
    			   		function(item)
    			   		{
    			   			return item.Total;
    			   		}
    			   )
    			   .FirstOrDefault(0);
    	}
    	
    	var totals=JSLINQ(AllDatas.Hours).Select(
                    	function(hour)
                    	{
                    		return getTotal(AllDatas.Dine,hour)+getTotal(AllDatas.Phone,hour);
                    	}
                    ).ToArray();
                    
       var dines=JSLINQ(AllDatas.Hours)
       		     .Select(function(hour){return getTotal(AllDatas.Dine,hour);})
       		     .ToArray();
		
		var phones=JSLINQ(AllDatas.Hours)
				   .Select(function(hour){return getTotal(AllDatas.Phone,hour);})
				   .ToArray();
		
		var settings = {
			chart :
	        {
	            renderTo : "reports",
	            defaultSeriesType : type,
	            marginRight : 25,
	            marginBottom : 25
	        },
	        title :
	        {
	            text : "最近30天忙碌统计",
	            x : -20
	        },
	        xAxis :
	        {
	            categories : JSLINQ(AllDatas.Hours).Select(function(hour){return hour+"点";}).ToArray() 
	        },
	        yAxis:
	        {
	            allowDecimals:false,
	            min:0,
	            title : { text : "金额(元)" },
	            plotLines : 
	                  [ 
	                      { value : 0, width : 1, color : "#808080" } ,
	                      { value : 0, width : 1, color : "#336699" },
	                      { value : 0, width : 1, color : "#888088" }
	                  ]
	        },
	        plotOptions:
	        {
	        	pie:
	        	{
	        		allowPointSelect:true,
	        		cursor:'pointer'
	        	}
	        },
	        legend :
	        {
	            layout : "vertical",
	            align : "right",
	            verticalAlign : "top",
	            x : -50,
	            y : 0,
	            borderWidth : 1
	        },
	        series :
	        [
         		{
                    name:"总计",
                    data:totals
         		},
         		{
                    name:"堂食",
                    data:dines
          
         		},
         		{
                    name:"外送",
                    data:phones
         		}
	        ],
	        tooltip:
	        {
	        	shared:true
	        }
		};
		
		if(node==="二外店") //如果是呼叫中心
		{
			settings.series.push(
				{
					name:"呼叫中心",
					data:JSLINQ(AllDatas.Hours).Select(
						function(hour)
						{
							return JSLINQ(AllDatas.Phone).Where(
								function(phone)
								{
									return phone.Hour==hour;
								}
							).Sum(function(phone){return phone.Count;});
						}
					).ToArray()
				}
			);
		}
		
		var c=new Highcharts.Chart(settings);
	}

	
	function refreshDataForMonth()
	{
		var node=$("#selectedNode").val();
		
		var end=new Date(AllDatas.End).getDate();
		var days=[];
		for(var i=1;i<=end;i++)
		{
			days[days.length]=i;
		}
		
		
		
		var displayDatas=JSLINQ(days)
		.Select(
			function(day)
			{
				var dine=JSLINQ(AllDatas.DineOrders)
							.Where(
								function(item)
								{
									return item.Date===day&&item.Node===node;
								}
							)
							.Select(
								function(item){return item.Total;}
							).FirstOrDefault(0);
							
				var phone=JSLINQ(AllDatas.PhoneOrders)
							.Where(
								function(item)
								{
									return item.Date===day&&item.Node===node;
								}
							)
							.Select(
								function(item){return item.Total;}
							).FirstOrDefault(0);
				return {
					Day:day,
					'堂食':dine,
					'外送':phone,
					'总计':dine+phone
				};
			}
		).ToArray();
		
		
		showChart(
			stringFormat("多纳达{0}({1}到{2})销售情况", node, AllDatas.Start, AllDatas.End),
			stringFormat("总计:￥{0}元,平均每天:￥{1}元", JSLINQ(displayDatas).Sum(function(item){return item['总计'];}).round(0), calcAverage(node)),
			displayDatas,
			function()
			{
				return JSLINQ(displayDatas).Select(function(item){return item.Day+"号";}).ToArray();
			}
			);
			
	}

	function refreshDataForWeek()
	{
		var node=$("#selectedNode").val();
		
		var displayDatas=JSLINQ(weekDays)
		.Select(
			function(day)
			{
				var dine=JSLINQ(AllDatas.DineOrders)
							.Where(
								function(item)
								{
									return weekDays[item.IndexOfWeek]===day&&item.Node===node;
								}
							)
							.Select(
								function(item){return item.Total;}
							).FirstOrDefault(0);
							
				var phone=JSLINQ(AllDatas.PhoneOrders)
							.Where(
								function(item)
								{
									return weekDays[item.IndexOfWeek]===day&&item.Node===node;
								}
							)
							.Select(
								function(item){return item.Total;}
							).FirstOrDefault(0);
				return {
					Day:day,
					'堂食':dine,
					'外送':phone,
					'总计':dine+phone
				};
			}
		).ToArray();
		
		

		

		
		showChart(
			stringFormat("多纳达{0}({1}到{2})销售情况", node, AllDatas.Start, AllDatas.End),
			stringFormat("总计:￥{0}元,平均每天:￥{1}元", JSLINQ(displayDatas).Sum(function(item){return item['总计'];}).round(0),calcAverage(node)),
			displayDatas,
			function()
			{
				return JSLINQ(displayDatas).Select(function(item){return item.Day;}).ToArray();
			}
			);
				
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
		if(refresh&&Math.abs(diff/60/1000)>10) //和目前的时间相隔大于10分钟,才刷新数据
		{								 
			GetAllDatas();
		}
	}
	,60000);
}
);