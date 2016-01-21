angular.module('combatApp.services', ['btford.socket-io'])

.factory('socket',function(socketFactory){
  //Create socket and connect to http://chat.socket.io
  var ioSocket = io.connect(window.websocketHost);

  var ngSocket = socketFactory({
   ioSocket: ioSocket
  });

  //ngSocket.forward(['error','connect','message','toTabletop']);
  ngSocket.forward(['error','connect','message']);

  window.ioSocket = ioSocket;

  ngSocket.sendToTabletop = function(topic,data){
    console.log('ngSocket.sendToTabletop',topic,data);

    var payload = { "event": topic, "data": data };
    ngSocket.emit('fromHandheld',payload);
  }

  return ngSocket;
})

.factory('Chats', function() {
  // Might use a resource here that returns a JSON array

  // Some fake testing data
  var chats = [{
    id: 0,
    name: 'Ben Sparrow',
    lastText: 'You on your way?',
    face: 'img/ben.png'
  }, {
    id: 1,
    name: 'Max Lynx',
    lastText: 'Hey, it\'s me',
    face: 'img/max.png'
  }, {
    id: 2,
    name: 'Adam Bradleyson',
    lastText: 'I should buy a boat',
    face: 'img/adam.jpg'
  }, {
    id: 3,
    name: 'Perry Governor',
    lastText: 'Look at my mukluks!',
    face: 'img/perry.png'
  }, {
    id: 4,
    name: 'Mike Harringtonz',
    lastText: 'This is wicked good ice cream.',
    face: 'img/mike.png'
  }];

  return {
    all: function() {
      return chats;
    },
    remove: function(chat) {
      chats.splice(chats.indexOf(chat), 1);
    },
    get: function(chatId) {
      for (var i = 0; i < chats.length; i++) {
        if (chats[i].id === parseInt(chatId)) {
          return chats[i];
        }
      }
      return null;
    }
  };
});
