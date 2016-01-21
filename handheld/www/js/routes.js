app.config(function($stateProvider, $urlRouterProvider) {

  $stateProvider
    .state('tabs', {
      url: "/tab",
      abstract: true,
      templateUrl: "templates/tabs.ng.html"
    })
    .state('tabs.home', {
      url: "/home",
      views: {
        'home-tab': {
          templateUrl: "templates/home.ng.html",
          controller: 'SpellTabCtrl'
        }
      }
    })
    .state('tabs.about', {
      url: "/about",
      views: {
        'about-tab': {
          templateUrl: "templates/about.ng.html"
        }
      }
    })
    .state('tabs.debug', {
      url: "/debug",
      views: {
        'debug-tab': {
          templateUrl: "templates/debug.ng.html",
          controller: 'DebugTabCtrl'
        }
      }
    })
    .state('tabs.navstack', {
      url: "/navstack",
      views: {
        'about-tab': {
          templateUrl: "templates/nav-stack.ng.html"
        }
      }
    })
   $urlRouterProvider.otherwise("/tab/home");

});
