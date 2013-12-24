
    $(document).ready(function () {
		
		//添加新的地址节点
		function appendNewAddress(addressData)
		{
			var $template=$("#addresses li.template");
				var $li=$template.clone().removeClass("template");
				
				$li.insertBefore($template).show()
				.data("model",addressData)
				.find("input.txtAddress").val(addressData.Address).end()
				.children(".delete").click(
					function()
					{
						if(confirm("确定要删除此地址吗?"))
						{
							
							customer.Addresses.splice($li.index("ul#addresses li:not(.template)"),1);
							$li.remove();
						}
					}
					);
		}
		
		
        $("#insertAddress").click(function () {
			$("#dialogLoading").dialog({modal:true});
			$.getJSON("/customers/getAddressesByID/0",
					  function(result)
					  {
						if(!customer.Addresses)
						{
							customer.Addresses=[];			
						}
						customer.Addresses.push(result);
						appendNewAddress(result);
						$("#dialogLoading").dialog('destroy');
					  }
					);
            
        }
        );
		var id=$("#ID").val()||0;
		var customer={};
		$("#dialogLoading").dialog({modal:true});
		$.getJSON("/customers/getcustomerbyid/"+id,
			function(result)
			{
				$("#dialogLoading").dialog('destroy');
				customer=result;
				//把数据显示出库
				$("input.txtName").val(customer.Name); 
				$("input.txtPhone").val(customer.Phone);
				
				$(":radio").each(
					function(i,r)
					{
						r.checked=(i==customer.Sex);
					}
				);
				
				$.each(customer.Addresses,function(i,item)
							  {
								appendNewAddress(item);
							  }
					   );
			}
		);
		
		$("#save").click(function()
				{
					customer.Name=$("input.txtName").val();
					customer.Phone=$("input.txtPhone").val();
					customer.Sex=$(":radio:checked").val();
					$("ul#addresses li:not(.template)").each(function(i,a)
					{
						
						customer.Addresses[i].City=$("select",a).val();
						customer.Addresses[i].Address=$("input.txtAddress",a).val();
					}
					);
					$("#dialogLoading").dialog({modal:true});
					
					
					$.post("/customers/create",{data:JSON.stringify(customer)},
						function(result)
						{
							$("#dialogLoading").dialog('destroy');
						}
					);
				}
			);
    }
);
