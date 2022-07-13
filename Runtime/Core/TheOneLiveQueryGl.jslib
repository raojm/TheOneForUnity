
mergeInto(LibraryManager.library, {
  TheOneLiveQueries: function () {
  },
  
  OpenWebsocketJs: function (key, path) {
    window.theoneLiveQueries.openSocket(Pointer_stringify(key), Pointer_stringify(path));
  },
  
  OpenWebsocketResponse: function () {
    var bufferSize = lengthBytesUTF8(window.theoneLiveQueries.openSocketResponse) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(window.theoneLiveQueries.openSocketResponse, buffer, bufferSize);
    return buffer; 
  },
  
  CloseWebsocketJs: function (key) {
    console.log("mlqgl ... CloseWebsocketJs called.");
    window.theoneLiveQueries.closeSocket(Pointer_stringify(key));
  },
  
  CloseWebsocketResponse: function () {
    var bufferSize = lengthBytesUTF8(window.theoneLiveQueries.closeSocketResponse) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(window.theoneLiveQueries.closeSocketResponse, buffer, bufferSize);
    return buffer; 
  },
  
  SendMessageJs: function (key, message) {
    window.theoneLiveQueries.sendRequest(Pointer_stringify(key), Pointer_stringify(message));
  },

  GetErrorQueueJs: function (key) {
    console.log("mlqgl ... GetErrorQueueJs called.");
    var errors = window.theoneLiveQueries.getErrors(Pointer_stringify(key));
    var errorString = JSON.stringify(errors);
    var bufferSize = lengthBytesUTF8(errorString) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(errorString, buffer, bufferSize);
    return buffer; 
  },

  GetResponseQueueJs: function (key) {
    var msgs = window.theoneLiveQueries.getMessages(Pointer_stringify(key));
    var respString = JSON.stringify(msgs); 
    var bufferSize = lengthBytesUTF8(respString) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(respString, buffer, bufferSize);
    return buffer; 
  },

  GetSocketStateJs: function (key) {
    return window.theoneLiveQueries.getSocketState(Pointer_stringify(key));
  }
});
