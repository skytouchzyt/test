jQuery(document).ready(
    function () {

        alert("this is a test");

        $("#account").val("597588445@qq.com");
        $("#password").val("zytzly97");
        


        var count=0;
        setInterval(function () {
            count++;
            $("a:contains('多纳达')").html("多纳达比萨:" + count);
        },
        1000
        );
    }
    );