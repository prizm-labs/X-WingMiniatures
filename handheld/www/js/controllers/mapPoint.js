app.controller("MapPointCtrl", function($scope, $ionicModal) {

      jQuery( "#selectable" ).selectable({
        selected: function(event, ui) {
            alert("Selected");
        },
        selecting: function(event, ui) {
            alert("Selecting");
        }
      });
});
