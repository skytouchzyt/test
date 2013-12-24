(function($)
{
	$.fn.dataGrid=
	function(options,arg)
	{
		var command;
		if(typeof options==="string")
		{
			command=options;
			options=null;
		}
		var $table=this;
		var $header=$("tr.header",$table);
		var $template=$("tr.template",$table);
		
		var settings=$table.data("options")||  //如果有的话,读取上一次的配置参数,否则就取默认的配置参数
		{
			paged:false,//是否分页
		    pageSize:10,//页大小
			pageIndex:0,//页号
			getDataAddress:null,//获取数据的地址,可以是网址,数组或者函数
			timeOut:0,//超时检测,默认为不检测
			getDataParams:null,//获取数据额外的参数
			onRowClick:null,//数据项被点击的时候调用
			onReceivedData:null,//接收到数据时调用
			onFormatData:null,//输出数据时调用,用作格式数据
			OnRenderRow:null,//显示行的时候调用
			alternation:false//是否交替显示
			//showAnimation:{effect:{mode:'show',direction:'right'},duration:500},
			//hideAnimation:{effect:{mode:'hide',direction:'left'},duration:500}
		};
		var settings=$.extend(settings,options||{});
			
		$table.data("options",settings);
		
		if(command)
		{
			switch(command)
			{
				case "updateRowUI":
					var $row=arg;
					updateRowUI($row);
					break;
				case "sort":
					var sortFunc=arg.sort;
					var pretreatFunc=arg.pre;
					var rows=$("tr.datarow",this).get();
					if(pretreatFunc)
					{
						$.each(rows,
							function()
							{
								pretreatFunc($(this).data("data"));
							}
						);
					}
					rows.sort(
						function(a,b)
						{
							return sortFunc($(a).data("data"),$(b).data("data"))
						}
					);
					$.each(rows,
						function()
						{
							$table.append($(this));	
							updateRowUI($(this));
						}
					);
					break;
					case "appendRow":
						rowData=arg;
						createRow(rowData);
					break;
			}
			return this;
		}
		
		
		
		if(!settings.getDataAddress)
			throw "请输入正确的数据地址!";
		var params=
		{
			paged:settings.paged,
			pageSize:settings.pageSize,
			pageIndex:settings.pageIndex,
		};
		if(typeof(settings.getDataParams)=="function")
		{
			params=settings.getDataParams(params);
		}
		
		
		//动画效果
		if(settings.hideAnimation)
			$table.effect('slide',settings.hideAnimation.effect,settings.hideAnimation.duration);
		
		//更新行UI
		function updateRowUI($row,rowData)
		{			
			if(!rowData)
			{
				rowData=$row.data("data");
			}		
			$headerCells=$("td",$header);
			$("td",$row).each(function(i,cell)
			{
				var dataField=$($headerCells[i]).attr("datafield");
				if(!dataField) return;//如果没有此属性就直接返回
				var data=rowData[dataField];
				if(settings.onFormatData)
					data=settings.onFormatData(rowData,dataField);
				$(cell).html(data);
			}
			);
			if(settings.onRenderRow)
			{
				settings.onRenderRow($row);
			}
		}
		
		function createRow(rowData)
		{
			var $newRow=$template.clone();
			$newRow
			.removeAttr("style")
			.removeClass("template")
			.addClass("datarow");
			$newRow.data("data",rowData);
			//$newRow.data("showDetails",true);
			
			$newRow.click(function(e)
			{
				if(typeof(settings.onRowClick)=="function")
				{
					settings.onRowClick($(this),e);								
				}
				return false;
			}
			);
			
			$newRow.insertBefore($template).show();
			
			updateRowUI($newRow,rowData);
		}
		
		function dealData(result)
		{
			settings.pageIndex=result.pageIndex;
			//删除旧的数据行
			$("tr.datarow",$table).remove();
			
			//alert(result.pageCount);
			
			$.each(result.datas,
				function(i,row)
				{
					createRow(row,$header,$template);								
				}
			);
			
			//生成页码
			//删除旧的页码
			$("ul.pager li:not(.template)",$table).remove();
			var $pagerTemplate=$("ul.pager li.template",$table);
			if(settings.pageIndex>0) //如果不是第一页就显示上一页按钮
			{
				$pagerTemplate.clone()
				.removeClass("template")
				.children('a').html('上一页').end()
				.bind('click',{pageIndex:settings.pageIndex-1},
				function(event)
				{
					$table.dataGrid({pageIndex:event.data['pageIndex']});
					return false;//返回false,防止页面滚动
				})
				.insertBefore($pagerTemplate);
				
			}
			
			for(var i=0;i<result.pageCount;i++)
			{
				var $newPager=$pagerTemplate.clone();
				if(i==settings.pageIndex)
					$newPager.addClass("currentPager");
				$newPager
				.removeClass("template")
				.children('a').html(i+1).end()
				.bind('click',{pageIndex:i},
				function(event)
				{
					$table.dataGrid({pageIndex:event.data['pageIndex']});
					return false; //返回false,防止页面滚动
				}
				).insertBefore($pagerTemplate);
				
			}
			if(settings.pageIndex<result.pageCount-1) //如果不是最后一页就显示下一页按钮
			{
				$pagerTemplate.clone()
				.removeClass("template")
				.children('a').html('下一页').end()
				.bind('click',{pageIndex:settings.pageIndex+1},
				function(event)
				{
					$table.dataGrid({pageIndex:event.data['pageIndex']});
					return false;//返回false,防止页面滚动
				})
				.insertBefore($pagerTemplate);
			}
			
			if(settings.alternation)
			{
				$("tr.datarow:odd",$table).addClass("datarowOdd");
				$("tr.datarow:even",$table).addClass("datarowEven");
			}
			
			//动画效果
			if(settings.showAnimation)
				$table.effect('slide',settings.showAnimation.effect,settings.showAnimation.duration);
			else
				$table.show();
		}
		
		//如果是一个字符串,就认为是获取数据的网址
		if(typeof(settings.getDataAddress)==="string")
		{
			$.getJSON(settings.getDataAddress,params,
					function(result)
					{
						if(settings.onReceivedData)
						{
							result=settings.onReceivedData(result);
						}
						if(isArray(result)) 
						{
							dealData({datas:result});
						}
						else
						{
							dealData(result);
						}
													
					}
					);
		}//如果是一个对象就认为是一个包含数据的数组
		else if(typeof(settings.getDataAddress)==="object")
		{
			dealData({datas:settings.getDataAddress});
		}//如果是一个函数就直接调用函数获取数据
		else if(typeof(settings.getDataAddress)==="function")
		{
			dealData({datas:settings.getDataAddress(params)});
		}
				  
					
					
		return this;	
	};
}
)(jQuery);