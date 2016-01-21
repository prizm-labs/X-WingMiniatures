window.bindButtonHandlers = bindButtonHandlers;
window.bindJoystickHandlers = bindJoystickHandlers;

function emitToSocket(socket,message) {

  if (!window.player) return;

  message.player = window.player._id;
  socket.emit('fromHandheld',message);
}

function bindButtonHandlers( elementId, socket ) {
  console.log("bindButtonHandlers");

  var element = document.getElementById(elementId);
  var manager = new Hammer.Manager(element);

  var pressThreshold = 150;

  var press1 = new Hammer.Press({
    event:'press1',
    pointers:1,
    threshold: 100,
    time: pressThreshold
  });

  var tap1 = new Hammer.Tap({
    event:'tap1',
    pointers:1,
    time: pressThreshold-1,
    threshold: 100
  });

  var pressStart = null;

  var timeToMaxSize = 2;
  var maxSize = 200;
  var defaultSize = 90;
  var maxPosition = 0;
  var defaultPosition = 55;
  /*
  width: 90px;
  height: 90px;

  width: 200px;
  height: 200px;
  */

  // tap1.recognizeWith(press1);
  // press1.requireFailure(tap1);

  manager.add([press1,tap1]);

  manager.on("tap1",function(event){
    console.log("tap1",event);
    $(event.target).css({background:"orange"});

    emitToSocket(socket,{'type':'fireQuick'});
    //socket.emit('fromHandheld',{'type':'fireQuick','player':1});

    endCharge(event);

    if (manager.currentTimeout) clearTimeout(manager.currentTimeout);

    manager.currentTimeout = setTimeout(function(){
      $(event.target).css({background:"red"});
    },50);
  });

  manager.on("press1",function(event){
    console.log("press1",event);

    pressStart = new Date();

    targetSize = maxSize+"px";
    targetSize2 = "50%";
    manager.currentTween = TweenLite.to(event.target, timeToMaxSize, {
      "height":maxSize, "width":maxSize,
      "top":maxPosition,"left":maxPosition,
      background:"orange",
      // "-moz-border-radius": targetSize2,
      // "-webkit-border-radius": targetSize2,
      "borderRadius": targetSize2,
      onComplete: function() {
        // if press up has not fired yet
        if (pressStart!==null) {
          endCharge(event);
        }
      }
    });

    //socket.emit('fromHandheld',{'type':'startCharge','player':1});
    emitToSocket(socket,{'type':'startCharge'});
  });

  manager.on("press1up",function(event){
    console.log("press1up",event);
    
    
    endCharge(event);
  });

  function endCharge(event){
    if (manager.currentTween) {
      manager.currentTween.kill();
      //socket.emit('fromHandheld',{'type':'endCharge','player':1});
      emitToSocket(socket,{'type':'endCharge'});

      manager.currentTween = null;
      pressStart = null;

      $(event.target).css({
        height:defaultSize,width:defaultSize,
        "top":defaultPosition,"left":defaultPosition,
        background:"red"
      });
    }    
  }
}

function bindJoystickHandlers( elementId, socket ) {
  console.log("bindJoystickHandlers");

  var element = document.getElementById(elementId);
  var manager = new Hammer.Manager(element);

  var defaultPosition = 55;
  var maxDelta = 80;

  var pan1 = new Hammer.Pan({
    event: 'pan1',
    pointers: 1,
    direction: Hammer.DIRECTION_ALL
  });

  var rotateA = new Hammer.Rotate({
    event:'rotate',
    pointers: 2
  });

  rotateA.recognizeWith(pan1);
  pan1.requireFailure(rotateA);

  manager.add([rotateA,pan1]);

  manager.currentAngle = 0;

  manager.on("pan1start pan2start", function(ev) {
    moveStart(ev);
  });

  manager.on("pan1move pan2move", function(ev) {
    moveUpdate(ev);

    //updatePositionDelta(ev);
    updatePositionDeltaByAbsolutePositionControlScheme(ev);
  });

  manager.on("pan1end pan2end", function(ev) {

    //impartMomentum(ev);
  });

  manager.on("rotatestart",function(ev){
    manager.rotationLast = ev.rotation;
    moveStart(ev);
  });

  manager.on("rotatemove",function(ev){

    var isCW = ev.rotation > manager.rotationLast;

    var delta = Math.abs(ev.rotation-manager.rotationLast);

    // depending on the order of touches
    // ev.rotation jumps from ~-50 to ~300
    if (delta>100) delta=5; // handle ev.rotation jump

    if (!isCW) {
      manager.currentAngle-=delta;
    } else {
      manager.currentAngle+=delta;
    }

    performRotation(ev.target,manager.currentAngle,ev);

    manager.rotationLast = ev.rotation;
  });

  manager.on("rotateend",function(ev){

  });

  function updatePositionDelta(ev) {

    var yDelta = -(parseInt($(ev.target).css("top"))-defaultPosition);
    var xDelta = parseInt($(ev.target).css("left"))-defaultPosition;

    //console.log(xDelta,yDelta);

    var speed, steering;

    yDelta = Math.min(Math.abs(yDelta),maxDelta)==Math.abs(yDelta) ? yDelta : yDelta>1 ? maxDelta : -maxDelta;
    xDelta = Math.min(Math.abs(xDelta),maxDelta)==Math.abs(xDelta) ? xDelta : xDelta>1 ? maxDelta : -maxDelta;
    
    // speed on Y-axis
    // -1 reverse <> forward 1
    speed = parseInt(yDelta/maxDelta*100);

    // steering on X-axis
    // -1 left <> right 1
    steering = parseInt(xDelta/maxDelta*100);

    console.log(speed,steering);

    emitToSocket(socket,{type:"navigation",speed:speed,direction:steering});
    // socket.emit('fromHandheld', {type:"navigation",speed:speed,direction:steering}, function (data) {
    //   console.log(data); // data will be 'woot'
    // });

  }

  function updatePositionDeltaByAbsolutePositionControlScheme(ev) {

    var yDelta = -(parseInt($(ev.target).css("top"))-defaultPosition);
    var xDelta = parseInt($(ev.target).css("left"))-defaultPosition;

    //console.log(xDelta,yDelta);

    var length, angle;

    yDelta = Math.min(Math.abs(yDelta),maxDelta)==Math.abs(yDelta) ? yDelta : yDelta>1 ? maxDelta : -maxDelta;
    xDelta = Math.min(Math.abs(xDelta),maxDelta)==Math.abs(xDelta) ? xDelta : xDelta>1 ? maxDelta : -maxDelta;
    
    // length of hypotenuse
    length = Math.sqrt(Math.pow(xDelta,2)+Math.pow(yDelta,2));

    length = parseInt(length/maxDelta)*100;

    // angle = arctan x[opposite] / y[adjacent]
    angle = Math.atan(xDelta/yDelta);

    if (xDelta>1&&yDelta>1) {
      console.log("Q I");

    } else if (xDelta>1&&yDelta<1) {
      console.log("Q II");
      angle = Math.PI+angle;

    } else if (xDelta<1&&yDelta<1) {
      console.log("Q III");
      angle = Math.PI+angle;

    } else if (xDelta<1&&yDelta>1) {
      console.log("Q IV");
      angle = Math.PI*2+angle;
    }

    console.log(length, angle);

    emitToSocket(socket,{type:"navigation",speed:length,angle:angle});
    // socket.emit('fromHandheld', {type:"navigation",speed:speed,direction:steering}, function (data) {
    //   console.log(data); // data will be 'woot'
    // });

  }


  function moveStart(ev) {
    manager.lastX = ev.center.x;
    manager.lastY = ev.center.y;
    if (manager.currentTween) manager.currentTween.kill();
  }

  function moveUpdate(ev) {
    var deltaX = manager.lastX-ev.center.x;
    var deltaY = manager.lastY-ev.center.y;

   var newX = parseInt(ev.target.offsetLeft)-deltaX;
   var newY = parseInt(ev.target.offsetTop)-deltaY;

      $(ev.target).css({
        "left":newX,
        "top":newY
      })

    manager.lastX = ev.center.x;
    manager.lastY = ev.center.y;
  }


  function performRotation(target,angle,ev) {

    this.options = {

      rotationCenterX: 50,
      rotationCenterY: 50
    };

    this.element = $(target);

    moveUpdate(ev);

    this.element.css('transform-origin', this.options.rotationCenterX + '% ' + this.options.rotationCenterY + '%');
    this.element.css('-ms-transform-origin', this.options.rotationCenterX + '% ' + this.options.rotationCenterY + '%'); /* IE 9 */
    this.element.css(
      '-webkit-transform-origin',
      this.options.rotationCenterX + '% ' + this.options.rotationCenterY + '%'); /* Chrome, Safari, Opera */

    this.element.css('transform','rotate(' + angle + 'deg)');
    this.element.css('-moz-transform','rotate(' + angle + 'deg)');
    this.element.css('-webkit-transform','rotate(' + angle + 'deg)');
    this.element.css('-o-transform','rotate(' + angle + 'deg)');
  }

  function impartMomentum(ev) {
    var v = ev.velocity;
    var vX = Math.abs(ev.velocityX)<Math.abs(ev.overallVelocityX) ? ev.overallVelocityX : ev.velocityX;
    var vY = Math.abs(ev.velocityY)<Math.abs(ev.overallVelocityY) ? ev.overallVelocityY : ev.velocityY;
    var decel = 1; // distance per second squared
    var time = Math.abs(v) / decel;

    var coefficient = 300;
    // d = v^2 / 2a

    // get time travelled
    // get destination x
    var dX = Math.pow(vX,2) / (2*decel) * coefficient;
    dX = (ev.velocityX>0) ? dX : -dX;
    // get destination y
    var dY = Math.pow(vY,2) / (2*decel) * coefficient;
    dY = (ev.velocityY>0) ? dY : -dY;

    var finalX = ev.target.offsetLeft+dX, finalY = ev.target.offsetTop+dY;

    // get animation handler to cancel on new event start
    manager.currentTween = TweenLite.to(ev.target, time, {left:finalX+"px",top:finalY+"px",
      onUpdate: function(){
        // prevent object from leaving bounds
        if ((ev.target.offsetLeft<=0) || (ev.target.offsetTop<=0)) {
           manager.currentTween.kill();
        }
      }
    });
  }

}
