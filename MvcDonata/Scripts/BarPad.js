$(document).ready(
	function()
	{
		var donotclick=false;
		
		$.get("/dishes/getbars",
			function(bars)
			{
				if(isArray(bars))
				{
					bars.push("出餐台");
					$.each(bars,function(i,b)
						{
							$("<option />")
							.val(b)
							.html(b)
							.appendTo("#defaultBar");
						}
					);
					
					init();
				}
			}
		);

		
		function onChangeBar()
		{
			document.title=localStorage.defaultBar;
			if(localStorage.defaultBar==="出餐台")
			{
				$("ul.menu li:first a").html("出餐");				
			}
			else
			{
				$("ul.menu li:first a").html("制作");	
			}
		}
		
		function init()
		{
			//加载设置
			$("#defaultBar")
			.val(localStorage.defaultBar||"出餐台")
			.change(function()
				{
					localStorage.defaultBar=$(this).val();
					onChangeBar();
				}
			);
			
			
			
			onChangeBar();
	
			$("#before")
			.val(localStorage.before||60)
			.change(function()
				{
					localStorage.before=$(this).val();
				}
			);
			
			$("#index")
			.val(localStorage.index||"0")
			.change(function()
			{
				localStorage.index=$(this).val();
				$("#labelIndex").html($(this).val());
			}
			);
			
			$("#index").change();
			
			
			$("#isRing")
			.attr("checked",localStorage.isRing?true:false)
			.click(function()
				{
					if($(this).check())
						localStorage.isRing=true;
					else
						delete localStorage.isRing;
				}
			);
			
			
			$("#displayAll")
			.attr("checked",localStorage.displayAll?true:false)
			.click(function()
			{
				if($(this).check())
						localStorage.displayAll=true;
				else
					delete localStorage.displayAll;
			}
			);
					
			var $template=$("ul.template li");
			
			function changeDishStatus()
			{
				
				if(donotclick)
				{
					return;
				}
				var dish=$(this)
				.removeClass("prepare")
				.removeClass("making")
				.addClass("clicked")
				.data("data");
				
				var status=dish.Status;
				
				//如果是出餐台
				var currentBar=localStorage.defaultBar;
				if(currentBar==="出餐台")
				{
					if(dish.Status==="上餐")
					{
						dish.Status="完成";
					}
					else if(dish.Status==="完成")
					{
						dish.Status="制作";    
					}  
					else //否则就改变桌号
					{
						var table=prompt("请输入新的桌号:",dish.TableNumber);
						if(table)
						{
							$.post("/dineorder/changetable",{barCode:dish.OrderBarCode,newTable:table});
						}
					} 
				}
				else if(currentBar==="收银台")
				{
					
				}
				else //其它后厨台位
				{
					
					if(dish.Status==="上餐")
					{
						dish.Status="制作";	
					}
					else if(dish.Status==="准备")
					{
						dish.Status="制作";
					}
					else if(dish.Status==="制作")
					{
						dish.Status="上餐";
					}
					
				}
				
				//如果状态改变了才提交给本地服务器,否则什么也不干
				if(status!==dish.Status)
				{
					$.post("/dineorder/updateDishStatus",
					{
						statusID:dish.ID,
						status:dish.Status
					}
					);
				}				
			}
			
			function createUpdateDish(status,$dish)
			{			
				var $new=$dish||$template.clone();
				$new.data("data",status)
				.addClass("updated")
				.removeClass("clicked")
				.unbind()
				.click(changeDishStatus);
				
				$("span.name",$new).html(status.Name);
				$("span.remark",$new)
				.html((status.TableNumber?(status.TableNumber+"号"):"")+status.Remark);
				
				if(localStorage.defaultBar==="出餐台")
				{
					$("span.time",$new).html(status.Time+"("+status.TimeSpan+"分)"+"["+status.CustomerCount+"人]");
				}
				else
				{
					$("span.time",$new).html(status.Time+"("+status.TimeSpan+")");	
				}
						
				$("input.id",$new).val(status.ID);			
				
				
				
				if(status.Remark.indexOf('外送')>=0||status.Remark.indexOf("外卖")>=0||status.Remark.indexOf('微信')>=0)
				{
					$("span.name",$new).addClass("phone");
					
					if (localStorage.defaultBar === "出餐台") {
					    $new.hide();
					}
					else {
					    $new.show();
					}
				
				}

				else if(status.Remark.indexOf("外带")>=0||status.Remark.indexOf('打包带走')>=0)
				{
					$("span.name",$new).addClass("takeaway");
				}
				else
				{
					$("span.name",$new).addClass("dine");
				}
				if(status.Status==="准备")
				{
					$new.addClass("prepare")
					.removeClass("go")
					.removeClass("making");
				}
				else if(status.Status==="制作")
				{
					$new.addClass("making")
					.removeClass("prepare")
					.removeClass("go");
					
				}
				else 
				{
					$new.addClass("go")
					.removeClass("making")
					.removeClass("prepare");
					
				}
				
				
					
				return $new;
			}	
			
			function find_create_or_update($ul,status)
			{
				var $first=$("li.status:first",$ul);
				//首先尝试找
				var $find=$("li  div input.id[value="+status.ID+"]",$ul);
				if($find.length===0) //没有
				{					
					$find=createUpdateDish(status).appendTo($ul);
				}
				else
				{
					createUpdateDish(status,$find.parents("li.status"));
				}


			}
			
			function displayForOutBar(statusList,$doneul,$notdoneul)
			{

				$.each(statusList,function()
					{		
			
						if(this.Status==="完成")
						{
							find_create_or_update($doneul,this);
						}
						else if(localStorage.displayAll||this.Status==="上餐")
						{
							
							find_create_or_update($notdoneul,this);
						}

							
					}
				);
			}
			
			function displayForOtherBars(statusList,$doneul,$notdoneul)
			{
				$.each(statusList,function()
					{
						var $i;
						if(this.Status==="上餐")
						{
							$i=find_create_or_update($doneul,this);
						}
						else if(this.Status==="制作"||this.Status==="准备")
						{
							$i=find_create_or_update($notdoneul,this);
						}
							
					}
				);
			}
			
			
			//响铃
			setInterval(
				function()
				{
					if($("#isRing").check())
					{
						$.get("/ring",
							function(result)
							{
								if(result)
								{
									
									try
									{
										$("#ring")
										.remove()
										.clone()
										.appendTo('body')[0].play();
										
									}
									catch(e)
									{
										alert(e);
									}
									
								}
							}
						);
					}					
				},
				2000
			);
			
			
			var refreshChar=['-','\\','|','/'];
			var refreshIndex=0;
			var speed=0;
			setInterval(
				function()
				{
					if(speed==0)
						return;
						
					refreshIndex=(++refreshIndex%refreshChar.length);
								
					$("#refresh").html(refreshChar[refreshIndex]);
					
					speed--;
					if(speed<0) speed=0;
				},
				200
			);
			
			
			var refresh=true;
			
			setInterval(
				function()
				{
					if(!refresh) return;
					
					refresh=false;
					$.get("/dineorder/GetDishStatus",
						{
							bar:$("#defaultBar").val(),
							mins:$("#before").val(),
							index:$("#index").val()
						},
						function(datas)
						{
							refresh=true;
							if(isArray(datas))
							{
								
								speed+=15;								
								if (speed > 15) speed = 15;
								var $doneul=$("div#done ul");
								var $notdoneul=$("div#notdone ul");;
								
								//取消所有餐品状态的更新标记
								$("li.status",$doneul).removeClass("updated");
								$("li.status",$notdoneul).removeClass("updated");
								
								
								//出餐台特殊,需要单独对待		
								if(localStorage.defaultBar==="出餐台")
								{
									displayForOutBar(datas,$doneul,$notdoneul);
								}
								else if(localStorage.defaultBar==="收银台")
								{
									
								}
								else //其他台位
								{
									displayForOtherBars(datas,$doneul,$notdoneul);
								}
								
								
								//删除不需要的餐品状态
								donotclick=true; //删除餐品时不能点击其他餐品
								$("li.status:not(.updated)",$doneul).remove();
								$("li.status:not(.updated)",$notdoneul).remove();
								donotclick=false;
							}
							
						}					
				).error(function()
					{
						refresh=true;
					}
				)
				},
				3000
			);
			
			
			function selectBar(bar)
			{
				$("div.tabBar").hide();
				
				$(bar).show();
				
				$("ul.menu li a").removeClass("selected");				
				$("ul.menu li a[href="+bar+"]")
				.addClass("selected");
			}
			
			$("ul.menu li").click(
				function()
				{
					var bar=$("a",this).html();
					switch(bar)
					{
						case '出餐':
						case '制作':
							selectBar("#notdone");
							break;
						case '完成':
							selectBar("#done");
							break;
						case '设置':
							selectBar("#dialogSettings");
							break;
						case '全屏':
							launchFullScreen(document.documentElement);
							break;
					}
					return false;
				}
			);
			
			
			function launchFullScreen(element) 
			{
		        if (element.requestFullScreen) {
		            element.requestFullScreen();
		        } else if (element.mozRequestFullScreen) {
		            element.mozRequestFullScreen();
		        } else if (element.webkitRequestFullScreen) {
		            element.webkitRequestFullScreen();
		        }
	    	}
		}
		
        
	}
);



