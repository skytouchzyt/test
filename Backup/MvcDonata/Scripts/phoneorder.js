﻿$(document).ready(
	function()
	{
		

		
		$("#createCall").click(function()
		{
			var phone=$("#phone").val();
			if(phone)
			{
				var args=phone.split(" ");
				if(args.length==2&&args[0]==="devicenumber")
				{
					localStorage.deviceIndex=args[1];
					$("#phone").val("");
					alert("设定设备号为:"+args[1]);
					return;
				}
			}
			$.post("/phoneorder/createnewphonecall",
			{
				number:localStorage.deviceIndex,
				phone:$("#phone").val(),
				time:new Date().format("yyyy-mm-dd HH:MM:ss")
			},
			function(result)
			{
			}
			);
			
		}
		);
		
		$(document).keydown(function(e)
		{
			if(e.keyCode==122) //F12
			{
				var result=window.prompt("请输入电话设备号:",localStorage.deviceIndex||1);
				if(result)
					localStorage.deviceIndex=result;
				return false;
			}
		}
		);
		
		function getNodes()
		{
			
			$.getJSON("/dns/getnodes",
			{
				server:localStorage.server||""
			},
			function(nodes)
			{
				var $nodes=$("ul#nodesList");
				var $template=$("li.template",$nodes);
				//清空以前的数据
				$("li:not(.template)",$nodes).remove();
				
				$.each(nodes,function()
				{
					var $node=$template.clone().removeClass("template");
					var updatedTime=new Date(parseInt(this.UpdatedTime.replace(/\/Date\((-?\d+)\)\//, '$1')));
					if(this.TimeSpan<=3)
					{
						$("div",$node).addClass("online");
					}
					else
					{
						$("div",$node).addClass("offline");
					}
					$("span.node",$node).html(this.Node);
					$node.appendTo($nodes).show();
				}
				);
			}
			);
		}
				
		//30秒刷新分店信息
		setInterval(getNodes,30000);
		
		function refreshData()
		{
			$("#orderList").dataGrid();
					
		}
		
		//检测是否有新的电话打入
		//如果有就弹出新窗口
		var getNewCall=true;
		setInterval(
		function()
		{
			if(localStorage.deviceIndex&&getNewCall)
			{
				getNewCall=false;
				$.get("/phoneorder/getnewphonecall",{number:localStorage.deviceIndex},
				function(phone)
				{
					getNewCall=true;
					if(phone)
					{
						var win=window.open(
						"/phoneorder/newphoneorder?phone="+phone,
						null,
						'top=0,left=0,toolbar=yes,menubar=yes,scrollbars=no, resizable=no,location=no, status=no,fullscreen=yes'
						);
											
					}
				}
				)
				.error(function()
					{
						getNewCall=true;
					}
				);
			}
			//让不在线的分店闪动
			$("ul#nodesList li div.offline").effect("pulsate",500);
		}
		,3000);
		
		
		//刷新订单
		setInterval(
		function()
		{
			var diff=new Date($("#txtSelectedDate").val())-new Date();
			if(Math.abs(diff/24/60/60/1000)<1) //和目前的时间相隔小于一天,才刷新数据
			{								  //如果显示的是过去的记录就不用刷新
				refreshData();
			}
		}
		,60000);
		
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
				alternation:true,
				getDataAddress:"/phoneorder/getphoneorders",
				getDataParams:function(params)
				{
					params.node=$("#selectedNode").val();
					params.selectedDate=$("#txtSelectedDate").val();
					params.server=localStorage.server||"";
					return params;
				},
				onRenderRow:function($row)
				{
					var data=$row.data("data");
					//超过3分钟没有收到的外送订单就显示红色
					if(!data.Received)
					{
						var time=new Date(parseInt(data.Time.replace(/\/Date\((-?\d+)\)\//, '$1')));
						var span=(new Date()-time)/60/1000;
						if(span>=3&&span<=30)
						{
							$row.css({'background-color':'red'});
							
							//报警
							$("#ring")
							.remove()
							.clone()
							.appendTo('body')[0].play();
						}
					}
					$row.css({cursor:'pointer'});
					return $row;
				},
				onReceivedData:function(result)
				{
					if(localStorage.server)
					{
						$("#server").html("主服务器地址:"+localStorage.server);
					}
					else
					{
						$("#server").html("当前服务器为主服务器.");
					}
					
					//显示订单总数
					$("#totalAmount").html("单子总数:"+result.Amount);
					
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
					if(dataField=="Time")
					{
						var data=row[dataField];
						var temp=new Date(parseInt(data.replace(/\/Date\((-?\d+)\)\//, '$1')));
						return temp.format("HH:MM:ss");
					}
					else if(dataField=="Phone")
					{
						//格式化显示电话
						var phone=row["Customer"].Phone;
						var newphone=[];
						for(var i=phone.length-1;i>=0;i--)
						{
							if((phone.length-i-1)%4==0&&newphone.length>0)
							{
								newphone.push("-");
							}
							newphone.push(phone[i]);
						}
						return newphone.reverse().join("");
					}
					else if(dataField=="Address")
					{
						return row["Customer"].Addresses[0].City+" "+row["Customer"].Addresses[0].Address;
					}
					else if(dataField=="Received")
					{
						if(row[dataField])
							return "收到";
						else
							return "未收到";
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
							
                            //先插入在画表格，这样表格的动画才能表现出来
							$detailsTable
                                .appendTo($detailsRow.children("td:gt(0)"))
                                .dataGrid(
							{
							    getDataAddress: "/phoneorder/getphoneorderdetails",
							    getDataParams: function (params) {
							        return {
							            OrderID: $currentRow.data("data").ID,
							            server: localStorage.server || ""
							        };
							    },
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
		
		 //获取餐品的名字,去掉后面的数字9或者12或者(
		function getRealName(dishName)
		{
			var name="";
			for(var i=0;i<dishName.length;i++)
			{
				if(dishName[i]>='0'&&dishName[i]<='9')
					break;
				else if(dishName[i]=="(")
					break;
				else
					name+=dishName[i];
			}
			return name;
		}
		
		//加载餐品
		$.getJSON("/dishes/getdishes",{node:"",orderType:"外卖",encoding:false},
		function(dishes)
		{
			localStorage.dishes=JSON.stringify(dishes);
			
			$.each(dishes,function()
			{
				var realName=getRealName(this.Name);
				
				//检测图片是否已经加载过
				if($("div#imageCache img[name='"+realName+"']").length>0)
					return;
				var img=new Image();
				img.src="../../content/images/"+realName+".jpg";
				img.name=realName;
				$(img).appendTo("div#imageCache");
			}
			);
		}
		);
	}
);