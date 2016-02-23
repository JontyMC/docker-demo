console.log('Running tests...');
var page = require('webpage').create();
var url = 'http://hello:5004/';

page.open(url, function(status) {
  var message = page.evaluate(function() {
    return document.body.textContent;
  });
  
  if (message !== 'hi from test') {
    console.log('FAILED: message was "' + message + '"');
    phantom.exit();
  }
  
  console.log("Tests Passed!")
  phantom.exit();
});