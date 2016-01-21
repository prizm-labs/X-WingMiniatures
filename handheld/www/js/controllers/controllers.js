angular.module('starter.controllers', ['btford.socket-io'])

.controller('DashCtrl', function($scope,$ionicModal,socket) {

  document.addEventListener("deviceready", onDeviceReady, false);
    function onDeviceReady()
    {
        $scope.changeOriantationLandspace = function() {
            screen.lockOrientation('landscape');
        }

        $scope.changeOriantationPortrait = function() {
            screen.lockOrientation('portrait');
        }
    }

    var modalInitialized = false;

    $scope.$on('socket:connect', function (ev, data) {
      console.log(ev,data);
    });

    $scope.$on('socket:error', function (ev, data) {
      console.log(ev,data);
    });

    $scope.$on('socket:toTabletop', function (ev, data) {
      //console.log(ev,data);
    });


    function bootstrapInteractions() {

      window.createPlayer();

      // window.ioSocket.emit('fromHandheld', 'tobi', function (data) {
      //   console.log(data); // data will be 'woot'
      // });

      bindJoystickHandlers('joystick-knob',window.ioSocket);
      bindButtonHandlers('button-center',window.ioSocket);
    }

    $ionicModal.fromTemplateUrl('templates/controller-modal.html', {
      scope: $scope,
      animation: 'slide-in-up'
    }).then(function(modal) {
      console.log('modal loaded');
      $scope.modal = modal;
    });
    $scope.openModal = function() {
      $scope.modal.show();
    };
    $scope.closeModal = function() {
      $scope.modal.hide();
    };
    //Cleanup the modal when we're done with it!
    $scope.$on('$destroy', function() {
      $scope.modal.remove();
    });

    // Execute action on hide modal
    $scope.$on('modal.shown', function() {
      // Execute action
      console.log("modal.show");

      if (!modalInitialized) {
        bootstrapInteractions();
        modalInitialized = true;
      }
    });

    // Execute action on hide modal
    $scope.$on('modal.hidden', function() {
      // Execute action
      console.log("modal.hidden");
      deactivatePlayer();
    });
    // Execute action on remove modal
    $scope.$on('modal.removed', function() {
      // Execute action
      console.log("modal.removed");
    });
})

.controller('ChatsCtrl', function($scope, Chats) {
  // With the new view caching in Ionic, Controllers are only called
  // when they are recreated or on app start, instead of every page change.
  // To listen for when this page is active (for example, to refresh data),
  // listen for the $ionicView.enter event:
  //
  //$scope.$on('$ionicView.enter', function(e) {
  //});

  $scope.chats = Chats.all();
  $scope.remove = function(chat) {
    Chats.remove(chat);
  };
})

.controller('ChatDetailCtrl', function($scope, $stateParams, Chats) {
  $scope.chat = Chats.get($stateParams.chatId);
})

.controller('AccountCtrl', function($scope) {
  $scope.settings = {
    enableFriends: true
  };
});
