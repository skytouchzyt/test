//处理订单
//选择餐品加入到orderDetails里
// LoadDishes 从服务器获取餐品数据,然后显示出来
// 

$(document).ready(
function()
{

	var soldoutDishes=[];
	var Dishes = {};

	var  suitPizza=
	[
		'新德里辣牛比萨',
        '墨西哥小牛排比萨',
		'果木姜汁鸭胸',
		'BBQ鸡肉',
		'泡菜肥牛'

	];
	
	var suitLunch=
	[
		"东坡肉饭",
        "川香牛肉酱意面",
		"南洋咖喱鸡腿饭",
        "奶香芝士培根意面",
        "铁板煎鸡饭",
        "什锦蘑菇饭",
        "什锦蘑菇面",
        "左宗棠鸡饭"
	];
	
	var suitDrinks=
	[
		"听可乐",
        "听雪碧",
        "听芬达",
        "听健怡",
		"冬阴功海鲜汤",
        "奶油南瓜汤"
	];

	var vipDishes =
    [
        { DishName: '照烧鸡排', Price: 18 },
        { DishName: '奶油南瓜汤', Price: 7 },
        { DishName: '泡菜肥牛9', Price: 29 },
        { DishName: '铁板煎鸡饭', Price: 15 },
        { DishName: '青翠三文鱼', Price: 15 }
    ];
	
	//通过名称获取Dish数据
	function getDishByName(name)
	{
	    if (Dishes[name])
	        return Dishes[name];
		
		throw "没有找到指定的餐品:"+name;
		
	}
	
   
    

	//处理套餐选餐
	//event.data包含存放下标和选项数组,数组长度为可以选几项
	//event.data.$dishes 所有可被选择的餐品
	//如果为空就默认为当前li兄弟li集合
	function onSelected(event)
	{
		if(!event.data.$dishes)
		{
			$(this)
			.parent("ul")
			.children("li")
			.removeClass("suitSelected");
		}
		else
		{
			event.data.$dishes.each
			(
				function()
				{
					$(this).removeClass("suitSelected");
				}
			);
		}
		
		if($(this).is(".suitSelected"))
		{
			event
			.data
			.selections[(--event.data.index)%event.data.selections.length]
			=null;
		}
		else
		{
			event
			.data
			.selections[(event.data.index++)%event.data.selections.length]
			=$(this);
		}
		
		$.each(event.data.selections,
			function(i,$item)
			{
				if($item)
				{
					$item.addClass("suitSelected");
				}
			}
		);
		
		return false;
		
	}
	
    

	//设置套餐中餐品的选中状态
	function setSelected($detail,$dialog)
	{
		try
		{
			$.each($detail.data("data").Attaches,
			function(i,a)
			{
			    $("li:contains('" + a.DishName + "')", $dialog)
                .addClass("suitSelected");
			}
			);
		}
		catch(e)
		{
			throw "onSelected 出错:"+e;
		}
	}
	
	//检测数组是否没有空元素
	function isArrayFull(array,targetDish)
	{
		if(!isArray(array))
		{
			throw "请输入一个数组!";
		}
		var notNullCount=0;
		for(var i=0;i<array.length;i++)
		{
			if(array[i])
			{
				notNullCount++;
				var a=createDetail(array[i].data("data"));
				a.Price=0;
				targetDish.Attaches.push(a);
			}
				
		}
		return notNullCount==array.length;
	}
	
	//更新或者加入套餐3人
	function addUpdateSuit3($target)
	{
		var $dialog=$("#suit3Dialog");
		var $part1=$("ul.part1",$dialog).empty();
		var $part2=$("ul.part2",$dialog).empty();
		
		var name = $("span.dishName", $target).html();
		var dish = getDishByName(name);
		var detail = createDetail(name);

		var week=new Date().getDay();
		if(week<1||week>5)
		{
			alert("周一到周五才有此套餐.");
			return;
		}
		
		week--;//换算成数组下标
		
		//添加四款可选比萨
		$.each(suitPizza,
			function(i,p)
			{
				createDish(getDishByName(p+"12"))
				.addClass("pizza")
				.unbind()
				.appendTo($part1);
			}
		);
		
		
		//添加欢聚大拼盘
		createDish(getDishByName("欢聚大拼盘"))
		.unbind()
		.appendTo($part1);
		

		
		//添加饮品
		createDish(getDishByName("果粒橙"))
        .unbind()
        .appendTo($part2);
		
		//添加沙拉	
		createDish(getDishByName("香蕉沙拉"))
        .unbind()
        .appendTo($part1);

		
		
		//处理比萨的选择
		var argsPizza=
		{
			selections:[null],
			index:0,
			$dishes:$dialog.find("li.pizza")
		};
		argsPizza.$dishes.bind('click',argsPizza,onSelected);
		
		//处理饮品的选择
		var argsDrink=
		{
			 selections:[null,null,null],
			 index:0,
			 $dishes:$dialog.find("li.drink")
		};
		argsDrink.$dishes.bind('click',argsDrink,onSelected);
		
		
		//处理沙拉的选择
		var argsSalad=
		{
			 selections:[null],
			 index:0,
			 $dishes:$dialog.find("li.salad")
		};
		argsSalad.$dishes.bind('click',argsSalad,onSelected);
		
		//如果是修改订单明细
		if($target.parents("ul").is("#orderDetails"))
		{
		    detail = $target.data("data");
			setSelected($target,$dialog);	
			
			argsPizza.selections=[];
			argsDrink.selections=[];
			argsSalad.selections=[];
			
			$.each(dish.Attaches,
			function(i,a)
			{
				var selections=null;
				if(a.ClassName==="外卖饮品")
				{
					selections=argsDrink.selections
					
				}
				else if(a.ClassName==="比萨")
				{
					selections=argsPizza.selections;
				}
				else if(a.ClassName==="沙拉")
				{
					selections=argsSalad.selections;
				}
				if(selections)				
					selections.push($("li:contains("+a.Name+")",$dialog));
			}
			);
		}
		
		$("input.remark",$dialog).val(detail.Remark);
		
		
		$("input.btnCancel",$dialog)
		.unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		$("input.btnOK",$dialog)
		.unbind()
		.click(function()
		{
			newDetail=createDetail(name);
			
			
			
			if(!isArrayFull(argsPizza.selections,newDetail))
			{
				alert("请选择一款比萨!");
				return false;
			}
			

			
			var hj=createDetail(getDishByName("欢聚大拼盘"));
			hj.Price=0;
			newDetail.Attaches.push(hj);

			var d = createDetail(getDishByName("香蕉沙拉"));
			d.Price = 0;
			newDetail.Attaches.push(d);

	
			d = createDetail(getDishByName("果粒橙"));
			d.Price = 0;
			newDetail.Attaches.push(d);
			
			
			newDetail.Remark=$("input.remark",$dialog).val();
			
			//如果是新加入的套餐
			if($target.parent("ul").is(".dishList"))
			{
				addDish2Details(newDetail);
			}
			else
			{
				newDetail.Amount=detail.Amount;
				$target.data("data",newDetail);
				updateDetailUI($target);
			}
			$dialog.dialog('close');
		}
		);
		
		$dialog.dialog({modal:true,title:"三人套餐",width:1150,heigth:741});
		
	}
	
	//更新或者加入套餐2人
	function addUpdateSuit2($target)
	{
		var $dialog=$("#suit2Dialog");
		var $part1=$("ul.part1",$dialog).empty();
		var $part2=$("ul.part2",$dialog).empty();

		
		var dish=$target.data("data");
		
		
		var week=new Date().getDay();
		if(week<1||week>5)
		{
			alert("周一到周五才有此套餐.");
			return;
		}
		
		
		//添加四款可选比萨
		$.each(suitPizza,
			function(i,p)
			{
				createDish(getDishByName(p+"9"))
				.addClass("pizza")
				.unbind()
				.appendTo($part1);
			}
		);
		

		
		
		//添加饮品
		createDish(getDishByName("加多宝"))
        .unbind()
        .appendTo($part2);

	    //添加小吃
		createDish(getDishByName("鸡米花"))
        .unbind()
        .appendTo($part2);

		
		//处理比萨的选择
		var argsPizza=
		{
			selections:[null],
			index:0,
			$dishes:$dialog.find("li.pizza")
		};
		argsPizza.$dishes.bind('click',argsPizza,onSelected);
		

		
		

		
		//如果是修改订单明细
		if($target.parents("ul").is("#orderDetails"))
		{
			setSelected($target,$dialog.find("li"));
			
			
			argsPizza.selections=[];
			argsDrink.selections=[];
			argsSnack.selections=[];
			
			$.each(dish.Attaches,
			function(i,a)
			{
				var selections;
				if(a.ClassName==="外卖饮品")
				{
					selections=argsDrink.selections
					
				}
				else if(a.ClassName==="比萨")
				{
					selections=argsPizza.selections;
				}
				else if(a.ClassName==="小吃")
				{
					selections=argsSnack.selections;
				}				
				selections.push($("li:contains("+a.Name+")",$dialog));
			}
			);
		}
		
		$("input.remark",$dialog).val(dish.Remark);
		
		$("input.btnCancel",$dialog)
		.unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		$("input.btnOK",$dialog)
		.unbind()
		.click(function()
		{
			var newdish=createDetail(dish);
			
			if(!isArrayFull(argsPizza.selections,newdish))
			{
				alert("请选择一份比萨!");
				return false;
			}
			
			var d = createDetail(getDishByName("加多宝"));
			d.Price = 0;
			newdish.Attaches.push(d);
			newdish.Attaches.push(d);

			d = createDetail(getDishByName("鸡米花"));
			d.Price = 0;
			newdish.Attaches.push(d);
			
			newdish.Remark=$("input.remark",$dialog).val();
			
			//如果是新加入的套餐
			if($target.parent("ul").is(".dishList"))
			{
				addDish2Details(newdish);
			}
			else
			{
				newdish.Amount=dish.Amount;
				$target.data("dish",newdish);
				updateDetailUI($target);
			}
			$dialog.dialog('close');
		}
		);
		
		$dialog.dialog({modal:true,title:dish.Name,width:761,height:769});
		
	}
	
	

	
	//更新或者加入特惠午餐
	function addUpdateSuitLunch($target)
	{
		var $dialog=$("#suitLunchDialog");
		var $part1=$("ul.part1",$dialog).empty();
		var $part2=$("ul.part2",$dialog).empty();

		
		var dish=$target.data("data");
		
		
		var week=new Date().getDay();
		if(week<1||week>5)
		{
			alert("周一到周五才有此套餐.");
			return;
		}

		
		
		
		//添加主食
		for(var i=0;i<suitLunch.length;i++)
		{
			createDish(getDishByName(suitLunch[i]))
			.unbind()
			.addClass("main")
			.appendTo($part1);
		}
		
		//添加饮品
		for(var i=0;i<suitDrinks.length;i++)
		{
			createDish(getDishByName(suitDrinks[i]))
			.unbind()
			.addClass("drink")
			.appendTo($part1);
		}
		
		
		var argsMain=
		{
			selections:[null],
			index:0,
			$dishes:$("li.main",$dialog)
		}
		
		argsMain.$dishes
		.bind('click',argsMain,onSelected);
		
		var argsDrink=
		{
			selections:[null],
			index:0,
			$dishes:$("li.drink",$dialog)
		}
		argsDrink.$dishes
		.bind('click',argsDrink,onSelected);
		
		
		//如果是修改订单明细
		if($target.parents("ul").is("#orderDetails"))
		{
			setSelected($target,$dialog.find("li"));
			
			argsMain.selections=[];
			argsDrink.selections=[];
			
			$.each(dish.Attaches,
			function(i,a)
			{
				var selections;
				if(a.ClassName==="外卖饮品"||a.ClassName==="汤")
				{
					selections=argsDrink.selections
					
				}
				else if(a.ClassName==="主食")
				{
					selections=argsMain.selections;
				}
								
				selections.push($("li:contains("+a.Name+")",$dialog));
			}
			);
			
		}
		
		$("input.remark",$dialog).val(dish.Remark);
		
		$("input.btnCancel",$dialog)
		.unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		$("input.btnOK",$dialog)
		.unbind()
		.click(function()
		{
			var newdish=createDetail(dish);
		
			
			newdish.Remark=$("input.remark",$dialog).val();
			
			if(!isArrayFull(argsMain.selections,newdish))
			{
				alert("请选择一份主食!");
				return;
			}
			
			if(!isArrayFull(argsDrink.selections,newdish))
			{
				alert("请选择一份饮品!");
				return;
			}

			//如果是新加入的套餐
			if($target.parent("ul").is(".dishList"))
			{
				addDish2Details(newdish);
			}
			else
			{
				newdish.Amount=dish.Amount;
				$target.data("dish",newdish);
				updateDetailUI($target);
			}
			$dialog.dialog('close');
			
		}
		);
		
		$dialog.dialog({modal:true,title:dish.Name,width:957,height:700});
		
	}
	
	//更新或者加入三明治
	function addUpdateSandwitch($target)
	{
		var sand=$target.data("data");
		var $dialog=$("#sandwitchDialog");
		
		$("input:checkbox").check(false);
		
		//如果是加入新的比萨
		if($target.parent("ul").is(".dishList"))
		{
			$("input.small",$dialog).check(true);
			$("input.remark",$dialog).val("");
		}
		else
		{
			//检测是否有双份肉或者鸡蛋沙拉
			for(var i=0;i<sand.Attaches.length;i++)
			{
				if(sand.Attaches[i].Name.indexOf("鸡蛋沙拉")>=0)
				{
					$("input:checkbox.jd",$dialog).check(true);
				}
				else if(sand.Attaches[i].Name.indexOf("双份肉")>=0)
				{
					$("input:checkbox.rou",$dialog).check(true);
				}
			}
			$("input.remark",$dialog).val(sand.Remark);
		}
		
		$("input.btnOK",$dialog).unbind()
		.click(function()
		{
			$dialog.dialog('close');
			var name=getRealName(sand.Name)+$("input:radio:checked",$dialog).val();
			var newsand=createDetail(getDishByName(name));
			
			newsand.Remark=$("input.remark",$dialog).val();
			
			
			$("input:checkbox:checked",$dialog)
			.each(function()
			{
				//首先获取比萨的大小
				var size=$("input:radio:checked",$dialog).val();
				var attach=createDetail(getDishByName($(this).val()+size));
				newsand.Attaches.push(attach);
			}
			);
			
			if($target.parent("ul").is(".dishList"))
			{
				addDish2Details(newsand);
			}
			else
			{
				newsand.Amount=sand.Amount;
				$target.data('dish',newsand);
				updateDetailUI($target);
			}
		}
		);
		
		$("input.btnCancel",$dialog).unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		
		$dialog.dialog({modal:true,title:sand.Name});
		
	}
	
	
	//更新或者加入比萨
	function addUpdatePizza($target)
	{
		var pizza=$target.data("data");
		var $dialog=$("#pizzaDialog");
		
		$("input:checkbox").check(false);
		
		//如果是加入新的比萨
		if($target.parent("ul").is(".dishList"))
		{
			$("input.small",$dialog).check(true);
			$("input.remark",$dialog).val("");
		}
		else 
		{
			//检测是否有卷边或者双份奶酪
			for(var i=0;i<pizza.Attaches.length;i++)
			{
				if(pizza.Attaches[i].DishName.indexOf("金牌卷边")>=0)
				{
					$("input:checkbox.jb",$dialog).check(true);
				}
				else if(pizza.Attaches[i].DishName.indexOf("双份奶酪")>=0)
				{
					$("input:checkbox.nl",$dialog).check(true);
				}
			}
			
			$("input.remark",$dialog).val(pizza.Remark);
		}
		
		
		
		$("input.btnOK",$dialog).unbind()
		.click(function()
		{
			$dialog.dialog('close');
			var name=getRealName(pizza.DishName||pizza.Name)+$("input:radio:checked",$dialog).val();
			var newpizza=createDetail(getDishByName(name));
			
			newpizza.Remark=$("input.remark",$dialog).val();
			
			
			$("input:checkbox:checked",$dialog)
			.each(function()
			{
				//首先获取比萨的大小
				var size;
				if($("input:radio:checked",$dialog).val()=="9")
					size="9";
				else
					size="12";
				var attach=createDetail(getDishByName($(this).val()+size));
				newpizza.Attaches.push(attach);
			}
			);
			
			if($target.parent("ul").is(".dishList"))
			{
				addDish2Details(newpizza);
			}
			else
			{
				newpizza.Amount=pizza.Amount;
				$target.data('dish',newpizza);
				updateDetailUI($target);
			}
		}
		);
		
		$("input.btnCancel",$dialog).unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		
		$dialog.dialog({modal:true,title:pizza.Name});
	}
	
	
	//更新或者加入饮品
	function addUpdateDrink($target)
	{
		var drink=$target.data("data");
		var $dialog=$("#drinkDialog");
		
		$("input:radio",$dialog).check(true);
		
		//如果是新加入的饮品
		if($target.parent("ul").is(".dishList"))
		{
			$("input.remark",$dialog).val("");
		}
		else //否则就是修改已点过的餐品
		{
			$("input:radio[name='specialDrink']",$dialog)
			.each(function()
			{
				if(drink.Name.indexOf($(this).val())>=0)
				{
					$(this).check(true);
				}
			}
			);
		}
		
		$("input.btnCancel",$dialog)
		.unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		$("input.btnOK",$dialog)
		.unbind()
		.click(function()
		{
			
			var newdrink=createDetail(getDishByName(getRealName(drink.Name)));
			newdrink.Name+=$("input:radio:checked",$dialog).val();
			newdrink.Remark=$("input.remark",$dialog).val();
			
			if($target.parent("ul").is(".dishList"))
			{
				addDish2Details(newdrink);
			}
			else
			{
				newdrink.Amount=drink.Amount;
				$target.data("dish",newdrink);
				updateDetailUI($target);
			}
			
			$dialog.dialog('close');
		}
		);
		
		
		
		$dialog.dialog({modal:true,title:drink.Name});
		
		
	}
	
	//加入餐品到订单表中
	function addDish2Details(newdetail)
	{
		//找到订单明细里面是否已经有此餐品
		//如果有,就直接修改数量
		//否则就加入新的餐品
		//另外还有对比是否有备注和附属
		//如果有也算不同的餐品
		var same=false;
		$("li","#orderDetails").each(function()
		{
			var detail=$(this).data("data");
			if(detail.DishName==newdetail.DishName
			&&detail.Attaches.length==0
			&&!detail.Remark
			)
			{
				same=true;
				//餐品从一份变为两份的时候给点餐员一个提示
				//防止误操作                                                         
				if(detail.Amount==1)
				{
					if(!confirm("确定要增加一份"+detail.DishName+"吗?"))
						return false;
				}
				detail.Amount++;
				updateDetailUI($(this));
				
				return false;
			}
		}
		);
		if(!same) //如果找到相同的餐品就返回
		{
			
			var $detail=createDish(newdetail);
			$detail
			.unbind()
			.find("div.remark").show()
			.end().find("div.operation").show()
			.end()
			.find("div.operation a")
			.click(function()
			{
				dish=$(this).parents("li").data('data');
				switch($(this).attr("class"))
				{
					case "add":
						dish.Amount++;
						if(dish.Amount==0)
							dish.Amount++;
						break;
					case "sub":
						dish.Amount--;
						if(dish.Amount==0)
							dish.Amount--;
						break;
					case "del":
						$detail
						.effect('slide',{mode:'hide',direction:'left'},500,
							function()
							{
								$(this).remove();
								updateDetailUI($detail);
							}
						);
						return false;
						break;
					case "remark":
						runRemarkDialog($detail);
						break;
				}
				updateDetailUI($detail);
				return false;
			}
			).end()
			.appendTo("#orderDetails")
			;
		}
		
		
		updateDetailUI($detail);
		
		//$("ul.dishList li:contains(返回)").click();
		
	}
	
	function runRemarkDialog($detail)
	{
	    var detail = $detail.data("data");
	    var className = getDishByName(detail.DishName).ClassName;
		if(className=="比萨")
		{
			addUpdatePizza($detail);
			return;
		}
		if(className=="三明治")
		{
			addUpdateSandwitch($detail);
			return;
		}
		if(className=="套餐"&&dish.Name=="套餐(3人)")
		{
			addUpdateSuit3($detail);
			return;
		}
		if(className=="套餐"&&dish.Name=="套餐(2人)")
		{
			addUpdateSuit2($detail);
			return;
		}
		if(className=="套餐"&&dish.Name=="特惠午餐")
		{
			addUpdateSuitLunch($detail);
			return;
		}

		
		if(className=="饮品"||dish.ClassName=="咖啡")
		{
			addUpdateDrink($detail);
			return;
		}
		
		var $dialog=$("#remarkDialog");

		$("input.remark",$dialog).val(detail.Remark);
		
		$("input.btnOK").unbind()
		.click(function()
		{
			$dialog.dialog('close');
			dish.Remark=$("input.remark",$dialog).val();
			updateDetailUI($detail);
			return false;
		}
		);
		
		$("input.btnCancel").unbind()
		.click(function()
		{
			$dialog.dialog('close');
		}
		);
		
		$dialog.dialog({modal:true,title:"给餐品添加备注:"+detail.DishName});
		
	}
	
	function getDishRemark(dish)
	{
		var remark=dish.Remark||"";
		for(var i=0;i<dish.Attaches.length;i++)
		{
			remark+=","+dish.Attaches[i].DishName;
		}
		if(remark)
		{
			if(remark[0]==',')
				remark=remark.substring(1,remark.length);
			remark=remark.trim().replace(' ',',');
		}
		return remark;
	}
	
	//更新UI
	function updateDetailUI($detail)
	{
		if($detail)
		{
			var dish=$detail.data("data");
			if(dish)
			{
				$("span.dishName",$detail).html(dish.DishName);
				$("span.price",$detail).html(dish.Price+"X"+dish.Amount);
				//更新备注
				var remark=getDishRemark(dish);
				$("p.remark",$detail).html(remark);
				
				
			}
		}
		var price = 0;
		var discount = 10;
		if ($("div#PhoneOrder input#vip").val()) //如果有会员卡就9.5折
		    discount = 9.5;
		$("ul#orderDetails li")
		.each(function()
		{		
		    var d = $(this).data("data");
		    var dish = getDishByName(d.DishName);
		    var temp = dish.CanDiscounted ? discount : 10.0; //如果餐品不能打折就返回10.0
			price += (calcDishPrice(d) * d.Amount * temp / 10).round();
		}
		);
		
		var t = $("div#PhoneOrder input:radio[name='orderType']:checked").val();
		if (t) {
		    var temp = getDishByName(t);

		    price += temp.Price;
		}
		
		$("div#PhoneOrder span.total")
		.html(price);
	}
	
	//计算餐品的价格
	function calcDishPrice(dish)
	{
		if(!dish.Attaches)
			return dish.Price;
		var price=dish.Price;
		$.each(dish.Attaches,
		function(i,a)
		{
			price+=calcDishPrice(a);
		}
		);
		return price;
	}
	
	//处理餐品点击
	function onDishClick()
	{
	    var name = $("span.dishName", this).html();
	    var dish = $(this).data("data");
	    var detail=createDetail(dish);
		
		//首先确定此餐品是否可以出售
	    var sold = true;
	    if (dish.SoldOutStatus) {
	        var node = $("div#PhoneOrder input:radio[name='Node']:checked").val();
	        for (var i = 0; i < dish.SoldOutStatus.length; i++) {
	            if (node == dish.SoldOutStatus[i]) {
	                sold = false;
	                break;
	            }
	        }
	    }
		if(!sold)
		{
			if(!confirm(name+"目前分店无法制作,是否继续?"))
				return false;
		}
		



		if (detail.DishName === "VIP独享" && dish.ClassName == "套餐") {

		    //VIP才能点
		    if (!customer.VIP_Number) {
		        alert("只有VIP才能点此餐品！");
		        return false;
		    }

		    //周一到周五才能点
		    var weekDay = new Date().getDay();
		    if (weekDay < 1 || weekDay > 5) {
		        alert("周一到周五才能点VIP独享！");
		        return false;
		    }
		    //检测是否已经点过‘VIP独享'
		    var find = false;
		    $("li", "#orderDetails").each(function () {
		        var d = $(this).data("data");
		        if (d.DishName === "VIP独享") {
		            find = true;
		            return false;
		        }
		    }
            );
		    if (find) {
		        alert("一次只能点一份VIP独享！");
		        return false;
		    }
		    //然后检测12小时内是否点过
		    if (customer.LastWeeklyDiscounted) {
		        var span = new Date() - new Date(customer.LastWeeklyDiscounted);
		        span = span / 1000 / 60 / 60;//换算成小时
		        if (span <= 12) {
		            alert("一天只能点一次VIP独享！");
		            return false;
		        }
		    }

		    
		    var a = createDetail(vipDishes[weekDay - 1].DishName);
		    a.Price = vipDishes[weekDay - 1].Price;
		    detail.Attaches.push(a);
		}


		if(dish.Name=="套餐(3人)"&&dish.ClassName=="套餐")
		{
			addUpdateSuit3($(this));
			return false;
		}
		
		if(dish.Name=="套餐(2人)"&&dish.ClassName=="套餐")
		{
			addUpdateSuit2($(this));
			return false;
		}
		if(dish.Name=="特惠午餐"&&dish.ClassName=="套餐")
		{
			addUpdateSuitLunch($(this));
			return false;
		}
		
		//处理比萨
		if(dish.ClassName=="比萨")
		{
			addUpdatePizza($(this));
			return false;
		}
		//三明治
		if(dish.ClassName=="三明治")
		{
			addUpdateSandwitch($(this));
			return false;
		}
		
		//饮品
		if(dish.ClassName=="饮品"||dish.ClassName=="咖啡")
		{
			addUpdateDrink($(this));
			return false;
		}
		
		addDish2Details(detail);
		
		return false;
	}
	
	function getDishImage(dish)
	{
		if(dish.Image)
			return "./../content/images/"+dish.Image;
		return $("img[name='"+getRealName(dish.Name||dish.DishName)+"']",opener.document).attr("src");
	}
	
	function createDish(dish)
	{
		var $template=$("#DishesDiv ul.dishList li.template.dish");
		var $newDish= $template
		.clone()
		.removeClass("template")
		.click(
			function()
			{
			    $(this)
				.effect('slide',{mode:'hide',direction:'left'},200,
					function()
					{
					    onDishClick.call(this);
					    $(this)
						.effect('slide',{mode:'show',direction:'right'},500);
					}
				);
			}
		)
        .data("data",dish)
		.show();
		
		
		$("img.dish",$newDish).attr(
		{
			src:getDishImage(dish),
			alt:dish.Name||dish.DishName
		}
		);
		
		$("span.dishName",$newDish).html(dish.Name||dish.DishName);
		$("span.price",$newDish).html(dish.Price);
		
		$('.boxgrid .cover',$newDish).hover(
					function () 
					{
						$(this).stop().animate({ top: '30px' }, { queue: false, duration: 300 });
					}, 
					function () 
					{
						$(this).stop().animate({ top: '100px' }, { queue: false, duration: 300 });
					}
				);
		
		
		
		return $newDish;
		
	}

    //加载餐品
    $.getJSON("/dishes/getdishesforphoneorder",
    function(r)
    {
        if (r.successed) {
            var list = JSON.parse(r.result);
            $.each(list, function () {
                Dishes[this.Name] = this;
            });
            loadClasses();
        }
        else {
            alert("加载餐品失败!");
        }
    }
    );
	
	//清空菜单
	function emptyMenu()
	{
		$("ul.dishList li:not(.template)").remove();
		$("ul.dishList>div").remove(); //effect动画会残留一个div,所以要清除掉
	}
	
	function loadDishesByClass(className)
	{
		
		emptyMenu();
		
		var back=
		{
			Name:"返回",
			Price:"",
			Image:"back.jpg"
		};
		createDish(back)
		.appendTo("ul.dishList")
		.unbind()
		.click(function()
		{
			
			$("ul.dishList")
			.effect('slide',{mode:'hide',direction:'right'},500,
			function()
			{
				loadClasses();
				$(this)
				.effect('slide',{mode:'show',direction:'right'},500);
			}
			);
		}
		);
		


		$.each(Dishes,function(i,dish)
		{		
			if(className==dish.ClassName)
			{
				var $newDish=createDish(dish);
				$newDish
				.appendTo("ul.dishList");	
			}			
		}
		);
		
		//排序
		var liArray=$("ul.dishList li:not(.template):gt(0)").get();
		liArray.sort(function(a,b)
		{
			var countA=$(a).data("data").Count;
			var countB=$(b).data("data").Count;
			if(countA>countB) return -1;
			if(countA<countB) return 1;
			return 0;
		}
		);
		
		$.each(liArray,function(index,li)
		{
			$("ul.dishList").append(li);
		}
		);

		
		
		if(className=="比萨"||className=="三明治")
		{
			dealPizza_Sandwitch();
		}		
		
	}
	
	
	 //获取餐品的名字,去掉后面的数字9或者12或者'('
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
	
	function createDetail(dishName)
	{
	    var newDetail = {};
	    var d=null;
	    if (typeof (dishName) === "string") {
	        d = getDishByName(dishName);
	    }
	    else if (typeof (dishName) === "object") {
	        d = dishName;
	    }
		newDetail.DishName = d.Name;
		newDetail.Price = d.Price;
		newDetail.Amount = 1;
		newDetail.Attaches = [];
		newDetail.Remark = "";

		return newDetail;
	}
	
	//处理比萨和三明治餐品
	function dealPizza_Sandwitch()
	{
		var $dishes=$("#DishesDiv ul.dishList li:not(.template)");
		var pizzaes=[];//保存已经处理过的餐品
		for(var i=0;i<$dishes.length;i++)
		{
			var dish=$($dishes[i]).data("data");
			if(dish.ClassName=="比萨"||dish.ClassName=="三明治")
			{
				$($dishes[i]).hide();
				var realName=getRealName(dish.Name);
				if(pizzaes.indexOf(realName)<0)
				{
					var newDish=createDetail(dish);
					newDish.Price="";
					newDish.Name = realName;
					newDish.Image = dish.Image;
					newDish.ClassName = dish.ClassName;
					var $new=createDish(newDish);
					$new.insertBefore($dishes[i]).show();
					pizzaes.push(realName);
				}			
			}
		}
	}
	
	
	var classImages=
	{
		'比萨':'超级至尊.jpg',
		'三明治':'三明治12.jpg',
		'主食':'红酒煨培根焗饭.jpg',
		'小吃':'奥尔良烤翅.jpg',
		'饮品':'草莓奶昔.jpg',
		'沙拉':'鸡蛋沙拉.jpg',
		'咖啡':'卡布奇诺.jpg',
		'汤':'罗宋汤.jpg',
		'甜点':'单球.jpg',
		'套餐':'小吃.jpg'
	}
	;
	
	//加载类别菜单
	function loadClasses() 
	{
		emptyMenu();
		
	    $.each(Dishes,function()
	    {
	        var target=this;
	        //判断此类别是否加入过
	        if($("ul.dishList li:contains("+target.ClassName+")").length==0)
	        {
	            var dish=
				{
				    Name:target.ClassName,
				    Price:"",
				    Image:classImages[target.ClassName]
				};
	            var $newDish=createDish(dish);
	            $newDish
				.appendTo("ul.dishList")
				.unbind()
				.click(function()
				{
				    var group=$(this).data("data"); 
				    $("ul.dishList")
					.effect('slide',{mode:'hide',direction:'right'},200,
					function()
					{
						  
					    loadDishesByClass(group.Name);
					    $(this)
						.effect('slide',{mode:'show',direction:'right'},200);
					}
					);
					
				}
				);
	        }
	    }
		);
	}
	
	
	
	
	function finishOrder()
	{
	    var vip=$("div#PhoneOrder input#vip").val();

		var phoneOrder=
		{

			Phone:$("div#PhoneOrder input#phone").val(),
			Address:$("div#PhoneOrder input#address").val().trim(),
			Remark:$("div#PhoneOrder input#remark").val(),
			Total:$("div#PhoneOrder span.total").html(),
			Discount:vip?9.5:10,
			CustomerCount:1,
			Time:new Date().format("yyyy-mm-dd HH:MM:ss"),
			Node:$("div#PhoneOrder input:radio[name='Node']:checked").val(),
			OrderType:$("div#PhoneOrder input:radio[name='orderType']:checked").val(),
			PayingType:$("div#PhoneOrder input:radio[name='Pay']:checked").val()
		};
		
		if (!phoneOrder.Node) {
		    alert("请选择分店！");
		    return;
		}
		if (!phoneOrder.OrderType) {
		    alert("请选择类型！");
		    return;
		}

		if (!phoneOrder.PayingType) {
		    alert("请选择支付方式！");
		    return;
		}

		var details=[];
		


		var temp=getDishByName(phoneOrder.OrderType);
		var detail=
		{
			DishName:temp.Name,
			Price:temp.Price,
			Amount:1
		};
		details.push(detail);

		
		$("div#PhoneOrder ul#orderDetails li")
		.each(function()
		{
			var detail=$(this).data("data");

			details.push(detail);
		}
		);
		
		phoneOrder.Details=details;
		

		
		$("#loadingDialog").dialog({modal:true});
		$.post("/phoneorder/uploadphoneorder",
		{
			jsonData:JSON.stringify(phoneOrder)
		},
		function(result)
		{
			if(result.result=="success")
			{
				window.close();
			}
			else
			{
				$("#loadingDialog").dialog('close');
				alert(result.errorMessage);
			}
		}
		)
		.error(function()
		{
			$("#loadingDialog").dialog('close');
			alert("发生错误,请重试!");
			
		}
		);
		
		
		
		
	}
	
	//自定义确认对话框
	function myconfirm(message,onOK,onCancel)
	{
		var $dialog=$("#confirmDialog");
		$("span.message",$dialog).html(message);
		

		$("input.btnOK",$dialog)
		.unbind()
		.click(
			function()
			{
				$dialog.dialog('close');
				if(onOK)
					onOK();
			}
		);
		
		

		$("input.btnCancel",$dialog)
		.unbind()
		.click(
			function()
			{
				$dialog.dialog('close');
				if(onCancel)
					onCancel();
			}
		);
		
		
		$dialog.dialog({modal:true,title:"提问"});
	}
	

	function phoneOrderSlideOut()
	{
		$("#PhoneOrder")
		.stop()
		.animate({ left: '-16px' }, { queue: false, duration: 500 });
	}
	
	function phoneOrderSlideIn()
	{
		$("#PhoneOrder")
		.stop()
		.animate({ left: '-550px' }, { queue: false, duration: 500 });
	}

	$('#PhoneOrder').hover(phoneOrderSlideOut,phoneOrderSlideIn);

	$("#saveOrder")
	.button()
	.click(function()
	{
		var a=$("div#PhoneOrder input#address").val().trim();
		if(!a)
		{
			alert("请输入客户地址!");
			return;
		}
		phoneOrderSlideOut();			
			myconfirm("确认要提交订单吗?",
			function()
			{
				finishOrder();
			}
			);
	}
	);
	
	
	$("div#PhoneOrder input:radio[name='orderType']")
	.click(function()
	{
		updateDetailUI();
	}
	);
	

	var customer=null;
	
	function readCustomerInfo(phone)
	{
		$.getJSON("/customers/GetCustomerInfoByPhone",{phone:phone},
		function(c)
		{
		    c = c ||
                {
                    Addresses: []
                };
			customer=c;
			for(var i=0;i<customer.Addresses.length;i++)
			{
				customer.Addresses[i].label="";
				customer.Addresses[i].value=customer.Addresses[i].Where;
			}
			//if(customer.ID) //如果有此客户就显示出来
			//{
			//	//$("div#PhoneOrder input#phone").val(customer.Phone);
			//	//$($("div#PHoneOrder input:radio[name='sex']")[customer.Sex]).check(true);
			//	//
			//	$("div#PhoneOrder input:radio[value='外卖']").check(true);
			//	//$("div#PhoneOrder input:radio[value='"+customer.Addresses[0].LastNode+"']").check(true);
			//	updateDetailUI();
			//}
		   
            //排序
			customer.Addresses.sort(function (a, b) {
			    a.LastUsedTime = a.LastUsedTime || "1980-03-19 00:00";
			    b.LastUsedTime = b.LastUsedTime || "1980-03-19 00:00";
			    if (a.LastUsedTime < b.LastUsedTime) return 1;
			    if (a.LastUsedTime > b.LastUsedTime) return -1;
			    return 0;
			});
            

			$("div#PhoneOrder input#address").val(customer.Addresses.length>0?customer.Addresses[0].Where:"");


			$("div#PhoneOrder input#vip").val(customer.VIP_Number);


		    updateDetailUI();

			$("#address").autocomplete({
				minLength:0,
				source:function(req,res)
				{
					res(customer.Addresses);
				},
				focus:function(event,ui)
				{
					$("#address").val(ui.item.Where);
					return false;
				},
				select:function(event,ui)
				{
					$("#address").val(ui.item.Where);
					return false;
				}	
			}				
			)
			.data("autocomplete")._renderItem=
			function(ul,item)
			{
				return $("<li style='text-align:left;'>")
				.data("item.autocomplete",item)
				.append("<a>"+item.Where+"</a>")
				.appendTo(ul);
			};
		}
		);
	}
	
	$("div#PhoneOrder input#phone")
	.change(function()
	{
		readCustomerInfo($(this).val());
	}
	);
	
	
	
	var phone=$("div#PhoneOrder input#phone").val();
	if(phone)
		readCustomerInfo(phone);
	
	self.resizeTo(screen.availWidth,screen.availHeight);
	self.moveTo(0,0);
	
	
	
    
	
	
}
);