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

