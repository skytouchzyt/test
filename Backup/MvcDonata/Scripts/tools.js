function stringFormat(strOrig) {

    var result = strOrig;

    for (var i = 1; i < arguments.length; i++) {

        result = result.replace("{" + (i - 1) + "}", arguments[i]);

    }

    return result;

}

function isArray(obj)
{
	return obj && //目标不能为空
	!(obj.propertyIsEnumerable('length')) && //目标的length属性是预定义的,这样就不能被枚举
	typeof obj === 'object' &&  //目标是一个对象
	typeof obj.length === 'number'; //目标的length是一个数字
	
};

Number.prototype.round=function(l)
{
	var sign=1;
	if(this<0)
		sign=-1;
	var temp=1;
	for(var i=0;i<l;i++)
		temp*=10;
	var result=parseInt(Math.abs(this)*temp+0.5);
	return result/temp*sign;
};

function getCharCode(event)
{
      var charcode = event.charCode;
      if(typeof charcode == "number" && charcode != 0){
            return charcode;
      }
      else
      {
            //在中文输入法下 keyCode == 229 || keyCode == 197(Opera)
            return event.keyCode;
      }
}	

