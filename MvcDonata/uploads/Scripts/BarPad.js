$(document).ready(
	function()
	{
		var bars=
		[
			"出餐台",
			"水台",
			"饼台",
			"小吃台",
			"主食台"
		];
		
		$.each(bars,function(i,b)
			{
				$("<option />")
				.val(b)
				.html(b)
				.appendTo("#defaultBar");
			}
		);
		
		//加载设置
		$("#defaultBar")
		.val(localStorage.defaultBar||"出餐台")
		.change(function()
			{
				localStorage.defaultBar=$(this).val();
			}
		);

		$("#before")
		.val(localStorage.before||60)
		.change(function()
			{
				localStorage.before=$(this).val();
			}
		);
		
		var currentBar=$("#defaultBar").val();

		var $template=$("ul.template li");
		
		function createUpdateDish(status,$dish)
		{			
			var $new=$dish||$template.clone().data("data",status);
			$("span.name",$new).html(status.Name);
			$("span.tablenumber",$new)
			.html(status.TableNumber+"("+status.CustomerCount+")");
			$("span.remark",$new).html(status.Remark);
			$("span.time",$new).html(status.Time);			
			$("input.id",$new).val(status.ID);			
			return $new;
		}	
		
		function find_create_or_update($ul,status)
		{
			//首先尝试找
			var $find=$("li  div input.id[value="+status.ID+"]",$ul);
			if($find.length===0) //没有
			{
				createUpdateDish(status).appendTo($ul);
			}
			else
			{
				createUpdateDish(status,$find);
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
					else
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
					if(this.Status==="上餐")
					{
						find_create_or_update($doneul,this);
					}
					else if(this.Status==="制作"||this.Status==="准备")
					{
						find_create_or_update($notdoneul,this);
					}
				}
			);
		}
		
		setInterval(
			function()
			{
				$.get("/dineorder/GetDishStatus",
					{
						bar:currentBar,
						mins:$("#before").val()
					},
					function(datas)
					{
						if(isArray(datas))
						{
							var index=$("div.tabBar").tabs('options','selected').index();
							var $doneul=$("div.tabBar > div:eq("+index+") ul.done");
							var $notdoneul=$("div.tabBar > div:eq("+index+") ul.notdone");;

							//出餐台特殊,需要单独对待		
							if(currentBar==="出餐台")
							{
								displayForOutBar(datas,$doneul,$notdoneul);
							}
							else //其他台位
							{
								displayForOtherBars(datas,$doneul,$notdoneul);
							}
							
							var currentTab=$("div.tabBar").tabs('options','selected');
							$("div.tabDish",currentTab).accordion('refresh');
							
						}
						
					}					
				)
			},
			1000
		);
		
		$("div.tabBar").tabs(
			{
				selected:$("div.tabBar ul li:contains('"+currentBar+"')").index()
			}
		)
		.bind("tabsselect",function(e,ui)
	    {
	    	currentBar=$(ui.tab).html();
	    });
		
		$("div.tabDish").accordion({heightStyle:'fill'});
        
        
	}
);
