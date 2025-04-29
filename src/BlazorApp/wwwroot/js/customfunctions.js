//TODO -> Funktion hier anlegen, im _Host.cshtml referenzieren mit dem script Tag, anschließend können die Funktionen in Blazor mit JSInvoke aufgerufen werden 
////function funcName() {

////}

function focusInput(name) {
    element = document.getElementById(name);
    element.focus();
}

function focusandSelectInput(name) {
    document.getElementById(name).select();
}

function openFilePicker(name) {
    var fileinput = document.getElementById('FileUploadInput');

    fileinput.click();
}

function getUserAgent() {
    return navigator.userAgent;
}

function getTest() {
    return navigator.appName + ' 00 ' + navigator.appVersion + ' 00 ' + navigator.appCodeName + ' 00 ' + navigator.platform + ' 00 ' + navigator.language + ' 00 ' + navigator.product + ' 00 ' + navigator.productSub + ' 00 ' + navigator.vendor + ' 00 ' + navigator.vendorSub;
}
