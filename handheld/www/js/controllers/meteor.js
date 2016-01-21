// Connect to a Meteor backend
var ceres = new Asteroid(window.meteorHost);

window.Asteroid = ceres;

// Use real-time collections
ceres.subscribe("handheldBootstrap");

players = ceres.getCollection("players");

playersRQ = players.reactiveQuery({});
playersRQ.on("added", function (id) {
  console.log(id,playersRQ.result);
});

playersRQ.on("changed", function (id) {
  console.log(id,playersRQ.result);
});

sessions = ceres.getCollection("sessions");

sessionsRQ = sessions.reactiveQuery({});
sessionsRQ.on("added", function (id) {
  console.log(id,sessionsRQ.result);

  window.session = sessionsRQ.result[0];
});

function createPlayer() {

  var name = chance.name();
  var color = [newColor(),newColor(),newColor()];

  var insertOperation = players.insert({
    "name":name,"colorRGB":color,
    active:true,
    session_id:window.session._id
  });

  insertOperation.remote.then(function(_id){

    var playerDoc = ceres.collections.players._set._items[_id];
    console.log(_id,playerDoc);
    window.player = playerDoc;
  });

  function newColor  () {

    return chance.integer({min: 0, max: 100})/100;
  }
}

function deactivatePlayer() {

  var playerDoc = window.player;
  playerDoc.active = false;

  var updateOperation = players.update(window.player._id,playerDoc);

  updateOperation.remote.then(function(_id){
    console.log("after update",_id);
  });
}

//ceres.channels.players

// tasks.insert({
//   description: "Do the laundry"
// });
// Get the task
// var laundryTaskRQ = tasks.reactiveQuery({description: "Do the laundry"});
// // Log the array of results
// console.log(laundryTaskRQ.result);
// // Listen for changes
// laundryTaskRQ.on("change", function () {
//   console.log(laundryTaskRQ.result);
// });
