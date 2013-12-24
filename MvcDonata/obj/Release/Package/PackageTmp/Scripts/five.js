 $(document).ready(function () {
        var $canvas = $("#myCanvas");
        var context = $canvas.get(0).getContext("2d");
        var BLACK=2;
        var WHITE=1;
        var EMPTY=0;
        var size=15;//棋盘大小
        var directions=
        [
        	{row:1,column:1},
        	{row:1,column:0},
        	{row:0,column:1},
        	{row:1,column:-1}
        ]
        var steps=[];
        var situations=[];
        situations[BLACK]=[];
        situations[WHITE]=[];
        var table=[];
        
        var LastChong=[0,10,20,70,90,3000];
        var LastHuo=  [0,20,50,80,500,3000];
        var FirstChong=[0,30,50,80,100,4000];
        var FirstHuo=  [0,40,70,90,1000,4000];
        

        var first=BLACK;
        var computer=BLACK;
        
        var mousePos={row:0,column:0};
        
        $canvas.mousemove(function(e)
        {
        	var column=((e.pageX-$(this).offset().left)/length).round()-1;
        	var row=((e.pageY-$(this).offset().top)/length).round()-1;
        	
        	
        	if(column<0||column>=size)
        		return;
        	if(row<0||row>=size)
        		return;
        	
        	$.post("/test/calcInfo",
        	{
        		json:JSON.stringify(steps),
        		row:row,
        		col:column
        	},
        	function(result)
        	{
        		/*
        		var total=0;
        		for(var i=0;i<directions.length;i++)
	        	{
					total+=result.posSituations[i].Value;
	        		$("#situations div:eq("+i+")").children("span:eq(1)").html(result.posSituations[i].Name+" "+result.posSituations[i].PieceString+" "+result.posSituations[i].Value);
	        	}
	        	
	        	$("#situations div:eq(4)").children("span:eq(1)").html(total);
	        	*/
	        	$("#white").html(result.whiteSituation);
	        	$("#black").html(result.blackSituation);
        	}
        	);
        	return;
        	
        	
        	
        	return;
        	mousePos={row:row,column:column};	
        	
        	if(row<0||row>=size||column<0||column>=size)
        		return;
        	
        	var total=0;
        	for(var i=0;i<directions.length;i++)
        	{
        		var s=calcSituation(first,first,row,column,directions[i])+calcSituation(first,BLACK,row,column,directions[i]);
        		total+=s;
        		$("#situations div:eq("+i+")").children("span:eq(1)").html(s);
        	}
        	$("#situations div:eq("+4+")").children("span:eq(1)").html(total);
        }
       	);
        
        //计算每个格子的大小
        var length=(Math.min($canvas.width(),$canvas.height())/(size+2)).round();

        $canvas.click(function(e)
        {
        	var column=((e.pageX-$(this).offset().left)/length).round()-1;
        	var row=((e.pageY-$(this).offset().top)/length).round()-1;
        	

        	

        	
        	
        	step(row,column,WHITE);       	
        	
        }
       	);
       	
       	
       	function step(row,column,pieceType)
       	{
       		if(table[row][column]!=EMPTY)
       		{
       			return;
       		}
       		table[row][column]=pieceType;
       		steps.push( //记录下棋步骤
       			{
       				Pos:
       					{
       						Row:row,
       						Col:column
       					},
       				Side:pieceType
       			}
       		);
       		
       		
       		updateChessBoard(context);
       		
   			if(isFive())
        	{
        		if(first==WHITE)
        		{
        			alert("白旗赢了!");
        		}
        		else
        		{
        			alert("黑棋赢了!");
        		}
        		for(var row=0;row<size;row++)
				{
					for(var column=0;column<size;column++)
					{
						table[row][column]=EMPTY;
					}
				}
				steps.length=0; //清空记录
        	}
   		
       		
       		first=first==BLACK?WHITE:BLACK;
       		
       		if(first==computer) //如果是电脑走棋
       		{
       			computerStep();
       		}
       		
       	}
        
        situations[WHITE]=[];
        situations[BLACK]=[];
        for(var i=0;i<size;i++)
        {
        	
        	table[i]=[];
        	situations[WHITE][i]=[];
        	situations[BLACK][i]=[];
        	for(var j=0;j<size;j++)
	        {
	        	table[i][j]=EMPTY;
	        	
	        	situations[WHITE][i][j]=0;
	        	situations[BLACK][i][j]=0;        	
	        	
	        }
        }
        
        
        
        function drawChessBoard(cxt)
        {
        	cxt.save();
        	
        	cxt.clearRect(0,0,$canvas.width(),$canvas.height());
        	
        	cxt.lineWidth=1;
  
        	cxt.strokeRect(0,0,$canvas.width(),$canvas.height());
        	
        	for(var i=1;i<=size;i++)
        	{
        		//画横线
        		cxt.beginPath();
        		cxt.moveTo(length,length*i);
        		cxt.lineTo(size*length,length*i);
        		cxt.closePath();
        		cxt.stroke();
        		
        		cxt.fillText((i-1),25,length*i);
        		
        		//画竖线
        		cxt.beginPath();
        		cxt.moveTo(length*i,length);
        		cxt.lineTo(length*i,size*length);
        		cxt.closePath();
        		cxt.stroke();
        		
        		cxt.fillText((i-1),length*i,30);
        	}
        	
        	drawDot(cxt,3,3);
        	drawDot(cxt,3,7);
        	drawDot(cxt,3,11);
        	
        	drawDot(cxt,7,3);
        	drawDot(cxt,7,7);
        	drawDot(cxt,7,11);
        	
        	drawDot(cxt,11,3);
        	drawDot(cxt,11,7);
        	drawDot(cxt,11,11);
        	
        	cxt.restore();
        }
        
        function drawDot(cxt,row,column)
        {
        	row++;
        	column++;
        	var x=column*length;
        	var y=row*length;
        	cxt.strokeStyle="rgb(0,0,0)";
			cxt.beginPath(); // Start the path 
			cxt.arc(x, y, 5, 0, Math.PI*2, false); // Draw a circle 
			cxt.closePath(); // Close the path 
			cxt.stroke();
			cxt.fill(); // Fill the path 
        }
		
		//画棋子,黑或者白,row和column从零开始到14
		function drawPiece(cxt,row,column,pieceType)
		{
			cxt.save();
			
			//计算格子的坐标
			var x=(column+1)*length;
			var y=(row+1)*length;
			cxt.translate(x,y);
			
			var color="rgb(0,0,0)";
			if(pieceType===WHITE)
				color="rgb(255,255,255)";
			cxt.fillStyle=color;
			cxt.strokeStyle="rgb(0,0,0)";
			context.beginPath(); // Start the path 
			context.arc(0, 0, length/3, 0, Math.PI*2, false); // Draw a circle 
			context.closePath(); // Close the path 
			context.stroke();
			context.fill(); // Fill the path 
			
			cxt.restore();
		}
		function drawTipRect(cxt,row,column,color)
		{
			cxt.save();
			
			var x=(column+1)*length;
			var y=(row+1)*length;
			cxt.translate(x,y);
			
			cxt.strokeStyle=color;
			cxt.strokeRect(-length/3-2,-length/3-2,length*2/3+4,length*2/3+4);
			
			cxt.restore();
		}
		
		function updateChessBoard(cxt)
		{
			drawChessBoard(cxt);
			for(var row=0;row<size;row++)
			{
				for(var column=0;column<size;column++)
				{
					if(table[row][column]===EMPTY)
						continue;
					drawPiece(cxt,row,column,table[row][column]);
				}
			}
			
			if(steps.length>0) //最后一步画一个红框
			{
				drawTipRect(cxt,steps[steps.length-1].Pos.Row,steps[steps.length-1].Pos.Col,"rgb(255,0,0)");
			}
			//画鼠标所在点的蓝色框
			if(mousePos.row>=0&&mousePos.row<size&&
				mousePos.column>=0&&mousePos.column<size
				)
				drawTipRect(cxt,mousePos.row,mousePos.column,"rgb(0,0,255)");
		}
		
		
			

		//判断是否是五连
		function isFive()
		{
			var last=steps[steps.length-1];
			var row=last.Pos.Row;
			var column=last.Pos.Col;
			var side=table[row][column];
			for(var i=0;i<directions.length;i++)
			{
				var dir=directions[i];
				
				var pos1=getLastSamePosition({row:row,column:column},dir,side); //正方向走一下
				var pos2=getLastSamePosition({row:row,column:column},{row:-dir.row,column:-dir.column},side);//反方向走一下
				//行差和列差的最大值
				var length=Math.max(Math.abs(pos1.row-pos2.row)+1,Math.abs(pos1.column-pos2.column)+1);
				
				if(length>=5)
				{
					return true;
				}
			}
			return false;
		}
		
		computerStep();//首先电脑走棋
		
		// 电脑Ai走棋
		function computerStep()
		{
			//服务器计算
			/*$.post("/test/calcStep",
				{json:JSON.stringify(steps)},
				function(s)
				{
					step(s.Pos.Row,s.Pos.Col,s.Side);
					//alert(s.Pos.Row+"  "+s.Pos.Col);
				}
			);
			return;*/
			
			//本地计算
			calcAllSituations();
			//计算最大的situation
			var max=null;
			for(var row=0;row<size;row++)
			{
				for(var column=0;column<size;column++)
				{
					if(table[row][column]!=EMPTY)
						continue;
					var s=0;
					for(var i=0;i<directions.length;i++)
					{
						s+=calcSituation(first,computer,row,column,directions[i])+calcSituation(first,computer==WHITE?BLACK:WHITE,row,column,directions[i]);
					}
					if(!max)
					{
						max={max:s,posible:[{row:row,column:column}]};
					}
					else
					{
						if(max.max<s)
						{
							max={max:s,posible:[{row:row,column:column}]};
						}
						else if(max.max==s)
						{
							max.posible.push({row:row,column:column});
						}
					}
				}
			}
			var select=Math.floor(Math.random()*max.posible.length);
			
			step(max.posible[select].row,max.posible[select].column,computer);
		}
		function calcAllSituations()
		{
			for(var row=0;row<size;row++)
			{
				for(var column=0;column<size;column++)
				{
					if(table[row][column]!=EMPTY)
					{
						situations[WHITE][row][column]=0;
						situations[BLACK][row][column]=0;
						continue;
					}
					for(var i=0;i<directions.length;i++)
					{
						calcSituation(first,BLACK,row,column,directions[i]);
						calcSituation(first,WHITE,row,column,directions[i]);
					}
					
				}
			}
		}
		
		//计算一个方向上的态势
		function calcSituation(whoFirst/*谁先手*/,side,row,column,dir)
		{
			var s=0;
			if(table[row][column]==EMPTY)
			{
				var pos1=getLastSamePosition({row:row,column:column},dir,side);
				var pos2=getLastSamePosition({row:row,column:column},{row:-dir.row,column:-dir.column},side);
				//行差和列差的最大值
				var index=Math.max(Math.abs(pos1.row-pos2.row)+1,Math.abs(pos1.column-pos2.column)+1);
				//判断是活还是冲
				var isLive=true;
				pos1={row:pos1.row+dir.row,column:pos1.column+dir.column};
				pos2={row:pos2.row-dir.row,column:pos2.column-dir.column};
				
				if(pos1.row<0||pos1.row>=size||pos1.column<0||pos1.column>=size)
				{
					isLive=false;
				}
				else if(pos2.row<0||pos2.row>=size||pos2.column<0||pos2.column>=size)
				{
					isLive=false;
				}
				else if(table[pos1.row][pos1.column]!=EMPTY)
				{
					isLive=false;
				}
				else if(table[pos2.row][pos2.column]!=EMPTY)
				{
					isLive=false;
				}
				var s=0;
				if(index>5)
					index=5;
				if(whoFirst===side)
				{
					s=isLive?FirstHuo[index]:FirstChong[index];
				}
				else
				{
					s=isLive?LastHuo[index]:LastChong[index];
				}
			}

			return s;
			
		}
		
		//获取此方向同一棋子最远的距离
		function getLastSamePosition(pos,dir,side)
		{
			while(pos.row+dir.row>=0&&pos.row+dir.row<size&&
					pos.column+dir.column>=0&&pos.column+dir.column<size&&
					table[pos.row+dir.row][pos.column+dir.column]===side)
			{
				pos={row:pos.row+dir.row,column:pos.column+dir.column};
			}
			return pos;
		}
		
		setInterval(
			function()
			{
				updateChessBoard(context);
			},
			100
		);
		
		
		

    }); 