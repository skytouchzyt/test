

(function($)
{
	 //获取餐品的名字,去掉后面的数字9或者12或者(
    function getPizzaName(pizza)
    {
    	var name="";
    	for(var i=0;i<pizza.length;i++)
    	{
    		if(pizza[i]>='0'&&pizza[i]<='9')
    			break;
    		else if(pizza[i]=="(")
    			break;
    		else
    			name+=pizza[i];
    	}
    	return name;
    }
	var bScroll=true;	
	//如果textbox控件获得焦点就不能滚动菜单区
	$.fn.initScroll=function()
	{
		$("input[type='text']",this)
		.unbind('focus blur')
		.focus(function(){bScroll=false;})
		.blur(function(){bScroll=true;});
		
		return this;
	};
	
	//dish对象
	function dish(name,price,type,img,count)
	{
		this.dishName=name;
		this.price=price;
		this.typeName=type;
		this.count=count||1;		
		this.image=img;
		this.attach=[];
		
		this.calcPrice=function()
		{
			var total=0;
			total+=this.price*this.count;
			if(this.attach)
			{
				for(var i=0;i<this.attach.length;i++)
					total+=this.attach[i].price*this.count;
			}
			return total;
		};
		
	}
	
		$.fn.dishFromMenu=function(dishData)
		{
			if(!dishData)
			{				
				var r= new dish(
						$(this).attr("dishName"),
						$(this).attr("price"),
						$(this).attr("typeName"),
						$("img",$(this)).attr("src")
						);
				
				//如果此餐品有定制的编辑对话框
				var $specialDialog=$(stringFormat(".dialog[dishName='{0}']",r.dishName));
				if($specialDialog.size()>0)
				{
					r.$editDialog=$specialDialog;
					r.initDialog=eval($specialDialog.attr("init"));
					return r;
				}
				
				//如果此小类餐品有定制的编辑对话框
				//最近的一个li父节点中含有编辑对话框的信息
				//如果此餐品是隐藏的就需要到前面的可见li中找编辑对话框的信息
				var $li=this.closest('li');//找父li节点
				if($li.is(':hidden')) 
				{
					$specialDialog=$.dish(getPizzaName(r.dishName)).$editDialog;
				}
				else //如果不是隐藏的就直接找父li节点
					$specialDialog=$("#"+$li.attr("editDialog"));
				if($specialDialog.size()>0)
				{
					
					r.$editDialog=$specialDialog;
					r.initDialog=eval($specialDialog.attr("init"));	
					return r;
				}
				
				//如果此大类的餐品有定制的编辑对话框
				$specialDialog=$(stringFormat(".dialog[typeName='{0}']",r.typeName));
				if($specialDialog.size()>0)
				{
					r.$editDialog=$specialDialog;
					r.initDialog=eval($specialDialog.attr("init"));
					return r;
				}
				
				return r;
				
			}
			else
			{
				throw "$.fn.dishFromMenu 代码还没有完成.";
			}
		};
		
		//更新餐品的单价,数量,价格和图片
		$.fn.updateDishUI=function(dishData)
		{
			if(!this.is("tr.remark,tr.attach,tr.dish"))
				throw "$.fn.updateDishUI 传入的数据无效.";
			
			$(".dishName span",this).text(dishData.dishName);
			$(".price",this).text(dishData.price);
			$(".dishName img",this).attr("src",dishData.image);
			if(!this.is("tr.attach")) //附属餐品不需要显示数量
				$(".count",this).text(dishData.count);
			return this;
		}
		
		//根据dish数据更新orderItem UI
		$.fn.updateOrderDetailItemUI=function()
		{
			var dishData=this.data("dish");
			
			if(this.is(".item.detail"))
			{
				//更新餐品UI
				$(".dish",this).updateDishUI(dishData);
				
				//更新附属UI
				$("tr.attach",this).remove();
				if(dishData.attach)
				{
					var $temp=$(".item.detail.template tr.attach");
					for(var i=0;i<dishData.attach.length;i++)
					{
						$temp.clone()
						.updateDishUI(dishData.attach[i])
						.insertBefore($("tr.remark",this))
						.show()
						;
					}
				}
				
				//更新备注UI
				$("tr.remark .dishName span",this).text(dishData.remark);
				
				$("tr.remark",this).visible(dishData.remark);
							
			}
			else
			{
				throw "$.fn.dish 传入的数据无效,无法写入餐品数据.";
			}
			//更新总计
			this.parent('fieldset.orderDetails').updateOrderUI();
			
			return this;
		}
		
		//更新总计
		$.fn.updateOrderUI=function()
		{
			if(!this.is('fieldset.orderDetails'))
				throw "$.fn.updateOrderUI 传入的对象无效.";
			var total=0;
			$("table.item.detail:not('.template')",this)
			.each(
					function()
					{
						total+=$(this).data("dish").calcPrice();
					}
				);
			$("table.item.total span.total",this).text(total);
			
			return this;			
		}
		
	 	//从订单列表或者餐品列表中获取餐品信息,或者通过餐品的名称获取餐品信息
		$.fn.dish=function(dishData) 
		{
			if(typeof(dishData)=="string") //通过餐品的名称获取餐品信息
			{
				return $(stringFormat(".boxgrid .add2Order[dishName='{0}']",dishData)).dish();
			}		
			var $item=$(this);
			
			if($item.length==0)
				throw "$.fn.Dish 传入的数据为空,无法读取餐品信息.";
				
			if($item.length>1) //如果传入多项,就返回数组
			{
				return $item.map
					(
						function()
							{
								return $(this).dish();
							}
					);
					
			}
								
			//订单列表项中的餐品
			//直接从jQuery data中读取数据
			if($item.is(".item.detail"))
			{
				var d=this.data("dish");
			    return d;
			}
			else if($item.is(".add2Order")) //餐品列表中的餐品
			{
				return $item.dishFromMenu();	
			}
			else
				throw "$.fn.dish 传入的数据无效,无法读取餐品信息.";
		}
	
		
		
		
		$.dish=$.fn.dish;
		
		//给每个订单中的餐品的操作按钮添加事件处理函数
		//包括 增加,减少,编辑,删除
		$.fn.initOperation=function()
		{
			var $orderItem=this;
			var dishData=this.data('dish');
			
			$(".itemEdit", this).click(function () {
				$orderItem.editDialog();	                        
	            return false;
	        }
	        );

	        $(".itemAdd", this).click(function () {
	            dishData.count++;
				if(dishData.count==0)
					dishData.count++;				
				$orderItem.updateOrderDetailItemUI();			
	            
	            return false;
	        }
	        );

	        $(".itemSub", this).click(function () {
	            dishData.count--;
				if(dishData.count==0)
					dishData.count--;
				$orderItem.updateOrderDetailItemUI();
	            return false;
	        }
	        );


	        $(".itemDel", this).click(function () {
				$orderDetails=$orderItem.parents("fieldset.orderDetails");
				$orderItem.effect('blind',{},500,function()
					{
						$(this).remove();
						$orderDetails.updateOrderUI();
					}
				);
				
				
	            return false;
	        }
	        );
	        
	        return this;
		};
		
		
		
		
		//运行编辑对话框
		$.fn.editDialog=function()
		{
			var dishData=this.dish();
			var bDialogOK=false;
			var $dialog;
			var $orderItem=this;
			
			function runDialog(callback)
			{
				$dialog
				.attr('title',dishData.dishName)
				.unbind()
				.initScroll()
				.dialog({close:
						function()
						{
							if(bDialogOK) //如果点击了确定按钮就保存餐品数据并更新UI
							{
								
								if(dishData.onOK)
									dishData=dishData.onOK($dialog,dishData);
								dishData.remark=$("input.remark",$dialog).val();
								if(callback) //如果是加入新的餐品
								{
									callback();
								}
								else //修改已有的餐品
								{
									//更新UI
									$orderItem
									.data("dish",dishData) //注意这里把数据回写
									.updateOrderDetailItemUI();
								}
							}
							$dialog.dialog('destroy');
						},
						open:
						function()
						{
							//关闭对话框后就删除此dialog
							$('input.btnOK',$dialog)
							.unbind()
							.click(
								function()
								{
									bDialogOK=true;
									$dialog.dialog('close');
								}
							);
							$('input.btnCancel',$dialog)
							.unbind()
							.click(
									function()
									{
										bDialogOK=false;
										$dialog.dialog('close');
									}
							);
							bDialogOK=false;
							$('input.remark',$dialog).val(dishData.remark);
							if(dishData.initDialog)
								dishData.initDialog($dialog,dishData);
						},
						modal:true
					}
					);
			}
			
			if(dishData.$editDialog) //如果有特殊的编辑对话框
			{
				$dialog=dishData.$editDialog;
			}
			else //否则就启用备注编辑对话框
			{
				$dialog=$(".remarkDialog.dialog");
			}
			
			if(this.is('.orderDetails .item.detail')) //编辑已加入订单的餐品
			{
				runDialog();
			}	
			else if(this.is("#Dishes ul.dish li  .boxgrid .add2Order")) //加入新的餐品
			{
				var $orig=this;
				function addNewDish2Order()
				{
					var $newItem=$('.item.detail.template').clone();
					$newItem.data("dish",dishData)
					.removeClass('template')
					.insertBefore('.item.detail.template')
					.updateOrderDetailItemUI()
					.initOperation()
					.effect('slide',{},500,
							function()
							{
								$(this).show();
							}
					);
				
				
					//首先获取倒数第二个orderItem项
					var targetPos;	
					var origPos=$orig.offset();
					var $items=$(".orderDetails .item.detail:not('.template')");
					var nums=$items.size();
					if(nums<2) //如果还没有选择任何餐品就取得orderDetails的位置
					{
						targetPos=$(".orderDetails").offset();
					}
					else
					{
						targetPos=$($items.get(nums-2)).offset();
						targetPos.top+=42;
					}
				
					$orig.clone()
					.appendTo('body')
					.css(
							{
								position:'absolute',
								top:origPos.top,
								left:origPos.left,
								'z-index':100000,
								border:'1px solid #ec000c'
							}
						)
					.animate
						(
							{
								top:targetPos.top,
								left:targetPos.left,
							},
							1500,
							function()
							{
								$(this).remove();
							}
						);
				}
				
				if(!$dialog.is(".remarkDialog.dialog")) //如果有特殊的编辑对话框
				{
					runDialog(addNewDish2Order);
					return this;
				}
				
				addNewDish2Order();
				return this;
				
			}
			
			return this;
			
		};
		
		$(document).ready(
			function()
			{
				$('.boxgrid').hover(
						function () 
						{
							$(".cover", this).stop().animate({ top: '62px' }, { queue: false, duration: 300 });
						}, 
						function () 
						{
							$(".cover", this).stop().animate({ top: '100px' }, { queue: false, duration: 300 });
						}
					);



                $('#PhoneOrder').hover(
						function () {
						$(this).stop().animate({ left: '-16px' }, { queue: false, duration: 500 });
					}, 
					function () {
									$(this).stop().animate({ left: '-550px' }, { queue: false, duration: 500 });
				});

				$.localScroll({
					//easing: 'easeInOutBack',
					speed: 1500
				});
			
			
				//设置滚动
				$(this).initScroll();

				var $lastSelectedDishes;
				$(document).keypress(function (e) 
				{
					if (bScroll) 
					{
						var start = String.fromCharCode(e.keyCode);
						
						
						$("#nav_" + start).click();
						
						if($lastSelectedDishes)
							$lastSelectedDishes.removeAttr("style");
						
						$lastSelectedDishes=
							$("#"+start)
							.nextUntil("li:empty")
							.children(".boxgrid");
						
						$lastSelectedDishes.css(
							{
								border:'2px dashed green'
							}
						);
						
						return false;
					}
				}
				);


				$(".add2Order").click(
					function () 
					{      
						$(this).editDialog();
						return false;
					}
					);
			}
		);
		
		function initDialogForPizza($dialog,dishData)
		{
			var bJB=false;//是否加卷边
			var bNL=false;//是否加双份奶酪

			$.each(dishData.attach,
				function()
				{
					if(this.dishName.indexOf('卷边')>=0)
						bJB=true;
					if(this.dishName.indexOf('双份奶酪')>=0)
						bNL=true;
				}
			);
			

			dishData.onOK=function($d,data)
			{
				var dishName=getPizzaName(data.dishName)+$('input:radio:checked',$d).val();
				var newDish=$.dish(dishName);
				newDish.count=data.count;
	
				var pizzaSize=parseInt($(":radio:checked",$d).val());
				$(":checkbox:checked:enabled",$d)
				.each(function()
					 {
						newDish.attach.push($.dish(pizzaSize+"寸"+$(this).val()));
					 }
					);				
				return newDish;
			};
			
			
			
			$("input:radio",$dialog).unbind().click(
				function()
				{
					var checked=$("input:radio[name='pizza'].italy",$dialog).check();
					var labelJB=$("input.jb",$dialog)
					.enable(!checked).next();
					//使芝心卷边的复选框后的label颜色变化
					//如果是禁用就变成灰色
					//否则就是正常颜色
					if(checked)//如果选中了薄饼
						labelJB.addClass("disabled");
					else
						labelJB.removeClass("disabled");
					
					
					$dialog.dialog(
					{
					//改变标题
						title:getPizzaName(dishData.dishName)+$("input:radio:checked",$dialog).val()
					});
				}
			);    	
    		$("input.jb",$dialog).enable(
    			dishData.dishName.indexOf("薄")<0 //如果是薄饼,就不能加卷边
    			&&dishData.dishName.indexOf("香烤牛肉")<0//香烤牛肉,荷塘月色,BBQ鸡肉不能加卷边
    			&&dishData.dishName.indexOf("荷塘月色")<0
    			&&dishData.dishName.indexOf("BBQ鸡肉")<0
    			)
			.check(bJB);
    	
    		$("input.nl",$dialog).check(bNL);
    	
    		$("input[name='pizza']",$dialog).checkGroup(
    			[
    			 	dishData.dishName.indexOf("9")>=0,
    			 	dishData.dishName.indexOf("12")>=0&&dishData.dishName.indexOf("薄")<0,
    			 	dishData.dishName.indexOf("12薄")>=0			 	
    			]
    		);
    		   		 		
    		
		}
		function initDialogForSandwich($dialog,dishData)
		{
			var bJD=false;//是否加鸡蛋沙拉
			var bROU=false;//是否加双份肉
			$.each(dishData.attach,function()
				{
					if(this.dishName.indexOf('鸡蛋沙拉')>=0)
						bJD=true;
					else if(this.dishName.indexOf('双份肉')>=0)
						bROU=true;
				}
			);
			
			//去掉餐品名称后面的数字
			//例如:如果是 '蔬菜奇士6' 就返回 '蔬菜奇士'
			var dishName=getPizzaName(dishData.dishName);
			
			var $labelROU=$('input:checkbox.rou',$dialog)
			.enable(dishName!='蔬菜奇士') //蔬菜奇士不能加双份肉
			.check(bROU)
			.next();
			
			if(dishName=='蔬菜奇士')
			{
				$labelROU.addClass("disabled");
			}
			else
			{
				$labelROU.removeClass("disabled");
			}
			
			
			$('input:checkbox.jd',$dialog).check(bJD);
			
			dishData.onOK=function($d,data)
			{
				var dishName=getPizzaName(data.dishName)+$('input:radio:checked',$d).val();
				var newDish=$.dish(dishName);
				newDish.count=data.count;
	
				var sandwichSize=parseInt($(":radio:checked",$d).val());
				$(":checkbox:checked:enabled",$d)
				.each(function()
					 {
						newDish.attach.push($.dish($(this).val()+sandwichSize));
					 }
					);				
				return newDish;
			}
			
			
		}
		function initDialogForSpecialDrink($dialog,dishData)
		{
			$(":radio",$dialog).checkGroup
			(
				[
					dishData.dishName.indexOf('(冰)')>=0,
					dishData.dishName.indexOf('(常温)')>=0,
					dishData.dishName.indexOf('(热)')>=0
				]
			);
			dishData.onOK=function($d,data)
			{
				var dishName=getPizzaName(data.dishName)+$('input:radio:checked',$d).val();
				var newDish=$.dish(dishName);
				newDish.count=data.count;			
				return newDish;
			};
		}
}	
)(jQuery);