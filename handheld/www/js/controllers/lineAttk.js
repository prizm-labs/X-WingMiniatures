app.controller('LineAttkCtrl', function($scope, $ionicModal) {
  $scope.classUp = "unselectedDirection";
  $scope.classDown = "unselectedDirection";
  $scope.classLeft = "unselectedDirection";
  $scope.classRight = "unselectedDirection";

  $scope.ChooseAttack = function(chosen) {
    $scope.classUp = "unselectedDirection";
    $scope.classDown = "unselectedDirection";
    $scope.classLeft = "unselectedDirection";
    $scope.classRight = "unselectedDirection";

    switch (chosen) {
      case "chooseUp":
        $scope.classUp == "unselectedDirection" ? $scope.classUp = "selectedDirection" : $scope.classUp = "unselectedDirection";
        break;
      case "chooseDown":
        $scope.classDown == "unselectedDirection" ? $scope.classDown = "selectedDirection" : $scope.classDown = "unselectedDirection";
        break;
      case "chooseLeft":
        $scope.classLeft == "unselectedDirection" ? $scope.classLeft = "selectedDirection" : $scope.classLeft = "unselectedDirection";
        break;
      case "chooseRight":
        $scope.classRight == "unselectedDirection" ? $scope.classRight = "selectedDirection" : $scope.classRight = "unselectedDirection";
        break;
      default:
        console.log("chosen direction not one of {up, down, left, right}");
        break;
    }
  }

  $scope.LaunchAttack = function() {
    //put the meteor call here
    console.log("launching attack, special for LINESSS");
    //throw in the chosen direction here
    $scope.closeModal();
  }
});
