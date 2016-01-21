app.controller("PlayerCtrl", function($scope) {
  $scope.playerHP = 30;
  $scope.playerMP = 35;
  $scope.playerName = "Gandalf";
  $scope.playerClass = "Rogue";

  $scope.ChangeHP = function(newValue) {
    $scope.playerHP = newValue;
    jQuery('.healthMeter').val($scope.playerHP).trigger('change');
  }

  $scope.ChangeMP = function(newValue) {
    $scope.playerMP = newValue;
    jQuery('.manaMeter').val($scope.playerMP).trigger('change');
  }


  jQuery(".meter").knob({
    'change' : function(v) {console.log(v);},
    'readOnly' : true
  });

  jQuery('.healthMeter').trigger(
    'configure',
    {
        "min":0,
        "max":100,
        "fgColor":"#66CC66",
        "angleOffset":-125,
        "angleArc":250,
        "displayInput":true,
        "readOnly":true
    }
  );
  jQuery('.healthMeter').val($scope.playerHP).trigger('change');

  jQuery('.manaMeter').trigger(
    'configure',
    {
        "min":0,
        "max":100,
        "fgColor":"#0000C4",
        "angleOffset":-125,
        "angleArc":250,
        "displayInput":true,
        "readOnly":true
    }
  );
  jQuery('.manaMeter').val($scope.playerMP).trigger('change');


});
