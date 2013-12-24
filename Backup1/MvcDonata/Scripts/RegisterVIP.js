$(document).ready(
    function () {
        //改变主框架的颜色
        $("#main,footer").css({ 'background-color': '#5C87B2' });

        var code="";

        //发送验证码
        $("#sendCode").click(
            function () {

                $(this).enable(false);

                var phone = $("#phone").val();
                if (!phone||(phone.length!=11&&phone.length!=6)) {
                    alert("请输入手机号或者输入VIP号码！");
                    $(this).enable(true);
                    return false;
                }

                $.post("/customers/sendcode", { phone: phone },
                    function (result) {
                        if (result.result) {
                            code = result.result;
                            alert("验证码已经发送成功！");
                        }
                        else {
                            alert("验证码发送失败:" + result.errorMessage);
                        }
                        $(this).enable(true);
                    }
                    ).error(function () {
                        $("#sendCode").enable(true);
                        alert("验证码发送失败！");
                    }
                    );
            }
            );
        //end
            
        //加入VIP
        $("#addNewVIP").click(
            function () {

                var phone = $("#phone").val();
                if (!phone || phone.length != 11) {
                    alert("请输入手机号！");
                    return false;
                }
                if (!code) {
                    if(confirm("没有发送验证码，是否继续注册？")==false)
                     return false;
                }

                if (code != $("#code").val()) {
                    alert("验证码错误,请重试！");
                    return false;
                }

                var number = $("#number").val();
                if (!number) {
                    alert("请输入VIP卡号！");
                    return false;
                }

                if (number.length != 6) {
                    alert("请输入6位VIP卡号！");
                    return false;
                }

                $.post("/Customers/AddNewVIP",
                    {
                        phone: phone,
                        number: number
                    },
                    function (result) {
                        if (result.result) {
                            alert("保存成功!");
                            code = "";
                            $("#phone").val("");
                            $("#number").val("");
                        }
                        else {
                            alert("创建失败：" + result.errorMessage);
                        }
                    }
                    ).error(
                        function () {
                            alert("创建出现错误，请重试！");
                        }
                        );

                

                
            }
            );
        //end

        //通过电话查询VIP号码
        $("#query")
            .click(function () {
                var query = $("#queryPhone").val();
                if (!query) {
                    alert("请输入手机号");
                    return false;
                }

                $.post("/Customers/GetVIPByPhone", { phone: query },
                    function (result) {
                        if (result.successed) {
                            if (result.result) {
                                alert("电话号码为:"+result.result.Phone+" VIP号码为：" + result.result.VIP_Number);
                            }
                            else {
                                alert("没有找到对应的VIP号码！");
                            }
                        }
                        else {
                            alert("查询出现错误：" + result.errorMessage);
                        }
                    }
                    )
                .error(function () {
                    alert("查询出现错误，请重试！");
                }
                );
            }
            );
        //end

        //给VIP充值
        $("#charge").click(
            function () {

                var phone = $("#charge_phone").val();
                var number = $("#charge_number").val();
                var value = $("#charge_value").val();

                if (!phone) {
                    alert("请输入手机号码！");
                    return false;
                }
                if (!number) {
                    alert("请输入VIP号码！");
                    return false;
                }
                if (!value) {
                    alert("请输入充值金额！");
                    return false;
                }

                value = value - 0;
                if (value <= 0) {
                    alert("请输入大于零的充值金额！");
                    return false;
                }


                $.post("/Customers/Charge",
                    {
                        phone: phone,
                        number: number,
                        value: value
                    },
                    function (result) {
                        if (result.successed) {
                            alert("充值成功：" + result.result);
                            $(".charge input[type=number]").val("");
                        }
                        else {
                            alert("充值出现错误：" + result.errorMessage);
                        }
                    }
                    ).error(function () {
                        alert("充值出现错误，请重试！");
                    }
                    );
            }
            );
        //end
        
    }
    );