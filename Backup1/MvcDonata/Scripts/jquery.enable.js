(function ($) {
    $.fn.enable = function (enabled) {
    	
    	if(enabled==null)
    	{
    		return !this.attr("disabled");
    	}
    	
        if (enabled) {
            this.removeAttr("disabled");
        }
        else {
            this.attr({ disabled: true });
        }
        return this;
    };
    
    $.fn.visible=function(bShow)
    {
    	if(bShow)
    		this.show("slow");
    	else
    		this.hide("slow");
    	return this;
    };
	
	
	
})(jQuery);

(function($) {
	$.fn.check=function(checked)
	{
		if(typeof checked==='undefined')
		{
			return this[0].checked;
		}
		
		if(checked)
		{
			this.attr({checked:""});
		}
		else
		{
			this.removeAttr("checked");
		}
		return this;
	}
})(jQuery);

(function($){
	$.fn.checkGroup=function(conditions)
	{
		for(var i=0;i<conditions.length;i++)
		{
			if(conditions[i])
			{
				$(this[i]).check(true);
				break;
			}
		}
	}
}
)(jQuery);



