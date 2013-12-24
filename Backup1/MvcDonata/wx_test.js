function (e)
{
    var t=jQuery,
        n=function()
        {
            var e=!1;
            inputs=m.getVal();
            
            switch(!1)
            {
                case!!inputs.account:
                    s.trigger("Warning",[f,"你还没有输入帐号！"]);
                    break;
                case!!inputs.password:
                    s.trigger("Warning",[l,"你还没有输入密码！"]);
                    break;
                case!!inputs.verify||!!c.data("isHide"):
                    s.trigger("Warning",[h,"你还没有输入验证码！"]),
                    r();
                    break;
                default:
                    e=!0
            }
            return e
        },
        r=function(){
            h.val(""),
            p.attr("src","/cgi-bin/verifycode?username="+m.getVal().account+"&r="+ +(new Date))
        },
        i=e.selector,
        s=t(i.error),
        o=t(i.errorArea),
        u=t(i.rememberAcct),
        a=t(i.changeImgLink),
        f=t(i.account).keydown(
            function(e){
                e.keyCode==13&&l.focus().select()}),
                l=t(i.password).keydown(function(e){e.keyCode==13&&d.click()}),
                c=t(i.verifyArea).data("isHide",1).hide(),
                h=t(i.verify).keydown(function(e){e.keyCode==13&&(l.val()?d.click():l.focus().select())}),
                p=t(i.verifyImg).click(r),
                d=t(i.loginBtn),
                v=function(e,t){return;var n,r,i
                };
    s.bind("Warning",function(e,t,n){o.removeClass("dn"),s.text(n).hide().fadeIn()}),
    p.bind(
        {
            load:function(){v([f,l]),c.show().data("isHide",0),h.focus().select()},
            error:function(){}
        }),
    s.bind("Response",
        function(e,t,n){
            o.removeClass("dn"),s.text(n).hide().fadeIn(),v([f,l],"N"),
            c.data("isHide")||r();
            switch(t){
            case"-3":
                l.focus().select();break;
            case"-6":
                h.focus().select();break;
            default:
            f.focus().select()
            }
            t!="-32"&&l.val("")});
    var m={
        showVerifyImg:r,
        submit:function(){
            if(!n())return;
            var e=m.getVal();
            t.post("/cgi-bin/login?lang=zh_CN",
                {
                    username:e.account,
                    pwd:t.md5(e.password.substr(0,16)),
                    imgcode:c.data("isHide")?"":e.verify,
                    f:"json"
                },
                function(t)
                {
                    var n=t.ErrCode+"",i;
                    u.hasClass("checkbox_checked")?WXM.Helpers.setCookie("remember_acct",e.account,30):WXM.Helpers.setCookie("remember_acct","EXPIRED",-1);
                    switch(n)
                    {
                        case"-1":i="系统错误，请稍候再试。";break;
                        case"-2":i="帐号或密码错误。";break;
                        case"-3":i="您输入的帐号或者密码不正确，请重新输入。";break;
                        case"-4":i="不存在该帐户。";break;
                        case"-5":i="您目前处于访问受限状态。";break;
                        case"-6":i="请输入图中的验证码",r();return;
                        case"-7":i="此帐号已绑定私人微信号，不可用于公众平台登录。";break;
                        case"-8":i="邮箱已存在。";break;
                        case"-32":i="您输入的验证码不正确，请重新输入",r();break;
                        case"-200":i="因频繁提交虚假资料，该帐号被拒绝登录。";break;
                        case"-94":i="请使用邮箱登陆。";break;
                        case"10":i="该公众会议号已经过期，无法再登录使用。";break;
                        case"65201":
                        case"65202":i="成功登陆，正在跳转...",location.href=t.ErrMsg;return;
                        case"0":i="成功登陆，正在跳转...",location.href=t.ErrMsg;return;
                        default:i="未知的返回。";return
                    }
                    s.trigger("Response",[n,i])},"json")
        },
        getVal:function(){return{account:t.trim(f.val()),password:t.trim(l.val()),verify:t.trim(h.val())}},setVal:function(e,n){return t(i).val(n).length}};return a.click(function(){m.showVerifyImg()}),d.click(m.submit),f.focus(),m}