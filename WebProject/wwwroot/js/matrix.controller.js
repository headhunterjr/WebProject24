(function () {
    'use strict';

    angular
        .module('matrixApp')
        .controller('MatrixController', MatrixController);

    MatrixController.$inject = ['$scope', '$http'];

    function MatrixController($scope, $http) {
        $scope.maxMatrixSize = 10000;
        $scope.tasks = [];

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/problemHub")
            .build();

        connection.start().then(function () {
            connection.invoke("GetConnectionId").then(function (connectionId) {
                $scope.connectionId = connectionId;
            });
        }).catch(function (err) {
            return console.error(err.toString());
        });

        connection.on("ReceiveProgressUpdate", function (stage, matrixSize, taskId, result) {
            $scope.$apply(function () {
                let taskIndex = $scope.tasks.findIndex(t => t.taskId === taskId);
                if (taskIndex === -1) {
                    taskIndex = $scope.tasks.length;
                    $scope.tasks.push({ taskId: taskId, matrixSize: matrixSize, stage: stage, result: null, cancelling: false });
                }

                if (typeof result !== 'number') {
                    $scope.tasks[taskIndex].stage = stage;

                    if (stage === "Timeout" || stage === "Cancelled") {
                        $scope.tasks[taskIndex].cancelling = true;
                    }
                } else {
                    $scope.tasks[taskIndex].stage = "Complete";
                    $scope.tasks[taskIndex].result = result;
                }
            });
        });

        $scope.submitForm = function () {
            if ($scope.matrixForm.$valid && $scope.matrixSize <= $scope.maxMatrixSize) {
                var matrixSize = $scope.matrixSize;
                $scope.matrixSize = null;
                $scope.matrixForm.$setPristine();
                $scope.matrixForm.$setUntouched();

                $http({
                    method: 'POST',
                    url: '/CurrentProblem/SolveProblem',
                    data: {
                        matrixSize: matrixSize,
                        connectionId: $scope.connectionId
                    },
                    headers: { 'Content-Type': 'application/json' }
                })
                    .catch(function (error) {
                        alert("An error occurred: " + error.data);
                    });
            } else {
                alert("Please enter a valid matrix size (<= " + $scope.maxMatrixSize + ")");
            }
        };

        $scope.cancelTask = function (task) {
            task.cancelling = true;
            $http({
                method: 'POST',
                url: '/CurrentProblem/CancelProblem',
                data: {
                    taskId: task.taskId,
                    matrixSize: task.matrixSize,
                    connectionId: $scope.connectionId
                },
                headers: { 'Content-Type': 'application/json' }
            })
                .then(function () {
                    task.stage = "Cancelled";
                })
                .catch(function (error) {
                    alert("Failed to cancel task: " + error.data);
                    task.cancelling = false;
                });
        };

        $scope.removeTask = function (task) {
            let index = $scope.tasks.indexOf(task);
            if (index > -1) {
                $scope.tasks.splice(index, 1);
            }
        };
    }
})();