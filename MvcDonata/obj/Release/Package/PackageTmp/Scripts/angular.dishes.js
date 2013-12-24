var dishesModule=angular.module("dishesModule", []);
dishesModule.filter('actived', function () {
    return function (input) {
        return input ? "激活" : "禁用";
    };
}
);
dishesModule.filter('discounted', function () {
    return function (input) {
        return input ? "是" : "否";
    };
}
);

dishesModule.controller("dishesController", ['$scope', '$http',
    function ($scope, $http) {
        

        function getData()
        {
            $http.get("/dishes/getdishes", { params: { all: true } })
        .success(
            function (data) {
                $scope.dishes = JSON.parse(data.result);
                $scope.classNames = Enumerable.From($scope.dishes)
            .Select("$.ClassName").Distinct().ToArray();
                $scope.classNames.unshift("全部");//插入到数组的头部
                $scope.curClassName = "全部";
            }
        )
        .error(
            function (data, status, headers, config) {
                alert("获取餐品数据出错,请重试!");
            }
        );
        }

        getData();

        $scope.refresh = function () {
            getData();
        };

        $scope.isAlt = function ($index) {
            return $index % 2 == 0;
        };

        $scope.edit = function ($index) {
            $scope.cur = {};
            $scope.index = $index;
            for (var a in $scope.dishes[$index])
            {
                $scope.cur[a] = $scope.dishes[$index][a];
            }
            $("#dialog").dialog({ modal: true, title: "修改"+$scope.cur.Name });
        };

        $scope.saveDish = function () {
            var data = JSON.stringify([$scope.cur]);
            $http.post("/dishes/uploaddishes/", { datas: data })
            .success(function()
            {
                for (var a in $scope.dishes[$scope.index]) {
                    $scope.dishes[$scope.index][a] = $scope.cur[a];
                }
                $("#dialog").dialog('close');
            }
            ).error(function()
            {
                alert("保存错误,请重试!");
            }
            );
        };
        $scope.cancel = function () {
            $scope.cur = null;
            $("#dialog").dialog('close');
        };

        $scope.isShow=function($index)
        {
            if ($scope.curClassName == "全部")
            {
                if($scope.displayAll)
                {
                    return true;
                }
                else
                {
                    return $scope.dishes[$index].Actived;
                }
            }
            else
            {
                if ($scope.dishes[$index].ClassName != $scope.curClassName)
                    return false;
                if ($scope.displayAll) {
                    return true;
                }
                else {
                    return $scope.dishes[$index].Actived;
                }
            }
            return true;
        }
    }
]);


