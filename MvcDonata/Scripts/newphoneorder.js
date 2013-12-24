    bScroll = true; //是否滚动,如果text box获取焦点就不能滚动
    
    
    function getDishByName(name)
    {
    	return $(stringFormat("div#Dishes .dish .add2Order[dishName='{0}']",name));
    }
    
    function addAttach($attach,$item)
    {
    	var $templateAttach=$(".orderDetails .item.detail.template .attach");
    	var $newAttach=$templateAttach.clone();
    	
    	$(".dishName",$newAttach).html($attach.attr("dishName"));
    	$(".price",$newAttach).html($attach.attr("price"));
    	$(".count",$newAttach).html(1);
    	
    	$newAttach.insertAfter($(".remark",$item)).show();
    }
    function addSandwichAttach($item,$dialog)
    {
    	//首先清除先前的attach
		$(".attach",$item).remove();
		
		
		var size=$("input:radio:checked",$dialog).val();
		
		//检测是否加入 鸡蛋沙拉 和 双份肉
		$("input:checkbox:checked",$dialog).each(
				function(index,box)
				{
					addAttach(getDishByName($(box).val()+size),$item);
				}
		);
    }
    function addPizzaAttach($item,$dialog)
    {
    	//首先清除先前的attach
		$(".attach",$item).remove();
		
		
		//检测是否加入卷边
		if($("input.jb:enabled",$dialog).check())
		{						
			//获取卷边的价格
			if($("input:radio.small",$dialog).check()) //如果是9寸饼
			{
				addAttach(getDishByName("9寸芝心卷边"),$item);
			}
			else
			{
				addAttach(getDishByName("12寸芝心卷边"),$item);
			}
		}
		
		//检测是否加入双份奶酪
		
		if($("input.nl:enabled",$dialog).check())
		{
			if($("input:radio.small",$dialog).check())
			{
				addAttach(getDishByName("9寸双份奶酪"),$item);
			}
			else
			{
				addAttach(getDishByName("12寸双份奶酪"),$item);
			}
		}
    }
    
    function add2Order($dish,$origItem)
    {
    	var $template = $(".template");
    	var $new;
        if($origItem==null)
        	$new = $template.clone().removeClass("template");
        else
        	$new=$origItem;

        $(".dish .dishName span", $new).html($dish.attr("dishName"));
        $(".dish .dishName", $new).attr({ "typeName": $dish.attr("typeName") });
        
        

        $(".dish .dishName img", $new).attr({ src: $("img",$dish).attr("src") });
        
        $(".dish .price", $new).html($dish.attr("price"));
        
        if($origItem==null)
        	$(".dish .count", $new).html(1);


        if($origItem==null)
        {
        	initOperation($($new).insertBefore(".item.total").show());
        }
        
        
        $("#PhoneOrder").animate({ scrollTop: $(".orderDetails").offset().top + $(".orderDetails").height() }, 1000);
        
        
        
        refreshTotal();
        
        return $new;
    }
    
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
    function initSpecialDrinkEditDialog(dish,$dialog)
    {

    		dishName=$(".dish .dishName span",$item).html();
    		$("input:radio.specialDrink",$dialog).checkGroup
    		(
    				[
    		 	
    				 ]	
    		);
    	
    }
    
    //通用编辑对话框的初始化函数
    function initDialog(dishData,$dialog,$item)
    {
    	var $ok=$dialog.find(".btnOK").unbind();
    	var $cancel=$dialog.find(".btnCancel").unbind();
    	
    	$(".remark",$dialog).val(dish.remark);

    	
    	$(".btnOK",$dialog).click(
    			function()
    			{
    				dishData.remark=$(".remark",$dialog).val();
    				if($item==null)
    				{
    					$(".item.detail.template")
        				.clone()
        				.dish(dishData)
        				.removeClass("template")
        				.insertBefore(".item.total")
        				.show();
    				}
    				else
    				{
    					$item.dish(dishData);
    				}
    				
    			}
        	);
    	
        
        $(".btnCancel",$dialog).click(
        		function()
        		{
        			$dialog.dialog('close');
        			$dialog.remove();
        		}
        );
    }
    
    
    function initSandwichDialog($item,$dialog)
    {
    	var dishName="";
    	//修改已点的餐品
    	if($item.hasClass("item")&&$item.hasClass("detail"))
    	{
    		dishName=$(".dish .dishName span",$item).html();
    	
    		$("input.rou",$dialog).enable(
    			dishName.indexOf("鸡蛋沙拉三明治")<0
    			&&dishName.indexOf("蔬菜奇士")<0
    			).check(
    					$(".attach .dishName:contains('加双份肉')",$item).length>0
    					);
    	
    		$("input.nl",$dialog).check(
    			$(".attach .dishName:contains('加鸡蛋沙拉')",$item).length>0
    			);
    	
    		$("input[name='sandwich']",$dialog).checkGroup(
    			[
    			 	dishName.indexOf("6")>=0,
    			 	dishName.indexOf("12")>=0			 	
    			]
    		);
    		
    		
    		
    		
    		dishName=getPizzaName(dishName);
    		
    		
    		$dialog.attr("title",dishName);
    		
    		
    		
    		$("input.btnOK",$dialog).click(
    			function()
    			{
    				dishName+=$("input:radio:checked",$dialog).val();
					
					add2Order(getDishByName(dishName),$item);
					
					addSandwichAttach($item,$dialog);
    				
					refreshTotal();
    			}
    		);
    	}
    	else if($item.hasClass("add2Order"))
    	{
    		$("input:checkbox",$dialog).check(false);
    		$("input.remark",$dialog).val("");
    		
    		var dishName=$item.attr("dishName");
    		
    		
    		
    		$dialog.attr("title",dishName);
    		
    		$("input.btnOK",$dialog).click(
    				function()
    				{
    					dishName+=$("input:radio:checked",$dialog).val();
    					var $dish=$(stringFormat("div#Dishes .dish .add2Order[dishName='{0}']",dishName));
    					var $new=add2Order($dish);
    					addSandwichAttach($new,$dialog);
    					
    					//写入备注
    					onRemarkDialogOK($new,$dialog);
    					
    					refreshTotal();
    				}
    		);
    		
    	}
    }
    
    function initPizzaDialog($item,$dialog)
    {
    	var dishName="";
    	
    	$("input:radio",$dialog).unbind().click(
				function()
				{
					$("input.jb",$dialog)
					.enable(!$("input:radio[name='pizza'].italy",$dialog).check());
				}
		);
    	
    	//修改已点的餐品
    	if($item.hasClass("item")&&$item.hasClass("detail"))
    	{
    		dishName=$(".dish .dishName span",$item).html();
    	
    		$("input.jb",$dialog).enable(
    			dishName.indexOf("薄")<0 //如果是薄饼,就不能加卷边
    			&&dishName.indexOf("香烤牛肉") //香烤牛肉,荷塘月色,BBQ鸡肉不能加卷边
    			&&dishName.indexOf("荷塘月色")
    			&&dishName.indexOf("BBQ鸡肉")
    			).check(
    					$(".attach .dishName:contains('卷边')",$item).length>0
    					);
    	
    		$("input.nl",$dialog).check(
    			$(".attach .dishName:contains('双份奶酪')",$item).length>0
    			);
    	
    		$("input[name='pizza']",$dialog).checkGroup(
    			[
    			 	dishName.indexOf("9")>=0,
    			 	dishName.indexOf("12")>=0&&dishName.indexOf("薄")<0,
    			 	dishName.indexOf("12薄")>=0			 	
    			]
    		);
    		
    		
    		
    		
    		dishName=getPizzaName(dishName);
    		
    		
    		$dialog.attr("title",dishName);
    		
    		
    		
    		$("input.btnOK",$dialog).click(
    			function()
    			{
    				dishName+=$("input:radio:checked",$dialog).val();
					
					add2Order(getDishByName(dishName),$item);
					
					addPizzaAttach($item,$dialog);
    				
					refreshTotal();
    			}
    		);
    	}
    	else if($item.hasClass("add2Order"))
    	{
    		$("input:checkbox",$dialog).check(false);
    		$("input.remark",$dialog).val("");
    		
    		var dishName=$item.attr("dishName");
    		
    		
    		
    		$dialog.attr("title",dishName);
    		
    		$("input.btnOK",$dialog).click(
    				function()
    				{
    					dishName+=$("input:radio:checked",$dialog).val();
    					var $dish=$(stringFormat("div#Dishes .dish .add2Order[dishName='{0}']",dishName));
    					var $new=add2Order($dish);
    					addPizzaAttach($new,$dialog);
    					
    					//写入备注
    					onRemarkDialogOK($new,$dialog);
    					
    					refreshTotal();
    				}
    		);
    		
    	}
    	
    	
    	
    }
    
    


    function refreshTotal()
    {
    	var prices=$(".orderDetails .item:not(.template) .price");
    	var counts=$(".orderDetails .item:not(.template) .count");
    	
    	var total=0;
    	
    	$.each(prices,
    			function(index,data)
    			{
    				total+=$(data).html()*$(counts[index]).html();
    			}
    		);
    	
    	$(".orderDetails .total .total").html(total);
    }
    
    function runSpecialEditDialog(dish,$item)
    {
    	var $dialog=null;
    	//此餐品是否有指定的编辑对话框

    	if(dish.editDialog)
    	{
    		$dialog=$("#"+editDialog);
    	}
    	else
    	{
      	
    		//此类别是否有定制的编辑对话框
    		var $d=$(stringFormat("div.EditDialog[typeName='{0}']",dish.typeName));
        
        	if($d.length>0)
        	{
        		$dialog=$d;
        	}
    	}
    	if(!$dialog)
    		return false;
    	
        initRemarkDialog(dish,$dialog);
        	
       var init=$dialog.attr("init");
       if(init)
       {
       	   eval(stringFormat("{0}({1},$dialog);",init,"$item"));
       }
        	
       $dialog.dialog({modal:true,width:300});
           
       return true;
    }
    
    function initOperation(parent) {
        $(".itemEditRemark", parent).click(function () {

            $item = $(this).parents(".item");


            var dishName = $(".dishName span", $item).html();
            var typeName = $(".dishName", $item).attr("typeName");
            //如果有定制的编辑对话框就运行,否则就运行通用对话框
            if(!runSpecialEditDialog($item,typeName))
            {
            
            	var $dialog=$("div.RemarkDialog[typeName='all']");
            
            	initRemarkDialog($item,$dialog);
            
            	
            
            	$dialog.dialog({ modal: true });
            
            }
            
            return false;
        }
        );

        $(".itemAdd", parent).click(function () {
            $item = $(this).parents(".item");
            var $count = $(".dish,.attach", $item).find(".count");

            var count = $count.html() - 0 + 1;

            if (count == 0)
                count++;

            $count.html(count);
            
            refreshTotal();
            
            return false;
        }
        );

        $(".itemSub", parent).click(function () {
            $item = $(this).parents(".item");
            var $count = $(".dish,.attach", $item).find(".count");

            var count = $count.html() - 1;

            if (count == 0)
                count--;

            $count.html(count);
            
            refreshTotal();
            
            return false;
        }
        );


        $(".itemDel", parent).click(function () {

            var $item = $(this).parents(".item");

            var count = $(".count", $item).html() - 0;

            var dishName = $(".dishName", $item).find("span").html();

            if (count > 1) {
                if (confirm(stringFormat("确定要删除{0}份{1}吗?", count, dishName))) {
                    $(this).parents(".item").remove();
                }
            }
            else
                $(this).parents(".item").remove();

            refreshTotal();
            
            return false;
        }
        );
    }
    
    $(document).ready(function () {


		$("div.remark .btnCancel").click(
			function()
			{
				$(this).parents("div.remark").dialog('close');
			}
		);


        $('.boxgrid').hover(function () {
            $(".cover", this).stop().animate({ top: '62px' }, { queue: false, duration: 300 });
        }, function () {
            $(".cover", this).stop().animate({ top: '100px' }, { queue: false, duration: 300 });
        });

        $.localScroll({
            easing: 'easeInOutBack',
            speed: 1500
        });


        $("input[type='text']")
        .focus(function () {
            bScroll = false;
        }
        )
        .blur(function () {
            bScroll = true;
        });



        $('#PhoneOrder').hover(function () {
            this.stop().animate({ left: '0px' }, { queue: false, duration: 500 });
        }, function () {
             this.stop().animate({ top: '-550px' }, { queue: false, duration: 500 });
        });

        $(document).keypress(function (e) {
            if (bScroll) {
                var char = String.fromCharCode(e.keyCode);
                $("#nav_" + char).click();
            }
        }
        );


        $(".add2Order").click(function () {      
        	var dish=$(this).dish();
            //如果有定制的编辑对话框就运行,然后返回
            if(runSpecialEditDialog(dish))
            	return false;
            
            add2Order(dish);
            
            return false;
        }
        );

        $(".dialog").bind("dialogclose",
        		function()
        		{
        			$(this).dialog("destroy");
        		}
        );

    }
);