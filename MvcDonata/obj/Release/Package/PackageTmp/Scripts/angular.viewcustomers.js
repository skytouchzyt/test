var dishesModule=angular.module("customersModule", []);
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

dishesModule.controller("customersController", ['$scope', '$http',
    function ($scope, $http) {
        var index = 0;
        var start = localStorage.startNumber | "000000";
        $scope.customers = [];
        var getData = function () {
            $http.get("/customers/calcnextcustomerscore", { params: { startNumber: start } })
            .success(function (data) {
                if(data.successed)
                {
                    var c=JSON.parse(data.result);
                    $scope.customers[index++ % 10] = c;
                    start = c.VIP_Number;
                    getData();
                }
            }
            );
        };

        $scope.page = 0;

        function getPage()
        {
            if (!$scope.search)
            {
                $scope.customers = Enumerable.From($scope.datas).OrderByDescending("$.Score").Skip($scope.page * 10).Take(10).ToArray();
            }
            else
            {
                $scope.customers = Enumerable.From($scope.datas).Where("$.VIP_Number.indexOf('" + $scope.search + "')>=0").ToArray();
            }
                
        }

        $scope.searchPage = function () {
            getPage();
        };

        $http.get("/customers/getcustomersorderbyscore")
        .success(function (data) {
            $scope.datas = data;
            $scope.pages = data.length / 10;
            getPage();
        }
        );

        $scope.pre = function () {
            if ($scope.page == 0)
                return;
            $scope.page--;
            getPage();
        };
        $scope.next = function () {
            if ($scope.page*10>=$scope.datas.length)
                return;
            $scope.page++;
            getPage();
        };
    }]
    );