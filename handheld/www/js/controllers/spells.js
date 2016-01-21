//main controller
app.controller('SpellTabCtrl', function($scope, $ionicModal, socket) {

  var rpc = Asteroid.call("getSpellsData");

  rpc.result.then(function(result) {

      console.log('TRPG JSON loaded');

      $scope.$apply(function(){
        $scope.spells = result;
      })

  }).catch(function(error){
    if (error) {
      console.log("error:", error);
    }
  })

  $scope.availableTargets = [
    {'name':'Jason Statham', 'hp':'trillions', 'imgSrc':'img/jason.jpg', 'selected':false, 'allocated':0},
    {'name':'Billford Billiams', 'hp':10, 'imgSrc':'img/batman.jpg', 'selected':false, 'allocated':0},
    {'name':'Sparky the WonderDog', 'hp':79, 'imgSrc':'img/sparky.jpg', 'selected':false, 'allocated':0},
  ];

  $scope.selected = {
    'name':'Billford Billiams'
  };

  $scope.possibleDamages = 2; //the number of targets this spell can target

  $scope.addDamage = function(targetName) {
    for (var i = 0; i < $scope.availableTargets.length; i++) {
      if ($scope.availableTargets[i].name == targetName && $scope.possibleDamages > 0)
      {
        $scope.availableTargets[i].allocated = $scope.availableTargets[i].allocated + 1;
        $scope.possibleDamages--;
      }
      if ($scope.availableTargets[i].allocated > 0)
      {
        $scope.availableTargets[i].selected = true;
      }
      if ($scope.availableTargets[i].allocated <= 0)
      {
        $scope.availableTargets[i].selected = false;
      }

    }
  }
  $scope.loseDamage = function(targetName) {

    for (var i = 0; i < $scope.availableTargets.length; i++) {
      if ($scope.availableTargets[i].name == targetName && $scope.availableTargets[i].allocated > 0)
      {
        $scope.availableTargets[i].allocated = $scope.availableTargets[i].allocated - 1;
        $scope.possibleDamages++;
      }
      if ($scope.availableTargets[i].allocated > 0)
      {
        $scope.availableTargets[i].selected = true;
      }
      if ($scope.availableTargets[i].allocated <= 0)
      {
        $scope.availableTargets[i].selected = false;
      }
    }
  }

  $scope.toggleMultiSelected = function(targetName) {
    for (var i = 0; i < $scope.availableTargets.length; i++) {
      if ($scope.availableTargets[i].name == targetName)
      {
        if ($scope.availableTargets[i].selected == true && $scope.availableTargets[i].allocated == 1) {
          $scope.availableTargets[i].selected = false;
          $scope.possibleDamages++;
          $scope.availableTargets[i].allocated = 0;

        } else if ( $scope.availableTargets[i].selected == false && $scope.possibleDamages < 1 ) {
          jQuery(".list").effect("shake", {times:4}, 20);  //shake the list if they try to select more

        } else if ($scope.availableTargets[i].selected == false){  //if the target is not currently selected
          $scope.availableTargets[i].selected = true;
          $scope.possibleDamages--;
          $scope.availableTargets[i].allocated = $scope.availableTargets[i].allocated + 1;
        }
      }
    }
    console.log("have toggled: " + targetName);
  };

  // $ionicModal.fromTemplateUrl('templates/attackPatterns/mapPoint.ng.html', {
  //     scope: $scope,
  //     animation: 'slide-in-up'
  //   }).then(function(modal) {
  //     $scope.modal = modal;
  //   });

  $scope.modals = {};

  targetingPatterns = ['angle','line','mapPoint','multi','single'];

  _.each(targetingPatterns,function(targetingPattern){
    $ionicModal.fromTemplateUrl('templates/attackPatterns/' + targetingPattern + '.ng.html', {
        scope: $scope,
        animation: 'slide-in-up'
      }).then(function(modal) {
        $scope.modals[targetingPattern] = modal;
        console.log(targetingPattern,$scope.modals);
      });
  })



  $scope.openModal = function(targetingPattern){

        $scope.modal = $scope.modals[targetingPattern];
        $scope.modal.show();

        if (targetingPattern=='angle') {
          jQuery(".angleTargeting").knob({
            'change' : function(newValue) {
              console.log(newValue);
            }
          });
          jQuery('.angleTargeting').trigger(
            'configure',
            {
                "width":200,
                "min":0,
                "max":100,
                "fgColor":"#222222",
                "angleOffset":-125,
                "thickness":".5",
                "angleArc":250,
                "value":50,
                "cursor":30
            }
          );
          jQuery('.angleTargeting').val(50).trigger('change');

        }




    console.log(targetingPattern);
  }

  $scope.closeModal = function(){
    $scope.modal.hide();
  }

  $scope.filterSpells = function (obj, idx){
    return !((obj._index = idx) % 2); //2 columns of spells
  }

  $scope.LaunchAttack = function() {
    //put the meteor call here
    console.log("launching attack!");

    //clean up variables
    for (var i = 0; i < $scope.availableTargets.length; i++) {  //clear the selected targets
      console.log($scope.availableTargets[i].name + " is: " + $scope.availableTargets[i].selected);
      $scope.availableTargets[i].selected = false;
    }
    $scope.closeModal();
  }

  $scope.CancelAttack = function() {
    //put the meteor call here
    console.log("canceling Attack");

    for (var i = 0; i < $scope.availableTargets.length; i++) {  //clear the selected targets
      console.log($scope.availableTargets[i].name + " is: " + $scope.availableTargets[i].selected);
      $scope.availableTargets[i].selected = false;
    }
    $scope.closeModal();
  }
});
