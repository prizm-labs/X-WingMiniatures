app.controller('DebugTabCtrl', function($scope, $ionicModal, socket) {

  console.log('DebugTabCtrl');

  $scope.player = {
    _id: "1234",
    name: "John"
  };

  $scope.manuevers = {
    ""
  }

  $scope.ships = [
    {
      "_id":"001",
      "class":"wizard",
      "abilities":["coneofcold","fireball","magicmissile"]
    },
    {
      "_id":"002",
      "class":"warrior",
      "abilities":["axe-melee","axe-ranged"]
    },
    {
      "_id":"003",
      "class":"cleric",
      "abilities":["heal","hammer"]
    },
    {
      "_id":"004",
      "class":"ranger",
      "abilities":["arrow","dual-swords"]
    }
  ];

  // select faction
  // select ship
  // select pilot
  // select upgrades

  // start Game
  // select manuevers

  $scope.targetSelected = false;

  $scope.chooseCharacter = chooseCharacter;

  $scope.sendCharacterData = sendCharacterData;
  $scope.startTurn = startTurn;
  $scope.sendAbilitySelection = sendAbilitySelection;
  $scope.sendSingleTargetSelection = sendSingleTargetSelection;
  $scope.sendAbilityCancellation = sendAbilityCancellation;
  $scope.sendTargetConfirmation = sendTargetConfirmation;


  function chooseCharacter(id) {
    var topic = 'character-select';
    $scope.selectedCharacter = $scope.characters[id];
    $scope.selectedCharacterKey = id;

    socket.sendToTabletop(topic,{
      "player_id":$scope.player._id,
      "character_id":$scope.selectedCharacter._id
    });
  }

  function sendCharacterData(id){
    var topic = 'character-data';
    socket.sendToTabletop(topic,$scope.characters[id]);
  }

  function startTurn() {
    var topic = "start-turn";
    socket.sendToTabletop(topic,$scope.selectedCharacter._id);
  }

  function sendAbilitySelection(id) {
    var topic = "ability-select";
    $scope.selectedAbility = id;
    socket.sendToTabletop(topic,id);
    // receive valid targets
  }

  function showAbilityTargets(data) {
    $scope.targetType = data.type;

     switch (data.type) {

       case "single":
        $scope.targets = data.targets;
        break;
     }

  }

  function sendSingleTargetSelection(id) {
    var topic = "target-select";
    socket.sendToTabletop(topic,id);

    $scope.targetSelected = true;
    // receive confirmation of selection
  }

  function sendTargetConfirmation() {
    var topic = "target-confirm";
    socket.sendToTabletop(topic,$scope.selectedCharacterKey);
  }

  function sendAbilityCancellation() {
    var topic = "ability-cancel";
    socket.sendToTabletop(topic,$scope.selectedAbility);

    $scope.selectedAbility = null;
    $scope.targetSelected = false;
  }


});
