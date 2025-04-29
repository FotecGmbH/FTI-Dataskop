function collapseNavbar() {
    if ($(document.getElementById('navbar-button')).css('display') == "block") {
        var collapseElementList = [].slice.call(document.querySelectorAll('#sidebarMenu'));
        var collapseList = collapseElementList.map(function (collapseEl) {
            return new bootstrap.Collapse(collapseEl);
        });
    }
}